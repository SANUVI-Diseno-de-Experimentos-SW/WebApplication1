namespace WebApplication1.patient_management.Interfaces.Assembler;

public static class HemoglobinHistoryResourceAssembler
{
    public static object ToResource(object data)
    {
        var type = data.GetType();
        
        return new
        {
            patientId = type.GetProperty("PatientId")?.GetValue(data),
            patientName = type.GetProperty("PatientName")?.GetValue(data),
            controls = type.GetProperty("Controls")?.GetValue(data),
            averageHemoglobin = type.GetProperty("AverageHemoglobin")?.GetValue(data),
            totalControls = type.GetProperty("TotalControls")?.GetValue(data),
            evolution = type.GetProperty("Evolution")?.GetValue(data),
            trend = type.GetProperty("Trend")?.GetValue(data)
        };
    }
}