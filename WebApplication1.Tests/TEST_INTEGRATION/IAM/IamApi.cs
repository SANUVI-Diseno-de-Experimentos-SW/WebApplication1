namespace WebApplication1.Tests.TEST_INTEGRATION.IAM;

/// <summary>
/// URLs y datos de prueba del contexto IAM (usuarios, login, reset de password).
/// Cada bounded context nuevo tendra su propio archivo igual (NutritionDiaryApi.cs, etc.).
/// </summary>
public static class IamApi
{
    public const string RegisterMotherUrl = "/api/users/register/mother";
    public const string RegisterStaffUrl = "/api/users/register/staff";
    public const string LoginUrl = "/api/users/login";
    public const string RequestCodeUrl = "/api/users/password/request-code";
    public const string VerifyCodeUrl = "/api/users/password/verify-code";
    public const string ResetPasswordUrl = "/api/users/password/reset";

    public const string ValidPassword = "qwerty-ar123A$";

    // ---------- datos unicos (cada test usa los suyos, asi no chocan entre si) ----------

    public static string UniqueDni() => Random.Shared.Next(10_000_000, 99_999_999).ToString();
    public static string UniquePhone() => "9" + Random.Shared.Next(10_000_000, 99_999_999);
    public static string UniqueEmail() => $"user{Guid.NewGuid():N}@example.com";

    public static object NewMother(string dni, string? email = null, string password = ValidPassword) => new
    {
        Name = "John",
        Lastname = "Doe",
        Dni = dni,
        Email = email ?? UniqueEmail(),
        Phone = UniquePhone(),
        Password = password
    };

    public static object NewStaff(string dni, string role, string? email = null) => new
    {
        Name = "Jane",
        Lastname = "Smith",
        Dni = dni,
        Email = email ?? UniqueEmail(),
        Phone = UniquePhone(),
        Password = ValidPassword,
        Role = role
    };
}
