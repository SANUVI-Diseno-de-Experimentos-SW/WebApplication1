using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.ValueObjects;

namespace WebApplication1.patient_management.Domain.Entities;


/// <summary>
/// Control de hemoglobina
/// </summary>
public class Control
{
    public string Id { get; }
    public DateTime Date { get; }
    public HemoglobinLevel HemoglobinLevel { get; private set; }
    public AnemiaStatus AnemiaStatus { get; private set; }

    public Control(string id, DateTime date, HemoglobinLevel hemoglobinLevel)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Control ID is required", nameof(id));

        if (date == default)
            throw new ArgumentException("Date is required", nameof(date));

        if (date > DateTime.UtcNow)
            throw new ArgumentException("Control date cannot be in the future", nameof(date));

        Id = id;
        Date = date;
        HemoglobinLevel = hemoglobinLevel ?? throw new ArgumentNullException(nameof(hemoglobinLevel));
        AnemiaStatus = CalculateAnemiaStatus();
    }

    // Constructor privado para serialización
    private Control() { }

    private AnemiaStatus CalculateAnemiaStatus()
    {
        var value = HemoglobinLevel.Value;

        if (!value.HasValue)
            return AnemiaStatus.Controlled;

        return value.Value switch
        {
            < 7 => AnemiaStatus.Severe,
            >= 7 and < 9 => AnemiaStatus.Moderate,
            >= 9 and < 11 => AnemiaStatus.Mild,
            _ => AnemiaStatus.Controlled
        };
    }

    public ControlPrimitives ToPrimitives()
    {
        return new ControlPrimitives
        {
            Id = Id,
            Date = Date,
            HemoglobinLevel = HemoglobinLevel.Value,
            AnemiaStatus = AnemiaStatus.ToStringValue()
        };
    }

    public class ControlPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double? HemoglobinLevel { get; set; }
        public string AnemiaStatus { get; set; } = string.Empty;
    }
}