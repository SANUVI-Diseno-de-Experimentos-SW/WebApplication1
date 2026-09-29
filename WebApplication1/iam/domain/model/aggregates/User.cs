using WebApplication1.Contexts.IAM.Domain.Models.Enums;
using WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;

namespace WebApplication1.Contexts.IAM.Domain.Models;

/// <summary>
/// Modelo de Usuario (Agregado raíz)
/// </summary>
public class User
{
    public UserId Id { get; private set; }
    public string Name { get; private set; }
    public string Lastname { get; private set; }
    public Password Password { get; private set; }
    public Role Role { get; private set; }
    public Dni Dni { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Constructor privado para serialización
    private User() { }

    public User(
        UserId id,
        string name,
        string lastname,
        Password password,
        Role role,
        Dni dni,
        Email email,
        Phone phone)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = ValidateName(name);
        Lastname = ValidateLastname(lastname);
        Password = password ?? throw new ArgumentNullException(nameof(password));
        Role = role;
        Dni = dni ?? throw new ArgumentNullException(nameof(dni));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    // ==========================================
    // VALIDACIONES
    // ==========================================

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        return name.Trim();
    }

    private static string ValidateLastname(string lastname)
    {
        if (string.IsNullOrWhiteSpace(lastname))
            throw new ArgumentException("Lastname is required", nameof(lastname));
        return lastname.Trim();
    }

    // ==========================================
    // MÉTODOS DE DOMINIO (Comportamiento)
    // ==========================================

    public void ChangePassword(Password newPassword)
    {
        Password = newPassword ?? throw new ArgumentNullException(nameof(newPassword));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string name, string lastname, Phone phone)
    {
        Name = ValidateName(name);
        Lastname = ValidateLastname(lastname);
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(Role newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasRole(Role role) => Role == role;

    public bool HasAnyRole(params Role[] roles) => roles.Contains(Role);

    public bool IsStaff() => Role == Role.Nurse || Role == Role.Admin;

    public bool IsMother() => Role == Role.Mother;

    public bool CanManageUsers() => Role == Role.Admin;

    // ==========================================
    // CONVERSIÓN A PRIMITIVOS (DTO)
    // ==========================================

    public UserPrimitives ToPrimitives()
    {
        return new UserPrimitives
        {
            Id = Id.Value,
            Name = Name,
            Lastname = Lastname,
            Password = Password.Value,
            Role = Role.ToStringValue(),
            Dni = Dni.Value,
            Email = Email.Value,
            Phone = Phone.Value,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            IsActive = IsActive
        };
    }

    // ==========================================
    // DTO para conversión
    // ==========================================

    public class UserPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}