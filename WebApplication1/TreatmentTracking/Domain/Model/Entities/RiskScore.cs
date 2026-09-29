using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;

namespace WebApplication1.TreatmentTracking.Domain.Model.Entities;

public class RiskScore
{
    public string Id { get; private set; }
    public int Score { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public DateTime CalculatedAt { get; private set; }

    public RiskScore(string id, int score, RiskLevel riskLevel, DateTime calculatedAt)
    {
        Id = id;
        Score = score;
        RiskLevel = riskLevel;
        CalculatedAt = calculatedAt;

        Validate();
    }

    // Constructor privado para serialización
    private RiskScore() { }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Risk score id is required", nameof(Id));

        if (Score < 0 || Score > 100)
            throw new ArgumentException("Risk score must be between 0 and 100", nameof(Score));

        if (CalculatedAt == default)
            throw new ArgumentException("Calculated date is required", nameof(CalculatedAt));
    }

    public void UpdateScore(int newScore)
    {
        if (newScore < 0 || newScore > 100)
            throw new ArgumentException("Invalid risk score", nameof(newScore));

        Score = newScore;
        RiskLevel = CalculateRiskLevel(newScore);
        CalculatedAt = DateTime.UtcNow;
    }

    private RiskLevel CalculateRiskLevel(int score)
    {
        if (score > 70) return RiskLevel.HIGH;
        if (score >= 30 && score <= 70) return RiskLevel.MEDIUM;
        return RiskLevel.LOW;
    }

    public RiskScorePrimitives ToPrimitives()
    {
        return new RiskScorePrimitives
        {
            Id = Id,
            Score = Score,
            RiskLevel = RiskLevel.ToStringValue(),
            CalculatedAt = CalculatedAt
        };
    }

    public class RiskScorePrimitives
    {
        public string Id { get; set; } = string.Empty;
        public int Score { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime CalculatedAt { get; set; }
    }
}