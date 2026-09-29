using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;

namespace WebApplication1.TreatmentTracking.Domain.Model.Entities;


public class DailyDose
{
    public string Id { get; private set; }
    public string TreatmentId { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DoseStatus Status { get; private set; }

    public DailyDose(
        string id,
        string treatmentId,
        DateTime scheduledDate,
        DateTime? confirmedAt,
        DoseStatus status)
    {
        Id = id;
        TreatmentId = treatmentId;
        ScheduledDate = scheduledDate;
        ConfirmedAt = confirmedAt;
        Status = status;

        Validate();
    }

    // Constructor privado para serialización
    private DailyDose()
    {
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Daily dose id is required", nameof(Id));

        if (string.IsNullOrWhiteSpace(TreatmentId))
            throw new ArgumentException("Treatment id is required", nameof(TreatmentId));

        if (ScheduledDate == default)
            throw new ArgumentException("Scheduled date is required", nameof(ScheduledDate));
    }

    public void Confirm()
    {
        if (Status == DoseStatus.CONFIRMED)
            throw new InvalidOperationException("Dose already confirmed");

        if (Status == DoseStatus.OMITTED)
            throw new InvalidOperationException("Cannot confirm an omitted dose");

        Status = DoseStatus.CONFIRMED;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void MarkAsOmitted()
    {
        if (Status == DoseStatus.CONFIRMED)
            throw new InvalidOperationException("Confirmed dose cannot be omitted");

        if (Status == DoseStatus.OMITTED)
            return;

        Status = DoseStatus.OMITTED;
    }

    public double CalculateHoursWithoutConfirmation()
    {
        if (Status == DoseStatus.CONFIRMED)
            return 0;

        var now = DateTime.UtcNow;
        var differenceMs = (now - ScheduledDate).TotalMilliseconds;
        var hours = Math.Floor(differenceMs / (1000 * 60 * 60));

        return hours;
    }

    public bool IsPending() => Status == DoseStatus.PENDING;
    public bool IsConfirmed() => Status == DoseStatus.CONFIRMED;
    public bool IsOmitted() => Status == DoseStatus.OMITTED;

    public DailyDosePrimitives ToPrimitives()
    {
        return new DailyDosePrimitives
        {
            Id = Id,
            TreatmentId = TreatmentId,
            ScheduledDate = ScheduledDate,
            ConfirmedAt = ConfirmedAt,
            Status = Status.ToStringValue()
        };
    }

    public class DailyDosePrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string TreatmentId { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}