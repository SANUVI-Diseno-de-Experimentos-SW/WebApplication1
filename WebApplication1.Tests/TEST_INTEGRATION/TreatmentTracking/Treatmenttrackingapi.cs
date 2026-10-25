namespace WebApplication1.Tests.TEST_INTEGRATION.TreatmentTracking;

/// <summary>URLs y datos de prueba del contexto TreatmentTracking (api/treatment-tracking).</summary>
public static class TreatmentTrackingApi
{
    public const string BaseUrl = "/api/treatment-tracking";

    public const string TreatmentsUrl = BaseUrl + "/treatments";
    public const string ConfirmDoseUrl = BaseUrl + "/doses/confirm";
    public const string CompleteTreatmentUrl = BaseUrl + "/treatments/complete";
    public const string AbandonTreatmentUrl = BaseUrl + "/treatments/abandon";
    public const string RiskOverviewUrl = BaseUrl + "/risk-overview";
    public const string NurseTreatmentsUrl = BaseUrl + "/nurses/treatments";

    public static string TodayDoseUrl(string patientId) => $"{BaseUrl}/patients/{patientId}/today-dose";
    public static string DoseHistoryUrl(string patientId) => $"{BaseUrl}/patients/{patientId}/dose-history";
    public static string PatientsByRiskUrl(string riskLevel) => $"{BaseUrl}/risk/{riskLevel}/patients";

    // ADVERTENCIA: ajusta estos valores a los formatos que acepte tu dominio.
    // Si "iniciar tratamiento" responde 400, el error (campo "error" y "detail") dice que valor rechazo.
    public const string SupplementName = "Sulfato ferroso";
    public const string Quantity = "1 tableta";
    public const string DosingHours = "08:00";
    public const int DurationDays = 30;

    // ADVERTENCIA: nombres de estados y niveles de riesgo. Ajustalos a los de tus enums.
    public const string ValidRiskLevel = "HIGH";
    public const string StatusActive = "ACTIVE";
    public const string StatusCompleted = "COMPLETED";

    public static object NewTreatment(string patientId) => new
    {
        PatientId = patientId,
        SupplementName,
        Quantity,
        DosingHours,
        DurationDays
    };
}