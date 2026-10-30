using WebApplication1.NutritionDiary.Domain.Models.Entities;
using Xunit;

namespace WebApplication1.Tests.NutritionDiary.Domain.Models.Entities;

public class FoodEntryTests
{
    // ==========================================
    // HELPERS
    // ==========================================

    private static readonly DateTime TestDate = new(2026, 10, 24, 12, 30, 0, DateTimeKind.Utc);

    private static FoodEntry CreateEntry(
        string id = "entry-001",
        string diaryId = "diary-001",
        string foodItemId = "food-001",
        double quantity = 150,
        string unit = "g",
        double ironContributed = 2.5,
        DateTime? registeredAt = null)
    {
        return new FoodEntry(
            id,
            diaryId,
            foodItemId,
            quantity,
            unit,
            ironContributed,
            registeredAt ?? TestDate);
    }

    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    [Fact]
    public void Constructor_WithValidData_CreatesEntrySuccessfully()
    {
        var entry = new FoodEntry("entry-001", "diary-001", "food-001", 150, "g", 2.5, TestDate);

        Assert.NotNull(entry);
        Assert.Equal("entry-001", entry.Id);
        Assert.Equal("diary-001", entry.DiaryId);
        Assert.Equal("food-001", entry.FoodItemId);
        Assert.Equal(150, entry.Quantity);
        Assert.Equal("g", entry.Unit);
        Assert.Equal(2.5, entry.IronContributed);
        Assert.Equal(TestDate, entry.RegisteredAt);
    }

    [Fact]
    public void Constructor_WithZeroIron_CreatesEntrySuccessfully()
    {
        var entry = CreateEntry(ironContributed: 0);

        Assert.Equal(0, entry.IronContributed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateEntry(id: invalidId!));

        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidDiaryId_ThrowsArgumentException(string? invalidDiaryId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateEntry(diaryId: invalidDiaryId!));

        Assert.Equal("diaryId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidFoodItemId_ThrowsArgumentException(string? invalidFoodItemId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateEntry(foodItemId: invalidFoodItemId!));

        Assert.Equal("foodItemId", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-50)]
    public void Constructor_WithQuantityZeroOrNegative_ThrowsArgumentException(double invalidQuantity)
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateEntry(quantity: invalidQuantity));

        Assert.Equal("quantity", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidUnit_ThrowsArgumentException(string? invalidUnit)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateEntry(unit: invalidUnit!));

        Assert.Equal("unit", ex.ParamName);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public void Constructor_WithNegativeIron_ThrowsArgumentException(double negativeIron)
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateEntry(ironContributed: negativeIron));

        Assert.Equal("ironContributed", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithDefaultRegisteredAt_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateEntry(registeredAt: default(DateTime)));

        Assert.Equal("registeredAt", ex.ParamName);
    }

    // ==========================================
    // UpdateQuantity
    // ==========================================

    [Fact]
    public void UpdateQuantity_WithValidData_UpdatesQuantityAndIron()
    {
        var entry = CreateEntry(quantity: 150, ironContributed: 2.5);

        entry.UpdateQuantity(300, 5.0);

        Assert.Equal(300, entry.Quantity);
        Assert.Equal(5.0, entry.IronContributed);
    }

    [Fact]
    public void UpdateQuantity_WithZeroIron_UpdatesSuccessfully()
    {
        var entry = CreateEntry(ironContributed: 2.5);

        entry.UpdateQuantity(100, 0);

        Assert.Equal(100, entry.Quantity);
        Assert.Equal(0, entry.IronContributed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-50)]
    public void UpdateQuantity_WithQuantityZeroOrNegative_ThrowsArgumentException(double invalidQuantity)
    {
        var entry = CreateEntry();

        var ex = Assert.Throws<ArgumentException>(() => entry.UpdateQuantity(invalidQuantity, 1.0));

        Assert.Equal("newQuantity", ex.ParamName);
    }

    [Fact]
    public void UpdateQuantity_WithInvalidQuantity_DoesNotChangeExistingState()
    {
        var entry = CreateEntry(quantity: 150, ironContributed: 2.5);

        Assert.Throws<ArgumentException>(() => entry.UpdateQuantity(-1, 9.9));

        Assert.Equal(150, entry.Quantity);
        Assert.Equal(2.5, entry.IronContributed);
    }

    [Fact]
    public void UpdateQuantity_DoesNotChangeIdentityFields()
    {
        var entry = CreateEntry();

        entry.UpdateQuantity(300, 5.0);

        Assert.Equal("entry-001", entry.Id);
        Assert.Equal("diary-001", entry.DiaryId);
        Assert.Equal("food-001", entry.FoodItemId);
        Assert.Equal("g", entry.Unit);
        Assert.Equal(TestDate, entry.RegisteredAt);
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var entry = CreateEntry(quantity: 200, unit: "ml", ironContributed: 3.2);

        var primitives = entry.ToPrimitives();

        Assert.Equal(entry.Id, primitives.Id);
        Assert.Equal(entry.DiaryId, primitives.DiaryId);
        Assert.Equal(entry.FoodItemId, primitives.FoodItemId);
        Assert.Equal(entry.Quantity, primitives.Quantity);
        Assert.Equal(entry.Unit, primitives.Unit);
        Assert.Equal(entry.IronContributed, primitives.IronContributed);
        Assert.Equal(entry.RegisteredAt, primitives.RegisteredAt);
    }

    [Fact]
    public void ToPrimitives_AfterUpdateQuantity_ReflectsNewValues()
    {
        var entry = CreateEntry();
        entry.UpdateQuantity(400, 7.7);

        var primitives = entry.ToPrimitives();

        Assert.Equal(400, primitives.Quantity);
        Assert.Equal(7.7, primitives.IronContributed);
    }
}
