using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services.PatientMetrics;

public class PatientMetricsTemperatureService
{
    private readonly Context _context;
    private readonly PatientService _patientService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;
    
    public PatientMetricsTemperatureService(
        Context context, 
        PatientService patientService, 
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _context = context;
        _patientService = patientService;
        _localized = localized;
    }

    public async Task<PatientTemperature> CreateEntry(string authorization, int patientId, PatientTemperaturePayload payload)
    {
        _patientService.ValidateCrudPermission(authorization, patientId, out _, out var doctor);

        var temperature = PatientTemperature.CreateFromPayload(patientId, doctor.Id, payload);

        _context.PatientTemperatures.Add(temperature);

        await _context.SaveChangesAsync();

        return temperature.Formatted();
    }

    public async Task<List<PatientTemperature>> GetPatientTemperatures(string authorization, int patientId)
    {
        _patientService.ValidateCrudPermission(authorization, patientId, out _, out _);

        var temperatures = _context.Patients
            .Where(p => p.Id == patientId)
            .SelectMany(p => p.Temperatures)
            .OrderByDescending(r => r.DateCreated);

        await temperatures.ForEachAsync(pt => pt.Formatted());
        
        return temperatures.ToList();
    }

    public PatientTemperature GetEntryById(string authorization, int temperatureRecordId)
    {
        ValidateCrudPermission(authorization, temperatureRecordId, out var temperatureRecord);
        
        return temperatureRecord.Formatted();
    }
    
    public async Task<IActionResult> DeleteEntryById(string authorization, int temperatureRecordId)
    {
        ValidateCrudPermission(authorization, temperatureRecordId, out _);

        await _context.PatientTemperatures.Where(t => t.Id == temperatureRecordId).ExecuteDeleteAsync();

        await _context.SaveChangesAsync();
        
        return new NoContentResult();
    }

    private void ValidateCrudPermission(string authorization, int temperatureRecordId, out PatientTemperature verifiedTemperatureRecord)
    {
        _patientService.ValidateIsDoctor(authorization, out _);

        var temperatureRecord = _context.PatientTemperatures.FirstOrDefault(t => t.Id == temperatureRecordId);

        if (temperatureRecord is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, 
                string.Format(_localized["PatientTemperatureError_NotFound"], temperatureRecordId));
        }

        // Temperature Record Doctor can differ from Validated Doctor if the patient is transferred
        // Check whether the Validated Doctor can read and modify patient by matching temperatureRecord.PatientId
        _patientService.ValidateCrudPermission(authorization, temperatureRecord.PatientId, out _, out _);

        verifiedTemperatureRecord = temperatureRecord;
    }
}