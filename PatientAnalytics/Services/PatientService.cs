using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Hubs;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services;

public class PatientService
{
    private readonly Context _context;
    private readonly JwtService _jwtService;
    private readonly IHubContext<PatientHub> _hubContext;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public PatientService(
        Context context,
        JwtService jwtService,
        IHubContext<PatientHub> hubContext, 
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _context = context;
        _jwtService = jwtService;
        _hubContext = hubContext;
        _localized = localized;
    }

    public Patient GetPatientById(string token, int patientId)
    {
        ValidateIsDoctor(token, out var user);
        
        var patient = _context
            .Patients
            .Include(p => p.BloodPressures)
            .Include(p => p.Temperatures)
            .Include(p => p.Heights)
            .Include(p => p.Weights)
            .FirstOrDefault(p => p.Id == patientId);

        if (patient is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, 
                string.Format(_localized["PatientError_NotFound"], patientId));
        }

        if (patient.DoctorId != user.Id)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                string.Format(_localized["PatientError_Forbidden"], patientId));
        }
        
        return patient;
    }

    public List<Patient> GetPatients(string token, string? email, string? name, string? address)
    {
        ValidateIsDoctor(token, out var user);

        var query = _context.Patients.Where(p => p.DoctorId == user.Id);
        
        if (email is not null)
        {
            query = query.Where(p => p.Email.ToLower().Contains(email.ToLower()));
        }
        
        if (name is not null)
        {
            query = query
                .Where(p => 
                    p.FirstName != null && p.FirstName.ToLower().Contains(name.ToLower()) || 
                    p.LastName != null && p.LastName.ToLower().Contains(name.ToLower()) || 
                    p.FirstName != null && p.LastName != null && (p.FirstName + " " + p.LastName).ToLower().Contains(name.ToLower()));
        }

        if (address is not null)
        {
            query = query.Where(p => p.Address != null && p.Address.ToLower().Contains(address.ToLower()));
        }

        return query.ToList();
    }
    
    public async Task<Patient?> CreatePatient(string token, PersonPayload payload)
    {
        ValidateIsDoctor(token, out var user);

        if (_context.Patients.FirstOrDefault(p => p.Email == payload.Email) is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                string.Format(_localized["RepeatedError_Email"], payload.Email));
        }
        
        var patient = Patient.CreatePatient(user.Id, payload);
        
        _context.Patients.Add(patient);

        await _context.SaveChangesAsync();

        var userNameIdentifier = _jwtService.DecodeJwt(token)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userNameIdentifier is not null)
        { 
            await _hubContext.Clients.User(userNameIdentifier).SendAsync("ReceiveNewPatient", patient);
        }

        return patient;
    }

    public async Task<Patient> EditPatient(string token, int patientId, PersonPayload payload)
    {
        ValidateCrudPermission(token, patientId, out var patient, out _);

        var patientWithIdenticalEmail = _context.Patients.FirstOrDefault(p => p.Email == payload.Email);

        if (patientWithIdenticalEmail is not null && patientWithIdenticalEmail != patient)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                string.Format(_localized["RepeatedError_Email"], payload.Email));
        }

        patient.UpdatePatient(payload);

        _context.Patients.Update(patient);
        
        await _context.SaveChangesAsync();

        var userNameIdentifier = _jwtService.DecodeJwt(token)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userNameIdentifier is not null)
        {
            await _hubContext.Clients.User(userNameIdentifier).SendAsync("ReceiveUpdatedPatient", patient);
        }
        
        return patient;
    }
    
    public async Task<IActionResult> DeletePatient(string token, int patientId)
    {
        ValidateCrudPermission(token, patientId, out var patient, out _);

        // TODO: Delete Medical Records in Metrics API when Deleting Patient entirely
        await _context.Patients.Where(p => p.Id == patientId).ExecuteDeleteAsync();

        await _context.SaveChangesAsync();

        var userNameIdentifier = _jwtService.DecodeJwt(token)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userNameIdentifier is not null)
        {
            await _hubContext.Clients.User(userNameIdentifier).SendAsync("ReceiveDeletedPatient", patient);
        }
        
        return new NoContentResult();
    }

    public void ValidateCrudPermission(string token, int patientId, out Patient verifiedPatient, out User verifiedDoctor)
    {
        ValidateIsDoctor(token, out var user);
        
        var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);

        if (patient is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, 
                string.Format(_localized["PatientError_NotFound"], patientId));
        }

        if (patient.DoctorId != user.Id)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                string.Format(_localized["PatientError_Forbidden"], patientId));
        }

        verifiedPatient = patient;
        verifiedDoctor = user;
    }

    public void ValidateIsDoctor(string token, out User verifiedUser)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                _localized["AuthError_Unauthorized"]);
        }

        verifiedUser = user;
    }
}