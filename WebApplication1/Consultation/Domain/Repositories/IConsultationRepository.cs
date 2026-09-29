
namespace WebApplication1.Consultation.Domain.Repositories;


/// <summary>
/// Repositorio para el aggregate root Consultation.
/// 
/// Contexto: Bounded Context "Communication"
/// 
/// Notas importantes:
/// - Las consultas cerradas se ELIMINAN físicamente (no hay soft delete)
/// - Los mensajes van embebidos dentro de Consultation (no hay MessageRepository)
/// - Solo se manejan consultas ACTIVAS (OPEN), las cerradas no existen en la BD
/// </summary>
public interface IConsultationRepository
{
    /// <summary>
    /// Crea una nueva consulta en la base de datos.
    /// 
    /// ¿Cuándo se usa?
    /// - StartConsultationCommand: cuando la madre envía el primer mensaje
    /// </summary>
    /// <param name="consultation">Aggregate root Consultation completo (con su primer mensaje embebido)</param>
    Task SaveAsync(Models.Aggregate.Consultation consultation);

    /// <summary>
    /// Actualiza una consulta existente.
    /// 
    /// ¿Cuándo se usa?
    /// - AddMessageCommand: cuando se agrega un nuevo mensaje (madre o enfermera)
    /// </summary>
    /// <param name="consultation">Aggregate root Consultation con los nuevos mensajes agregados</param>
    Task UpdateAsync(Models.Aggregate.Consultation consultation);

    /// <summary>
    /// Busca una consulta por su ID.
    /// 
    /// ¿Cuándo se usa?
    /// - GetConsultationChatQuery: para mostrar el chat completo
    /// - AddMessageCommand: para verificar que la consulta existe y está activa
    /// - GetMessagesAfterQuery: para sincronización offline con Room
    /// - CloseConsultationCommand: para verificar que la consulta existe antes de eliminar
    /// </summary>
    /// <param name="consultationId">ID único de la consulta</param>
    /// <returns>La consulta si existe, null si no</returns>
    Task<Models.Aggregate.Consultation?> FindByIdAsync(string consultationId);

    /// <summary>
    /// Obtiene todas las consultas activas de una madre.
    /// 
    /// ¿Cuándo se usa?
    /// - GetOpenConsultationsByMotherQuery: para mostrar "Mis Consultas" en Ferova Family
    /// 
    /// Nota: Solo devuelve consultas que existen (todas están activas por definición)
    /// </summary>
    /// <param name="motherId">ID de la madre (usuario en Ferova Family)</param>
    /// <returns>Lista de consultas activas de la madre (puede ser vacía)</returns>
    Task<List<Models.Aggregate.Consultation>> FindOpenByMotherIdAsync(string motherId);

    /// <summary>
    /// Obtiene todas las consultas activas asignadas a una enfermera.
    /// 
    /// ¿Cuándo se usa?
    /// - GetOpenConsultationsByNurseQuery: para mostrar "Bandeja de Consultas" en Ferova Clinic
    /// 
    /// Nota:
    /// - Solo devuelve consultas que existen (todas están activas por definición)
    /// - El filtro por searchTerm (nombre de madre o paciente) se puede hacer en aplicación o MongoDB
    /// </summary>
    /// <param name="nurseId">ID de la enfermera (usuario en Ferova Clinic)</param>
    /// <returns>Lista de consultas activas asignadas a la enfermera (puede ser vacía)</returns>
    Task<List<Models.Aggregate.Consultation>> FindOpenByNurseIdAsync(string nurseId);

    /// <summary>
    /// Busca si existe una consulta activa para un paciente específico.
    /// 
    /// ¿Cuándo se usa?
    /// - StartConsultationCommand (validación): evita que la madre cree DOS consultas activas para el MISMO paciente
    /// 
    /// Flujo típico:
    /// 1. Madre intenta iniciar consulta para paciente "Mateo"
    /// 2. Se llama a findOpenByPatientId(patientId)
    /// 3. Si existe consulta activa → redirigir al chat existente (NO crear nueva)
    /// 4. Si no existe → crear nueva consulta con save()
    /// 
    /// ¿Por qué es importante?
    /// - Un paciente solo puede tener UNA consulta activa a la vez
    /// - Evita duplicados y confusión para la enfermera
    /// </summary>
    /// <param name="patientId">ID del paciente (registrado en Patient Management)</param>
    /// <returns>La consulta activa si existe, null si no</returns>
    Task<Models.Aggregate.Consultation?> FindOpenByPatientIdAsync(string patientId);

    /// <summary>
    /// Elimina físicamente una consulta de la base de datos.
    /// 
    /// ¿Cuándo se usa?
    /// - CloseConsultationCommand: cuando la enfermera cierra una consulta
    /// 
    /// Comportamiento:
    /// - Eliminación TOTAL del documento en MongoDB
    /// - NO hay soft delete (no se guarda historial)
    /// - NO hay campo "status" (las consultas cerradas NO existen)
    /// 
    /// Consecuencias:
    /// - Después de eliminar, la consulta desaparece de:
    ///   - GetOpenConsultationsByMother
    ///   - GetOpenConsultationsByNurse
    ///   - GetConsultationChat (devuelve 404)
    /// </summary>
    /// <param name="consultationId">ID de la consulta a eliminar</param>
    Task DeleteAsync(string consultationId);
}