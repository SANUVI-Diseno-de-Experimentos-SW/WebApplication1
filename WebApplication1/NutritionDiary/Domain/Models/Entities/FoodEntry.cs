namespace WebApplication1.NutritionDiary.Domain.Models.Entities;

public class FoodEntry
{
    public string Id { get; }
    public string DiaryId { get; }
    public string FoodItemId { get; }
    public double Quantity { get; private set; }
    public string Unit { get; }
    public double IronContributed { get; private set; }
    public DateTime RegisteredAt { get; }

    public FoodEntry(
        string id,
        string diaryId,
        string foodItemId,
        double quantity,
        string unit,
        double ironContributed,
        DateTime registeredAt)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Food entry id is required", nameof(id));

        if (string.IsNullOrWhiteSpace(diaryId))
            throw new ArgumentException("Diary id is required", nameof(diaryId));

        if (string.IsNullOrWhiteSpace(foodItemId))
            throw new ArgumentException("Food item id is required", nameof(foodItemId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit is required", nameof(unit));

        if (ironContributed < 0)
            throw new ArgumentException("Iron contributed cannot be negative", nameof(ironContributed));

        if (registeredAt == default)
            throw new ArgumentException("Registration date is required", nameof(registeredAt));

        Id = id;
        DiaryId = diaryId;
        FoodItemId = foodItemId;
        Quantity = quantity;
        Unit = unit;
        IronContributed = ironContributed;
        RegisteredAt = registeredAt;
    }

    // Constructor privado para serialización
    private FoodEntry() { }

    public void UpdateQuantity(double newQuantity, double newIronContributed)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        Quantity = newQuantity;
        IronContributed = newIronContributed;
    }

    public FoodEntryPrimitives ToPrimitives()
    {
        return new FoodEntryPrimitives
        {
            Id = Id,
            DiaryId = DiaryId,
            FoodItemId = FoodItemId,
            Quantity = Quantity,
            Unit = Unit,
            IronContributed = IronContributed,
            RegisteredAt = RegisteredAt
        };
    }

    public class FoodEntryPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string DiaryId { get; set; } = string.Empty;
        public string FoodItemId { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double IronContributed { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}