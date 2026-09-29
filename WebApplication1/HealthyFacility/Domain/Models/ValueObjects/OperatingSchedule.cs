namespace WebApplication1.HealthyFacility.Domain.Models.ValueObjects;

public class OperatingSchedule
{
    public List<string> AvailableDays { get; }
    public List<string> AvailableSlots { get; }

    public OperatingSchedule(List<string> availableDays, List<string> availableSlots)
    {
        if (availableDays == null || availableDays.Count == 0)
            throw new ArgumentException("Available days required", nameof(availableDays));

        if (availableSlots == null || availableSlots.Count == 0)
            throw new ArgumentException("Available slots required", nameof(availableSlots));

        AvailableDays = availableDays;
        AvailableSlots = availableSlots;
    }

    // Constructor privado para serialización
    private OperatingSchedule() { }

    public OperatingSchedulePrimitives ToPrimitives()
    {
        return new OperatingSchedulePrimitives
        {
            AvailableDays = AvailableDays,
            AvailableSlots = AvailableSlots
        };
    }

    public class OperatingSchedulePrimitives
    {
        public List<string> AvailableDays { get; set; } = new();
        public List<string> AvailableSlots { get; set; } = new();
    }
}