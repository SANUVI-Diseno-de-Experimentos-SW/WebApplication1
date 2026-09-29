using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.Contexts.IAM.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.iam.infrastructure.Mapper;

using WebApplication1.Contexts.IAM.Domain.Models.Enums;
using WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;


/// <summary>
/// Mapper entre el dominio de usuario y el documento de MongoDB
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Convierte un documento de MongoDB a un objeto de dominio User
    /// </summary>
    public static User ToDomain(UserDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        // Convertir el ObjectId a string para el UserId
        var userId = document.Id ?? throw new InvalidOperationException("User document has no Id");

        // Convertir el string del rol a enum Role
        var role = RoleExtensions.FromString(document.Role);

        return new User(
            new UserId(userId),
            document.Name,
            document.Lastname,
            new Password(document.Password), // Nota: esto es la contraseña hasheada
            role,
            new Dni(document.Dni),
            new Email(document.Email),
            Phone.FromPersistence(document.Phone)
        );
    }

    /// <summary>
    /// Convierte un objeto de dominio User a un documento de MongoDB
    /// </summary>
    public static UserDocument ToPersistence(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var primitives = user.ToPrimitives();

        return new UserDocument
        {
            // NOTA: No asignamos Id porque MongoDB lo genera automáticamente
            // o lo toma del Id del usuario si es una actualización
            Name = primitives.Name,
            Lastname = primitives.Lastname,
            Password = primitives.Password, // Contraseña hasheada
            Role = primitives.Role,
            Dni = primitives.Dni,
            Email = primitives.Email,
            Phone = primitives.Phone,
            IsActive = primitives.IsActive,
            CreatedAt = primitives.CreatedAt,
            UpdatedAt = primitives.UpdatedAt
        };
    }

    /// <summary>
    /// Actualiza un documento existente con los datos del usuario
    /// </summary>
    public static void UpdateDocument(UserDocument document, User user)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var primitives = user.ToPrimitives();

        document.Name = primitives.Name;
        document.Lastname = primitives.Lastname;
        document.Password = primitives.Password;
        document.Role = primitives.Role;
        document.Dni = primitives.Dni;
        document.Email = primitives.Email;
        document.Phone = primitives.Phone;
        document.IsActive = primitives.IsActive;
        document.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Convierte múltiples documentos a una lista de usuarios
    /// </summary>
    public static IReadOnlyList<User> ToDomainList(IEnumerable<UserDocument> documents)
    {
        return documents.Select(ToDomain).ToList().AsReadOnly();
    }
}