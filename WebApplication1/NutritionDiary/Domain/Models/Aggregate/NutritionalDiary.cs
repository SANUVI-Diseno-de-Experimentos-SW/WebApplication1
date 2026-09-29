namespace WebApplication1.NutritionDiary.Domain.Models.Aggregate;

public class NutritionalDiary
{
    public string Id { get; }
    public string PatientId { get; }
    public string MotherId { get; }
    public DateTime Date { get; }
    public double TotalIronAbsorbed { get; private set; }
    public bool HasInhibitor { get; private set; }

    public NutritionalDiary(
        string id,
        string patientId,
        string motherId,
        DateTime date,
        double totalIronAbsorbed,
        bool hasInhibitor)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Diary id is required", nameof(id));

        if (string.IsNullOrWhiteSpace(patientId))
            throw new ArgumentException("Patient id is required", nameof(patientId));

        if (string.IsNullOrWhiteSpace(motherId))
            throw new ArgumentException("Mother id is required", nameof(motherId));

        if (date == default)
            throw new ArgumentException("Diary date is required", nameof(date));

        if (totalIronAbsorbed < 0)
            throw new ArgumentException("Total iron absorbed cannot be negative", nameof(totalIronAbsorbed));

        Id = id;
        PatientId = patientId;
        MotherId = motherId;
        Date = date;
        TotalIronAbsorbed = totalIronAbsorbed;
        HasInhibitor = hasInhibitor;
    }

    // Constructor privado para serialización
    private NutritionalDiary() { }

    public void UpdateMetrics(double totalIronAbsorbed, bool hasInhibitor)
    {
        if (totalIronAbsorbed < 0)
            throw new ArgumentException("Total iron absorbed cannot be negative", nameof(totalIronAbsorbed));

        TotalIronAbsorbed = totalIronAbsorbed;
        HasInhibitor = hasInhibitor;
    }

    public void MarkInhibitorDetected()
    {
        HasInhibitor = true;
    }

    public void ResetDailyIron()
    {
        TotalIronAbsorbed = 0;
        HasInhibitor = false;
    }

    public NutritionalDiaryPrimitives ToPrimitives()
    {
        return new NutritionalDiaryPrimitives
        {
            Id = Id,
            PatientId = PatientId,
            MotherId = MotherId,
            Date = Date,
            TotalIronAbsorbed = TotalIronAbsorbed,
            HasInhibitor = HasInhibitor
        };
    }

    public class NutritionalDiaryPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string MotherId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double TotalIronAbsorbed { get; set; }
        public bool HasInhibitor { get; set; }
    }
}