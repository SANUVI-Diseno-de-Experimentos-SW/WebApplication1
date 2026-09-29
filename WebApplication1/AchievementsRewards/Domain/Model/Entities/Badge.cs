using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;

namespace WebApplication1.AchievementsRewards.Domain.Model.Entities;



public class Badge
{
    public string Id { get; private set; }
    public string AchievementId { get; private set; }
    public BadgeType Type { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Milestone { get; private set; }
    public bool IsUnlocked { get; private set; }
    public DateTime? UnlockedAt { get; private set; }

    public Badge(
        string id,
        string achievementId,
        BadgeType type,
        string name,
        string description,
        int milestone,
        bool isUnlocked,
        DateTime? unlockedAt)
    {
        Id = id;
        AchievementId = achievementId;
        Type = type;
        Name = name;
        Description = description;
        Milestone = milestone;
        IsUnlocked = isUnlocked;
        UnlockedAt = unlockedAt;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Badge id is required", nameof(Id));

        if (string.IsNullOrWhiteSpace(AchievementId))
            throw new ArgumentException("AchievementId is required", nameof(AchievementId));

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Badge name is required", nameof(Name));

        if (string.IsNullOrWhiteSpace(Description))
            throw new ArgumentException("Badge description is required", nameof(Description));

        if (Milestone <= 0)
            throw new ArgumentException("Milestone must be greater than zero", nameof(Milestone));
    }

    // ==========================================
    // MÉTODOS DE DOMINIO
    // ==========================================

    public void Unlock()
    {
        if (IsUnlocked)
        {
            throw new InvalidOperationException("Badge is already unlocked");
        }

        IsUnlocked = true;
        UnlockedAt = DateTime.UtcNow;
    }

    public bool CanBeUnlockedWithStreak(int currentStreak)
    {
        if (IsUnlocked) return false;
        return currentStreak >= Milestone;
    }

    public bool CanBeUnlockedWithBestStreak(int bestStreak)
    {
        if (IsUnlocked) return false;
        return bestStreak >= Milestone;
    }

    // ==========================================
    // FACTORY METHOD
    // ==========================================

    public static Badge Create(
        string id,
        string achievementId,
        BadgeType type,
        int durationDays)
    {
        var milestone = MilestoneCalculator.GetMilestone(type, durationDays);

        string name, description;

        switch (type)
        {
            case BadgeType.FIRST_WEEK:
                name = "Primera semana";
                description = "Completaste 7 días consecutivos sin fallar";
                break;
            case BadgeType.HALF_TREATMENT:
                name = "Mitad del tratamiento";
                description = $"Alcanzaste la mitad del tratamiento ({milestone} días consecutivos)";
                break;
            case BadgeType.TREATMENT_COMPLETED:
                name = "Tratamiento completado";
                description = $"Completaste el tratamiento completo de {milestone} días";
                break;
            default:
                throw new ArgumentException($"Unknown badge type: {type}");
        }

        return new Badge(
            id,
            achievementId,
            type,
            name,
            description,
            milestone,
            false,  // isUnlocked = false
            null    // unlockedAt = null
        );
    }

    // ==========================================
    // TO PRIMITIVES
    // ==========================================

    public BadgePrimitives ToPrimitives()
    {
        return new BadgePrimitives
        {
            Id = Id,
            AchievementId = AchievementId,
            Type = Type.ToStringValue(),
            Name = Name,
            Description = Description,
            Milestone = Milestone,
            IsUnlocked = IsUnlocked,
            UnlockedAt = UnlockedAt
        };
    }

    public class BadgePrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string AchievementId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Milestone { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
    }
}