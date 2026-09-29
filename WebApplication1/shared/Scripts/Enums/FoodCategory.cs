namespace WebApplication1.shared.Scripts.Enums;

public enum FoodCategory
{
    MEAT,
    FISH,
    LEGUME,
    VEGETABLE,
    GRAIN,
    FRUIT,
    DAIRY,
    BEVERAGE
}

public enum IronType
{
    hemo,
    [System.Runtime.Serialization.EnumMember(Value = "no-hemo")]
    no_hemo
}