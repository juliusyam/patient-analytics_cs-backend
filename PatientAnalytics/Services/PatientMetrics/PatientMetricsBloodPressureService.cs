using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services.PatientMetrics;

public class PatientMetricsBloodPressureService
{
    private readonly Context _context;
    private readonly PatientService _patientService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public PatientMetricsBloodPressureService(
        Context context, 
        PatientService patientService, 
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _context = context;
        _patientService = patientService;
        _localized = localized;
    }

    public async Task<PatientBloodPressure> CreateEntry(
        string authorization, 
        int patientId,
        PatientBloodPressurePayload payload)
    {
        _patientService.ValidateCrudPermission(authorization, patientId, out _, out var doctor);

        var bloodPressureRecord = PatientBloodPressure.CreateFromPayload(patientId, doctor.Id, payload);

        _context.PatientBloodPressures.Add(bloodPressureRecord);

        await _context.SaveChangesAsync();
        
        return bloodPressureRecord.Formatted();
    }

    public async Task<List<PatientBloodPressure>> GetPatientBloodPressures(string authorization, int patientId)
    {
        _patientService.ValidateCrudPermission(authorization, patientId, out _, out _);

        var bloodPressureRecords = _context.Patients
            .Where(p => p.Id == patientId)
            .SelectMany(p => p.BloodPressures)
            .OrderByDescending(r => r.DateCreated);

        await bloodPressureRecords.ForEachAsync(pbp => pbp.Formatted());

        return bloodPressureRecords.ToList();
    }

    public PatientBloodPressure GetEntryById(string authorization, int bloodPressureRecordId)
    {
        ValidateCrudPermission(authorization, bloodPressureRecordId, out var bloodPressureRecord);

        return bloodPressureRecord.Formatted();
    }

    public async Task<IActionResult> DeleteEntryById(string authorization, int bloodPressureRecordId)
    {
        ValidateCrudPermission(authorization, bloodPressureRecordId, out _);

        await _context.PatientBloodPressures.Where(pbp => pbp.Id == bloodPressureRecordId).ExecuteDeleteAsync();

        await _context.SaveChangesAsync();

        return new NoContentResult();
    }
    
    private void ValidateCrudPermission(
        string authorization, 
        int bloodPressureRecordId, 
        out PatientBloodPressure verifiedBloodPressureRecord)
    {
        _patientService.ValidateIsDoctor(authorization, out _);

        var bloodPressureRecord = _context.PatientBloodPressures.FirstOrDefault(pbp => pbp.Id == bloodPressureRecordId);

        if (bloodPressureRecord is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, 
                string.Format(_localized["PatientBloodPressureError_NotFound"], bloodPressureRecordId));
        }

        // Blood Pressure Record Doctor can differ from Validated Doctor if the patient is transferred
        // Check whether the Validated Doctor can read and modify patient by matching bloodPressureRecord.PatientId
        _patientService.ValidateCrudPermission(authorization, bloodPressureRecord.PatientId, out _, out _);

        verifiedBloodPressureRecord = bloodPressureRecord;
    }
}