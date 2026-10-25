namespace WebApplication1.Tests.TEST_INTEGRATION.NutritionDiary;

/// <summary>URLs del contexto NutritionDiary (api/nutritional-diary).</summary>
public class NutritionDiaryApi
{
    public const string BaseUrl = "/api/nutritional-diary"; 
    
    public const string FoodEntryUrl = BaseUrl + "/food-entry";
    public const string SearchFoodsUrl = BaseUrl + "/foods/search";
 
    public static string TodayUrl(string patientId) => $"{BaseUrl}/today/{patientId}";
    public static string HistoryUrl(string patientId) => $"{BaseUrl}/history/{patientId}";
    public static string FoodsByCategoryUrl(string category) => $"{BaseUrl}/foods/category/{category}";
    public static string FoodDetailsUrl(string foodItemId) => $"{BaseUrl}/foods/{foodItemId}";
}