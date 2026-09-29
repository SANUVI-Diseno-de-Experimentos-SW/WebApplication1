using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;

namespace WebApplication1.TreatmentTracking.Domain.Model.Aggregate;

public class Treatment
{
    public string Id { get; private set; }
    public string PatientId { get; private set; }
    public string NurseId { get; private set; }
    public string Supplement { get; private set; }
    public string Quantity { get; private set; }
    public string DosingHours { get; private set; }
    public int DurationDays { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public TreatmentStatus Status { get; private set; }
    public double AdherenceScore { get; private set; }
    public int CurrentStreak { get; private set; }
    public int TotalConfirmed { get; private set; }
    public int TotalOmitted { get; private set; }
    public string? CompletionObservation { get; private set; }
    public string? AbandonmentObservation { get; private set; }
    public RiskScore RiskScore { get; private set; }

    public Treatment(
        string id,
        string patientId,
        string nurseId,
        string supplement,
        string quantity,
        string dosingHours,
        int durationDays,
        DateTime startDate,
        DateTime endDate,
        TreatmentStatus status,
        double adherenceScore,
        int currentStreak,
        int totalConfirmed,
        int totalOmitted,
        string? completionObservation,
        string? abandonmentObservation,
        RiskScore riskScore)
    {
        Id = id;
        PatientId = patientId;
        NurseId = nurseId;
        Supplement = supplement;
        Quantity = quantity;
        DosingHours = dosingHours;
        DurationDays = durationDays;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        AdherenceScore = adherenceScore;
        CurrentStreak = currentStreak;
        TotalConfirmed = totalConfirmed;
        TotalOmitted = totalOmitted;
        CompletionObservation = completionObservation;
        AbandonmentObservation = abandonmentObservation;
        RiskScore = riskScore;

        Validate();
    }

    // Constructor privado para serialización
    private Treatment() { }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Treatment id is required", nameof(Id));

        if (string.IsNullOrWhiteSpace(PatientId))
            throw new ArgumentException("Patient id is required", nameof(PatientId));

        if (string.IsNullOrWhiteSpace(NurseId))
            throw new ArgumentException("Nurse id is required", nameof(NurseId));

        if (string.IsNullOrWhiteSpace(Supplement))
            throw new ArgumentException("Supplement is required", nameof(Supplement));

        if (string.IsNullOrWhiteSpace(Quantity))
            throw new ArgumentException("Quantity is required", nameof(Quantity));

        if (string.IsNullOrWhiteSpace(DosingHours))
            throw new ArgumentException("Dosing hours is required", nameof(DosingHours));

        if (DurationDays <= 0)
            throw new ArgumentException("Duration days must be greater than zero", nameof(DurationDays));

        if (StartDate == default)
            throw new ArgumentException("Start date is required", nameof(StartDate));

        if (EndDate == default)
            throw new ArgumentException("End date is required", nameof(EndDate));
    }

    // ==========================================
    // MÉTODOS DE DOMINIO
    // ==========================================

    public void CompleteTreatment(string nurseId, string? observation = null)
    {
        if (NurseId != nurseId)
            throw new InvalidOperationException("Only assigned nurse can complete treatment");

        if (Status != TreatmentStatus.ACTIVE)
            throw new InvalidOperationException("Only active treatments can be completed");

        Status = TreatmentStatus.COMPLETED;
        CompletionObservation = observation;
    }

    public void AbandonTreatment(string nurseId, string? observation = null)
    {
        if (NurseId != nurseId)
            throw new InvalidOperationException("Only assigned nurse can abandon treatment");

        if (Status != TreatmentStatus.ACTIVE)
            throw new InvalidOperationException("Only active treatments can be abandoned");

        Status = TreatmentStatus.ABANDONED;
        AbandonmentObservation = observation;
    }

    public void UpdateAdherenceMetrics(bool confirmed)
    {
        if (confirmed)
        {
            TotalConfirmed++;
            CurrentStreak++;
        }
        else
        {
            TotalOmitted++;
            CurrentStreak = 0;
        }

        var total = TotalConfirmed + TotalOmitted;
        if (total == 0)
        {
            AdherenceScore = 100;
            return;
        }

        AdherenceScore = (TotalConfirmed / (double)total) * 100;
    }

    public void UpdateRiskScore(RiskScore riskScore)
    {
        RiskScore = riskScore;
    }

    public bool IsActive() => Status == TreatmentStatus.ACTIVE;
    public bool IsCompleted() => Status == TreatmentStatus.COMPLETED;
    public bool IsAbandoned() => Status == TreatmentStatus.ABANDONED;

    // ==========================================
    // FACTORY METHOD
    // ==========================================

    public static Treatment Create(
        string id,
        string patientId,
        string nurseId,
        string supplement,
        string quantity,
        string dosingHours,
        int durationDays)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(durationDays);

        var riskScore = new RiskScore(
            Guid.NewGuid().ToString(),
            10, // Cambiado de 0 a 10 para que tenga un score inicial
            RiskLevel.LOW,
            DateTime.UtcNow
        );

        return new Treatment(
            id,
            patientId,
            nurseId,
            supplement,
            quantity,
            dosingHours,
            durationDays,
            startDate,
            endDate,
            TreatmentStatus.ACTIVE,
            100,
            0,
            0,
            0,
            null,
            null,
            riskScore
        );
    }

    // ==========================================
    // TO PRIMITIVES
    // ==========================================

    public TreatmentPrimitives ToPrimitives()
    {
        return new TreatmentPrimitives
        {
            Id = Id,
            PatientId = PatientId,
            NurseId = NurseId,
            Supplement = Supplement,
            Quantity = Quantity,
            DosingHours = DosingHours,
            DurationDays = DurationDays,
            StartDate = StartDate,
            EndDate = EndDate,
            Status = Status.ToStringValue(),
            AdherenceScore = AdherenceScore,
            CurrentStreak = CurrentStreak,
            TotalConfirmed = TotalConfirmed,
            TotalOmitted = TotalOmitted,
            CompletionObservation = CompletionObservation,
            AbandonmentObservation = AbandonmentObservation,
            RiskScore = RiskScore.ToPrimitives()
        };
    }

    public class TreatmentPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string NurseId { get; set; } = string.Empty;
        public string Supplement { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public string DosingHours { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public double AdherenceScore { get; set; }
        public int CurrentStreak { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalOmitted { get; set; }
        public string? CompletionObservation { get; set; }
        public string? AbandonmentObservation { get; set; }
        public RiskScore.RiskScorePrimitives RiskScore { get; set; } = new();
    }
}