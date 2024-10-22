namespace PatientAnalytics.Models.PatientMetrics.Units;

public enum TemperatureUnit
{
    Celsius,
    Fahrenheit
}

internal static class TemperatureUnitMethods
{
    public static string ToLabel(this TemperatureUnit temperatureUnit)
    {
        return temperatureUnit switch
        {
            TemperatureUnit.Celsius => "Label_Celsius",
            TemperatureUnit.Fahrenheit => "Label_Fahrenheit",
            _ => ""
        };
    }

    public static string? SelectEntryValueForUnit(this TemperatureUnit temperatureUnit, PatientTemperature entry)
    {
        return temperatureUnit switch
        {
            TemperatureUnit.Celsius => entry.TemperatureCelsiusFormatted,
            TemperatureUnit.Fahrenheit => entry.TemperatureFahrenheitFormatted,
            _ => null
        };
    }
}