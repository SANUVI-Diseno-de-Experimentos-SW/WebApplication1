using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.Contexts.IAM.Domain.Models.Enums;
using WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;

namespace WebApplication1.Tests;

public class UserTest
{
     // ==========================================
    // HELPERS
    // ==========================================
    // IMPORTANTE: si tus value objects se crean distinto (por ejemplo Email.Create("...")),
    // solo tienes que ajustar estos helpers; el resto de las pruebas no cambia.
 
    private static UserId NewId() => new("user-001");
    private static Password NewPassword(string value = "qwerty-ar123A$") => new(value);
    private static Dni NewDni() => new("12345678");
    private static Email NewEmail() => new("john.doe@example.com");
    private static Phone NewPhone(string value = "999888777") => new(value);
 
    private static User CreateUser(
        string name = "John",
        string lastname = "Doe",
        Role role = Role.Admin)
    {
        return new User(
            NewId(),
            name,
            lastname,
            NewPassword(),
            role,
            NewDni(),
            NewEmail(),
            NewPhone());
    }
 
    // ==========================================
    // CONSTRUCTOR
    // ==========================================
 
    [Fact]
    public void Constructor_WithValidData_CreatesUserSuccessfully()
    {
        var id = NewId();
        var password = NewPassword();
        var dni = NewDni();
        var email = NewEmail();
        var phone = NewPhone();
 
        var user = new User(id, "John", "Doe", password, Role.Admin, dni, email, phone);
 
        Assert.NotNull(user);
        Assert.Same(id, user.Id);
        Assert.Equal("John", user.Name);
        Assert.Equal("Doe", user.Lastname);
        Assert.Same(password, user.Password);
        Assert.Equal(Role.Admin, user.Role);
        Assert.Same(dni, user.Dni);
        Assert.Same(email, user.Email);
        Assert.Same(phone, user.Phone);
    }
    
    [Fact]
    public void Constructor_SetsDefaultState_ActiveWithCreatedAtAndNoUpdatedAt()
    {
        var before = DateTime.UtcNow;
 
        var user = CreateUser();
 
        var after = DateTime.UtcNow;
        Assert.True(user.IsActive);
        Assert.Null(user.UpdatedAt);
        Assert.InRange(user.CreatedAt, before, after);
    }
    
    [Fact]
    public void Constructor_TrimsNameAndLastname()
    {
        var user = CreateUser(name: "  John  ", lastname: "  Doe  ");
 
        Assert.Equal("John", user.Name);
        Assert.Equal("Doe", user.Lastname);
    }
    
    [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ThrowsArgumentException(string? invalidName)
        {
            var ex = Assert.ThrowsAny<ArgumentException>(() => CreateUser(name: invalidName!));
     
            Assert.Equal("name", ex.ParamName);
        }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidLastname_ThrowsArgumentException(string? invalidLastname)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateUser(lastname: invalidLastname!));
 
