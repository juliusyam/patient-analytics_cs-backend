using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services.PatientMetrics;

public class PatientMetricsWeightService
{
    private readonly Context _context;
    private readonly PatientService _patientService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;
    
    public PatientMetricsWeightService(
        Context context, 
        PatientService patientService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _context = context;
        _patientService = patientService;
        _localized = localized;
    }

    public async Task<PatientWeight> CreateEntry(string authorization, int patientId, PatientWeightPayload payload)
    {
        _patientService.ValidateCrudPermission(authorization, patientId, out _, out var doctor);

        var weightRecord = PatientWeight.CreateFromPayload(patientId, doctor.Id, payload);

        _context.PatientWeights.Add(weightRecord);

        await _context.SaveChangesAsync();

        return weightRecord.Formatted();
    }

    public async Task<List<PatientWeight>> GetPatientWeights(string authorization, int patientId)
    {
        _patientService.ValidateCrudPermission(authorization, patientId, out _, out _);

        var weightRecords = _context.PatientWeights.Where(pw => pw.PatientId == patientId);

        await weightRecords.ForEachAsync(pw => pw.Formatted());

        return weightRecords.ToList();
    }

    public PatientWeight GetEntryById(string authorization, int weightRecordId)
    {
        ValidateCrudPermission(authorization, weightRecordId, out var weightRecord);

        return weightRecord.Formatted();
    }

    public async Task<IActionResult> DeleteEntryById(string authorization, int weightRecordId)
    {
        ValidateCrudPermission(authorization, weightRecordId, out _);

        await _context.PatientWeights.Where(pw => pw.Id == weightRecordId).ExecuteDeleteAsync();

        await _context.SaveChangesAsync();

        return new NoContentResult();
    }
    
    private void ValidateCrudPermission(string authorization, int weightRecordId, out PatientWeight verifiedWeightRecord)
    {
        _patientService.ValidateIsDoctor(authorization, out _);

        var weightRecord = _context.PatientWeights.FirstOrDefault(ph => ph.Id == weightRecordId);

        if (weightRecord is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, 
                string.Format(_localized["PatientWeightError_NotFound"], weightRecordId));
        }

        // Weight Record Doctor can differ from Validated Doctor if the patient is transferred
        // Check whether the Validated Doctor can read and modify patient by matching weightRecord.PatientId
        _patientService.ValidateCrudPermission(authorization, weightRecord.PatientId, out _, out _);

        verifiedWeightRecord = weightRecord;
    }
}