namespace PatientAnalytics.Models.PatientMetrics.Units;

public enum WeightUnit
{
    Lb,
    Kg,
    St
}

internal static class WeightUnitMethods
{
    public static string ToLabel(this WeightUnit weightUnit)
    {
        return weightUnit switch
        {
            WeightUnit.Lb => "Unit_Lb",
            WeightUnit.Kg => "Unit_Kg",
            WeightUnit.St => "Unit_St",
            _ => ""
        };
    }

    public static string? SelectEntryValueForUnit(this WeightUnit weightUnit, PatientWeight entry)
    {
        return weightUnit switch
        {
            WeightUnit.Lb => entry.WeightLbFormatted,
            WeightUnit.Kg => entry.WeightKgFormatted,
            WeightUnit.St => entry.WeightStFormatted,
            _ => null
        };
    }
}