        Assert.Equal("lastname", ex.ParamName);
    }
    
    [Fact]
    public void Constructor_WithNullId_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new User(null!, "John", "Doe", NewPassword(), Role.Admin, NewDni(), NewEmail(), NewPhone()));
 
        Assert.Equal("id", ex.ParamName);
    }
 
    [Fact]
    public void Constructor_WithNullPassword_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new User(NewId(), "John", "Doe", null!, Role.Admin, NewDni(), NewEmail(), NewPhone()));
 
        Assert.Equal("password", ex.ParamName);
    }
 
    [Fact]
    public void Constructor_WithNullDni_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new User(NewId(), "John", "Doe", NewPassword(), Role.Admin, null!, NewEmail(), NewPhone()));
 
        Assert.Equal("dni", ex.ParamName);
    }
 
    [Fact]
    public void Constructor_WithNullEmail_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new User(NewId(), "John", "Doe", NewPassword(), Role.Admin, NewDni(), null!, NewPhone()));
 
        Assert.Equal("email", ex.ParamName);
    }
 
    [Fact]
    public void Constructor_WithNullPhone_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new User(NewId(), "John", "Doe", NewPassword(), Role.Admin, NewDni(), NewEmail(), null!));
 
        Assert.Equal("phone", ex.ParamName);
    }
 
    // ==========================================
    // ChangePassword
    // ==========================================
 
    [Fact]
    public void ChangePassword_WithValidPassword_UpdatesPasswordAndUpdatedAt()
    {
        var user = CreateUser();
        var newPassword = NewPassword("newSecurePassword456$");
        var before = DateTime.UtcNow;
 
        user.ChangePassword(newPassword);
 
        Assert.Same(newPassword, user.Password);
        Assert.NotNull(user.UpdatedAt);
        Assert.InRange(user.UpdatedAt!.Value, before, DateTime.UtcNow);
    }
 
    [Fact]
    public void ChangePassword_WithNull_ThrowsArgumentNullException()
    {
        var user = CreateUser();
 
        var ex = Assert.Throws<ArgumentNullException>(() => user.ChangePassword(null!));
 
        Assert.Equal("newPassword", ex.ParamName);
    }
 
    // ==========================================
    // UpdateProfile
    // ==========================================
 
    [Fact]
    public void UpdateProfile_WithValidData_UpdatesFieldsAndUpdatedAt()
    {
        var user = CreateUser();
        var newPhone = NewPhone("911222333");
        var before = DateTime.UtcNow;
 
        user.UpdateProfile("  Jane  ", "  Smith  ", newPhone);
 
        Assert.Equal("Jane", user.Name);
        Assert.Equal("Smith", user.Lastname);
        Assert.Same(newPhone, user.Phone);
        Assert.NotNull(user.UpdatedAt);
        Assert.InRange(user.UpdatedAt!.Value, before, DateTime.UtcNow);
    }
 
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        var user = CreateUser();
 
        Assert.ThrowsAny<ArgumentException>(() => user.UpdateProfile(invalidName!, "Smith", NewPhone()));
    }
 
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_WithInvalidLastname_ThrowsArgumentException(string? invalidLastname)
    {
        var user = CreateUser();
 
        Assert.ThrowsAny<ArgumentException>(() => user.UpdateProfile("Jane", invalidLastname!, NewPhone()));
    }
 
    [Fact]
    public void UpdateProfile_WithNullPhone_ThrowsArgumentNullException()
    {
        var user = CreateUser();
 
        var ex = Assert.Throws<ArgumentNullException>(() => user.UpdateProfile("Jane", "Smith", null!));
 
        Assert.Equal("phone", ex.ParamName);
    }
 
    // ==========================================
    // Activate / Deactivate
    // ==========================================
 
    [Fact]
    public void Deactivate_SetsIsActiveToFalseAndUpdatesUpdatedAt()
    {
        var user = CreateUser();
 
        user.Deactivate();
 
        Assert.False(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }
 
    [Fact]
    public void Activate_AfterDeactivate_SetsIsActiveToTrueAndUpdatesUpdatedAt()
    {
        var user = CreateUser();
        user.Deactivate();
 
        user.Activate();
 
        Assert.True(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }
 
    // ==========================================
    // Roles
    // ==========================================
 
    [Fact]
    public void ChangeRole_UpdatesRoleAndUpdatedAt()
    {
        var user = CreateUser(role: Role.Mother);
 
        user.ChangeRole(Role.Nurse);
 
        Assert.Equal(Role.Nurse, user.Role);
        Assert.NotNull(user.UpdatedAt);
    }
 
    [Fact]
    public void HasRole_WithMatchingRole_ReturnsTrue()
    {
        var user = CreateUser(role: Role.Nurse);
 
        Assert.True(user.HasRole(Role.Nurse));
    }
 
    [Fact]
    public void HasRole_WithDifferentRole_ReturnsFalse()
    {
        var user = CreateUser(role: Role.Nurse);
 
        Assert.False(user.HasRole(Role.Admin));
    }
 
    [Fact]
    public void HasAnyRole_WhenRoleIsInList_ReturnsTrue()
    {
        var user = CreateUser(role: Role.Mother);
 
        Assert.True(user.HasAnyRole(Role.Nurse, Role.Mother));
    }
 
    [Fact]
    public void HasAnyRole_WhenRoleIsNotInList_ReturnsFalse()
    {
        var user = CreateUser(role: Role.Mother);
 
        Assert.False(user.HasAnyRole(Role.Nurse, Role.Admin));
    }
 
    [Theory]
    [InlineData(Role.Nurse, true)]
    [InlineData(Role.Admin, true)]
    [InlineData(Role.Mother, false)]
    public void IsStaff_ReturnsExpectedValueForRole(Role role, bool expected)
    {
        var user = CreateUser(role: role);
 
        Assert.Equal(expected, user.IsStaff());
    }
 
    [Theory]
    [InlineData(Role.Mother, true)]
    [InlineData(Role.Nurse, false)]
    [InlineData(Role.Admin, false)]
    public void IsMother_ReturnsExpectedValueForRole(Role role, bool expected)
    {
        var user = CreateUser(role: role);
 
        Assert.Equal(expected, user.IsMother());
    }
 
    [Theory]
    [InlineData(Role.Admin, true)]
    [InlineData(Role.Nurse, false)]
    [InlineData(Role.Mother, false)]
    public void CanManageUsers_ReturnsExpectedValueForRole(Role role, bool expected)
    {
        var user = CreateUser(role: role);
 
        Assert.Equal(expected, user.CanManageUsers());
    }
 
    // ==========================================
    // ToPrimitives
    // ==========================================
 
    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var user = CreateUser();
 
        var primitives = user.ToPrimitives();
 
        Assert.Equal(user.Id.Value, primitives.Id);
        Assert.Equal(user.Name, primitives.Name);
        Assert.Equal(user.Lastname, primitives.Lastname);
        Assert.Equal(user.Password.Value, primitives.Password);
        Assert.Equal(user.Role.ToStringValue(), primitives.Role);
        Assert.Equal(user.Dni.Value, primitives.Dni);
        Assert.Equal(user.Email.Value, primitives.Email);
        Assert.Equal(user.Phone.Value, primitives.Phone);
        Assert.Equal(user.CreatedAt, primitives.CreatedAt);
        Assert.Equal(user.UpdatedAt, primitives.UpdatedAt);
        Assert.Equal(user.IsActive, primitives.IsActive);
    }

}