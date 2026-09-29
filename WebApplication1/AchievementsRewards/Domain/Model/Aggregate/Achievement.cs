using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;

namespace WebApplication1.AchievementsRewards.Domain.Model.Aggregate;

public class Achievement
{
    public string Id { get; private set; }
    public string PatientId { get; private set; }
    public string MotherId { get; private set; }
    public string TreatmentId { get; private set; }
    public int DurationDays { get; private set; }
    public int CurrentStreak { get; private set; }
    public int LongestStreak { get; private set; }
    public int BestStreak { get; private set; }
    public DateTime? StreakStartDate { get; private set; }
    public int TotalPoints { get; private set; }
    public AchievementStatus Status { get; private set; }

    public Achievement(
        string id,
        string patientId,
        string motherId,
        string treatmentId,
        int durationDays,
        int currentStreak,
        int longestStreak,
        int bestStreak,
        DateTime? streakStartDate,
        int totalPoints,
        AchievementStatus status)
    {
        Id = id;
        PatientId = patientId;
        MotherId = motherId;
        TreatmentId = treatmentId;
        DurationDays = durationDays;
        CurrentStreak = currentStreak;
        LongestStreak = longestStreak;
        BestStreak = bestStreak;
        StreakStartDate = streakStartDate;
        TotalPoints = totalPoints;
        Status = status;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Achievement id is required", nameof(Id));

        if (string.IsNullOrWhiteSpace(PatientId))
            throw new ArgumentException("Patient id is required", nameof(PatientId));

        if (string.IsNullOrWhiteSpace(MotherId))
            throw new ArgumentException("Mother id is required", nameof(MotherId));

        if (string.IsNullOrWhiteSpace(TreatmentId))
            throw new ArgumentException("Treatment id is required", nameof(TreatmentId));

        if (DurationDays <= 0)
            throw new ArgumentException("Duration days must be greater than zero", nameof(DurationDays));

        if (CurrentStreak < 0)
            throw new ArgumentException("Current streak cannot be negative", nameof(CurrentStreak));

        if (LongestStreak < 0)
            throw new ArgumentException("Longest streak cannot be negative", nameof(LongestStreak));

        if (BestStreak < 0)
            throw new ArgumentException("Best streak cannot be negative", nameof(BestStreak));

        if (TotalPoints < 0)
            throw new ArgumentException("Total points cannot be negative", nameof(TotalPoints));
    }

    // ==========================================
    // MÉTODOS DE DOMINIO
    // ==========================================

    public void OnDoseConfirmed()
    {
        if (Status != AchievementStatus.ACTIVE)
        {
            throw new InvalidOperationException("Cannot update points on non-active achievement");
        }

        // 1. Sumar puntos (+10)
        TotalPoints += 10;

        // 2. Actualizar racha actual
        var previousStreak = CurrentStreak;
        CurrentStreak++;

        // 3. Actualizar streakStartDate si es nueva racha
        if (previousStreak == 0)
        {
            StreakStartDate = DateTime.UtcNow;
        }

        // 4. Actualizar longestStreak (mejor racha histórica)
        if (CurrentStreak > LongestStreak)
        {
            LongestStreak = CurrentStreak;
        }

        // 5. Actualizar bestStreak (para badges - NO se reinicia nunca)
        if (CurrentStreak > BestStreak)
        {
            BestStreak = CurrentStreak;
        }
    }

    public void OnDoseOmitted()
    {
        if (Status != AchievementStatus.ACTIVE)
        {
            throw new InvalidOperationException("Cannot update non-active achievement");
        }

        // 1. NO se suman puntos
        // 2. La racha actual se reinicia
        CurrentStreak = 0;
        StreakStartDate = null;

        // 3. bestStreak NO cambia (se mantiene el mejor histórico)
        // 4. longestStreak NO cambia
    }

    public void OnTreatmentCompleted()
    {
        if (Status != AchievementStatus.ACTIVE)
        {
            throw new InvalidOperationException("Only active treatments can be completed");
        }

        // Bonus final por completar tratamiento
        TotalPoints += 50;
        Status = AchievementStatus.COMPLETED;
    }

    public void OnTreatmentAbandoned()
    {
        if (Status != AchievementStatus.ACTIVE)
        {
            throw new InvalidOperationException("Only active treatments can be abandoned");
        }

        Status = AchievementStatus.ABANDONED;
    }

    // ==========================================
    // FACTORY METHOD
    // ==========================================

    public static Achievement Create(
        string id,
        string patientId,
        string motherId,
        string treatmentId,
        int durationDays)
    {
        return new Achievement(
            id,
            patientId,
            motherId,
            treatmentId,
            durationDays,
            0,          // currentStreak inicial
            0,          // longestStreak inicial
            0,          // bestStreak inicial
            null,       // streakStartDate inicial
            0,          // totalPoints inicial
            AchievementStatus.ACTIVE
        );
    }

    // ==========================================
    // TO PRIMITIVES
    // ==========================================

    public AchievementPrimitives ToPrimitives()
    {
        return new AchievementPrimitives
        {
            Id = Id,
            PatientId = PatientId,
            MotherId = MotherId,
            TreatmentId = TreatmentId,
            DurationDays = DurationDays,
            CurrentStreak = CurrentStreak,
            LongestStreak = LongestStreak,
            BestStreak = BestStreak,
            StreakStartDate = StreakStartDate,
            TotalPoints = TotalPoints,
            Status = Status.ToStringValue()
        };
    }

    public class AchievementPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string MotherId { get; set; } = string.Empty;
        public string TreatmentId { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int BestStreak { get; set; }
        public DateTime? StreakStartDate { get; set; }
        public int TotalPoints { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}