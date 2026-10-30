using WebApplication1.NutritionDiary.Domain.Models.Entities;
using WebApplication1.NutritionDiary.Domain.Models.ValueObjects;
using Xunit;

namespace WebApplication1.Tests.NutritionDiary.Domain.Models.Entities;

public class FoodItemTests
{
    // ==========================================
    // HELPERS
    // ==========================================
    // IMPORTANTE: NewNutrientContent() es el unico lugar que depende del constructor de
    // NutrientContent (no lo conozco). Si tu value object se crea distinto
    // (por ejemplo NutrientContent.Create(...)), ajusta solo este metodo.
    // Para la categoria uso el primer valor del enum, asi no dependo de nombres concretos.
    
    private static NutrientContent NewNutrientContent() => new(2.5, "hemo");
    private static FoodCategory AnyCategory() => Enum.GetValues<FoodCategory>().First();

    private static FoodItem CreateItem(
        string id = "food-001",
        string name = "Higado de res",
        NutrientContent? nutrientContent = null,
        bool isInhibitor = false,
        FoodCategory? category = null)
    {
        return new FoodItem(
            id,
            name,
            nutrientContent ?? NewNutrientContent(),
            isInhibitor,
            category ?? AnyCategory());
    }

    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    [Fact]
    public void Constructor_WithValidData_CreatesFoodItemSuccessfully()
    {
        var content = NewNutrientContent();
        var category = AnyCategory();

        var item = new FoodItem("food-001", "Higado de res", content, true, category);

        Assert.NotNull(item);
        Assert.Equal("food-001", item.Id);
        Assert.Equal("Higado de res", item.Name);
        Assert.Same(content, item.NutrientContent);
        Assert.True(item.IsInhibitor);
        Assert.Equal(category, item.Category);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_SetsIsInhibitorAsProvided(bool isInhibitor)
    {
        var item = CreateItem(isInhibitor: isInhibitor);

        Assert.Equal(isInhibitor, item.IsInhibitor);
    }

    [Fact]
    public void Constructor_AcceptsEveryFoodCategory()
    {
        foreach (var category in Enum.GetValues<FoodCategory>())
        {
            var item = CreateItem(category: category);

            Assert.Equal(category, item.Category);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateItem(id: invalidId!));

        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateItem(name: invalidName!));

        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullNutrientContent_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new FoodItem("food-001", "Higado de res", null!, false, AnyCategory()));

        Assert.Equal("nutrientContent", ex.ParamName);
    }

    // ==========================================
    // IsHemoIron / IsNonHemoIron
    // ==========================================
    // Se comparan contra lo que responde NutrientContent, asi la prueba valida la
    // delegacion sin depender de como se construye un contenido hemo o no hemo.

    [Fact]
    public void IsHemoIron_DelegatesToNutrientContent()
    {
        var content = NewNutrientContent();
        var item = CreateItem(nutrientContent: content);

        Assert.Equal(content.IsHemo(), item.IsHemoIron());
    }

    [Fact]
    public void IsNonHemoIron_DelegatesToNutrientContent()
    {
        var content = NewNutrientContent();
        var item = CreateItem(nutrientContent: content);

        Assert.Equal(content.IsNonHemo(), item.IsNonHemoIron());
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var content = NewNutrientContent();
        var item = CreateItem(nutrientContent: content, isInhibitor: true);

        var primitives = item.ToPrimitives();

        Assert.Equal(item.Id, primitives.Id);
        Assert.Equal(item.Name, primitives.Name);
        Assert.Equal(item.IsInhibitor, primitives.IsInhibitor);
        Assert.Equal(item.Category.ToStringValue(), primitives.Category);
        Assert.NotNull(primitives.NutrientContent);
        Assert.Equivalent(content.ToPrimitives(), primitives.NutrientContent);
    }

    [Fact]
    public void ToPrimitives_ConvertsEveryFoodCategoryToItsStringValue()
    {
        foreach (var category in Enum.GetValues<FoodCategory>())
        {
            var item = CreateItem(category: category);

            var primitives = item.ToPrimitives();

            Assert.Equal(category.ToStringValue(), primitives.Category);
        }
    }
}
