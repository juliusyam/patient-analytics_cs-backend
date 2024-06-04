using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services.PatientMetrics;

public class PatientMetricsHeightService
{
    private readonly Context _context;
    private readonly PatientService _patientService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;
    
    public PatientMetricsHeightService(
        Context context, 
        PatientService patientService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _context = context;
        _patientService = patientService;
        _localized = localized;
    }

    public async Task<PatientHeight> CreateEntry(string authorization, int patientId, PatientHeightPayload payload)
    {
        _patientService.ValidateCrudPermission(authorization, patientId, out _, out var doctor);

        var heightRecord = PatientHeight.CreateFromPayload(patientId, doctor.Id, payload);

        _context.PatientHeights.Add(heightRecord);

        await _context.SaveChangesAsync();

        return heightRecord.Formatted();
    }

    public async Task<List<PatientHeight>> GetPatientHeights(string authorization, int patientId)
    {
        _patientService.ValidateCrudPermission(authorization, patientId, out _, out _);

        var heightRecords = _context.Patients
            .Where(p => p.Id == patientId)
            .SelectMany(p => p.Heights);

        await heightRecords.ForEachAsync(ph => ph.Formatted());

        return heightRecords.ToList();
    }

    public PatientHeight GetEntryById(string authorization, int heightRecordId)
    {
        ValidateCrudPermission(authorization, heightRecordId, out var heightRecord);

        return heightRecord.Formatted();
    }

    public async Task<IActionResult> DeleteEntryById(string authorization, int heightRecordId)
    {
        ValidateCrudPermission(authorization, heightRecordId, out _);

        await _context.PatientHeights.Where(ph => ph.Id == heightRecordId).ExecuteDeleteAsync();

        await _context.SaveChangesAsync();

        return new NoContentResult();
    }
    
    private void ValidateCrudPermission(string authorization, int heightRecordId, out PatientHeight verifiedHeightRecord)
    {
        _patientService.ValidateIsDoctor(authorization, out _);

        var heightRecord = _context.PatientHeights.FirstOrDefault(ph => ph.Id == heightRecordId);

        if (heightRecord is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, 
                string.Format(_localized["PatientHeightError_NotFound"], heightRecordId));
        }

        // Height Record Doctor can differ from Validated Doctor if the patient is transferred
        // Check whether the Validated Doctor can read and modify patient by matching heightRecord.PatientId
        _patientService.ValidateCrudPermission(authorization, heightRecord.PatientId, out _, out _);

        verifiedHeightRecord = heightRecord;
    }
}