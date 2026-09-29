using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.iam.Interfaces.Resources;

namespace WebApplication1.iam.Interfaces.Assemblers;

using WebApplication1.Contexts.IAM.Interfaces.Resources;


/// <summary>
/// Ensamblador de recursos de usuario
/// </summary>
public static class UserResourceAssembler
{
    /// <summary>
    /// Convierte un User de dominio a UserResource
    /// </summary>
    public static UserResource ToResource(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var primitives = user.ToPrimitives();

        return new UserResource
        {
            Id = primitives.Id,
            Name = primitives.Name,
            Lastname = primitives.Lastname,
            Role = primitives.Role,
            Dni = primitives.Dni,
            Email = primitives.Email,
            Phone = primitives.Phone
        };
    }

    /// <summary>
    /// Convierte una lista de usuarios a recursos
    /// </summary>
    public static IReadOnlyList<UserResource> ToResources(IEnumerable<User> users)
    {
        return users.Select(ToResource).ToList().AsReadOnly();
    }
}