using System.Runtime.Intrinsics.Arm;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.Contexts.IAM.Domain.Models.Enums;
using WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.Contexts.IAM.Infrastructure.Persitencia.MongoDb.Models;
using WebApplication1.iam.infrastructure.Mapper;
namespace WebApplication1.iam.infrastructure.Persitencia.MongoDb.Repositories;


/// <summary>
/// Implementación del repositorio de usuarios con MongoDB
/// </summary>
public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDocument> _usersCollection;
    private readonly ILogger<MongoUserRepository> _logger;

    public MongoUserRepository(
        IMongoDatabase database,
        ILogger<MongoUserRepository> logger)
    {
        _usersCollection = database.GetCollection<UserDocument>("users");
        _logger = logger;

        // Crear índices
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        try
        {
            // Índice único para Email
            var emailIndex = new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(x => x.Email),
                new CreateIndexOptions { Unique = true }
            );

            // Índice único para DNI
            var dniIndex = new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(x => x.Dni),
                new CreateIndexOptions { Unique = true }
            );

            // Índice único para Phone
            var phoneIndex = new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(x => x.Phone),
                new CreateIndexOptions { Unique = true }
            );

            // Índice para Role
            var roleIndex = new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(x => x.Role)
            );

            _usersCollection.Indexes.CreateMany(new[]
            {
                emailIndex,
                dniIndex,
                phoneIndex,
                roleIndex
            });

            _logger.LogInformation("Índices de MongoDB creados exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al crear índices (pueden ya existir)");
        }
    }

    // ==========================================
    // CRUD BÁSICO
    // ==========================================

    public async Task<User> SaveAsync(User user)
    {
        try
        {
            var document = UserMapper.ToPersistence(user);

            // Buscar si existe
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Email, document.Email);
            var existing = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (existing != null)
            {
                // Actualizar documento existente
                document.Id = existing.Id;
                document.CreatedAt = existing.CreatedAt;
                document.UpdatedAt = DateTime.UtcNow;

                await _usersCollection.ReplaceOneAsync(
                    filter,
                    document,
                    new ReplaceOptions { IsUpsert = false }
                );

                _logger.LogInformation("Usuario actualizado: {Email}", document.Email);
            }
            else
            {
                // Crear nuevo documento
                await _usersCollection.InsertOneAsync(document);
                _logger.LogInformation("Usuario creado: {Email}", document.Email);
            }

            // Actualizar el ID del usuario con el ObjectId de MongoDB
            // Nota: Esto requeriría una actualización en el dominio, lo dejamos como está
            return user;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogError(ex, "Error de duplicado al guardar usuario");
            throw new InvalidOperationException("Ya existe un usuario con este email, DNI o teléfono");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar usuario");
            throw;
        }
    }

    public async Task<User?> FindByIdAsync(UserId id)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Id, id.Value);
            var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (document == null)
                return null;

            return UserMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por ID: {UserId}", id.Value);
            throw;
        }
    }

    public async Task<User?> FindByEmailAsync(Contexts.IAM.Domain.Models.ValueObjects.Email email)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Email, email.Value);
            var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (document == null)
                return null;

            return UserMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por email: {Email}", email.Value);
            throw;
        }
    }

    public async Task<User?> FindByDniAsync(Dni dni)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Dni, dni.Value);
            var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (document == null)
                return null;

            return UserMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por DNI: {Dni}", dni.Value);
            throw;
        }
    }

    public async Task<User?> FindByPhoneAsync(Phone phone)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Phone, phone.Value);
            var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (document == null)
                return null;

            return UserMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por teléfono: {Phone}", phone.Value);
            throw;
        }
    }

    public async Task<IReadOnlyList<User>> FindByRoleAsync(Role role)
    {
        try
        {
            var roleString = role.ToStringValue();
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Role, roleString);
            var documents = await _usersCollection.Find(filter).ToListAsync();

            return UserMapper.ToDomainList(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuarios por rol: {Role}", role);
            throw;
        }
    }

    public async Task<IReadOnlyList<User>> FindAllAsync()
    {
        try
        {
            var documents = await _usersCollection.Find(_ => true).ToListAsync();
            return UserMapper.ToDomainList(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los usuarios");
            throw;
        }
    }

    // ==========================================
    // BÚSQUEDAS ESPECÍFICAS
    // ==========================================

    public async Task<User?> FindMotherByDniAsync(string dni)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.And(
                Builders<UserDocument>.Filter.Eq(x => x.Role, Role.Mother.ToStringValue()),
                Builders<UserDocument>.Filter.Eq(x => x.Dni, dni)
            );

            var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return document == null ? null : UserMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar madre por DNI: {Dni}", dni);
            throw;
        }
    }

    public async Task<User?> FindNurseByIdAsync(string id)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.And(
                Builders<UserDocument>.Filter.Eq(x => x.Role, Role.Nurse.ToStringValue()),
                Builders<UserDocument>.Filter.Eq(x => x.Id, id)
            );

            var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return document == null ? null : UserMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar enfermera por ID: {Id}", id);
            throw;
        }
    }

    public async Task<User?> FindMotherByIdAsync(string id)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.And(
                Builders<UserDocument>.Filter.Eq(x => x.Role, Role.Mother.ToStringValue()),
                Builders<UserDocument>.Filter.Eq(x => x.Id, id)
            );

            var document = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return document == null ? null : UserMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar madre por ID: {Id}", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<User>> FindAllNursesAsync()
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Role, Role.Nurse.ToStringValue());
            var documents = await _usersCollection.Find(filter).ToListAsync();

            return UserMapper.ToDomainList(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las enfermeras");
            throw;
        }
    }

    public async Task<IReadOnlyList<User>> FindMothersBySearchTermAsync(string searchTerm)
    {
        try
        {
            // Buscar por DNI (coincidencia parcial)
            var filter = Builders<UserDocument>.Filter.And(
                Builders<UserDocument>.Filter.Eq(x => x.Role, Role.Mother.ToStringValue()),
                Builders<UserDocument>.Filter.Regex(x => x.Dni, new BsonRegularExpression(searchTerm, "i"))
            );

            var documents = await _usersCollection.Find(filter).ToListAsync();
            return UserMapper.ToDomainList(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar madres por término: {SearchTerm}", searchTerm);
            throw;
        }
    }

    // ==========================================
    // RESET DE CONTRASEÑA
    // ==========================================

    public async Task SaveResetCodeAsync(Contexts.IAM.Domain.Models.ValueObjects.Email email, string code, DateTime expiresAt)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Email, email.Value);
            var update = Builders<UserDocument>.Update
                .Set(x => x.ResetCode, code)
                .Set(x => x.ResetCodeExpiresAt, expiresAt)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _usersCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No se encontró usuario para guardar código de reset: {Email}", email.Value);
                throw new InvalidOperationException($"User with email {email.Value} not found");
            }

            _logger.LogInformation("Código de reset guardado para: {Email}", email.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar código de reset para: {Email}", email.Value);
            throw;
        }
    }

    public async Task<bool> ValidateResetCodeAsync(Contexts.IAM.Domain.Models.ValueObjects.Email email, string code)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.And(
                Builders<UserDocument>.Filter.Eq(x => x.Email, email.Value),
                Builders<UserDocument>.Filter.Eq(x => x.ResetCode, code),
                Builders<UserDocument>.Filter.Gt(x => x.ResetCodeExpiresAt, DateTime.UtcNow)
            );

            var count = await _usersCollection.CountDocumentsAsync(filter);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar código de reset para: {Email}", email.Value);
            throw;
        }
    }

    public async Task UpdatePasswordAsync(Contexts.IAM.Domain.Models.ValueObjects.Email email, string newPassword)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Email, email.Value);
            var update = Builders<UserDocument>.Update
                .Set(x => x.Password, newPassword)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _usersCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No se encontró usuario para actualizar contraseña: {Email}", email.Value);
                throw new InvalidOperationException($"User with email {email.Value} not found");
            }

            _logger.LogInformation("Contraseña actualizada para: {Email}", email.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contraseña para: {Email}", email.Value);
            throw;
        }
    }

    public async Task ClearResetCodeAsync(Contexts.IAM.Domain.Models.ValueObjects.Email email)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Email, email.Value);
            var update = Builders<UserDocument>.Update
                .Set(x => x.ResetCode, null)
                .Set(x => x.ResetCodeExpiresAt, null)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _usersCollection.UpdateOneAsync(filter, update);
            _logger.LogInformation("Código de reset limpiado para: {Email}", email.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar código de reset para: {Email}", email.Value);
            throw;
        }
    }

    // ==========================================
    // MÉTODOS ADICIONALES
    // ==========================================

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedByRoleAsync(
        Role role,
        int page,
        int pageSize)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var roleString = role.ToStringValue();
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Role, roleString);

            var totalCount = await _usersCollection.CountDocumentsAsync(filter);

            var documents = await _usersCollection
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .SortBy(x => x.Name)
                .ThenBy(x => x.Lastname)
                .ToListAsync();

            var users = UserMapper.ToDomainList(documents);
            return (users, (int)totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios paginados por rol: {Role}", role);
            throw;
        }
    }

    public async Task<bool> ExistsByEmailAsync(Contexts.IAM.Domain.Models.ValueObjects.Email email)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Email, email.Value);
            var count = await _usersCollection.CountDocumentsAsync(filter);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia por email: {Email}", email.Value);
            throw;
        }
    }

    public async Task<bool> ExistsByDniAsync(Dni dni)
    {
        try
        {
            var filter = Builders<UserDocument>.Filter.Eq(x => x.Dni, dni.Value);
            var count = await _usersCollection.CountDocumentsAsync(filter);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia por DNI: {Dni}", dni.Value);
            throw;
        }
    }
}