using PatientAnalytics.Middleware;
using PatientAnalytics.Models;

namespace PatientAnalytics.Services;

public class PatientService
{
    private readonly JwtService _jwtService;
    private readonly Context _context;

    public PatientService(JwtService jwtService, Context context)
    {
        _jwtService = jwtService;
        _context = context;
    }

    public Patient GetPatientById(string token, int patientId)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

        var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);

        if (patient is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, $"Unable to find patient with id: {patientId}");
        }

        if (patient.DoctorId != user.Id)
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You can only access patients registered to you");
        }
        
        return patient;
    }

    public List<Patient> GetPatients(string token, string? email, string? name, string? address)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

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

    public async Task<Patient> EditPatient(string token, int patientId, Person payload)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

        var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);

        if (patient is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, $"Unable to locate patient with id: {patientId}");
        }

        if (patient.DoctorId != user.Id)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, $"This user is forbidden from editing patient with id: {patientId}");
        }

        var patientWithIdenticalEmail = _context.Patients.FirstOrDefault(p => p.Email == payload.Email);

        if (patientWithIdenticalEmail is not null && patientWithIdenticalEmail != patient)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, $"Email address {payload.Email} already taken");
        }

        patient.UpdatePatient(payload);

        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();

        //await _hubContext.Clients.All.SendAsync("ReceiveUpdatedPatient", patient);

        return patient;
    }

}