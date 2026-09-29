namespace WebApplication1.Consultation.Domain.Models.Queries;

/// <summary>
/// Query para sincronización incremental de mensajes entre Room (SQLite) y MongoDB.
/// 
/// ¿Por qué existe?
/// - Permite que la app móvil en Kotlin (con Room) solo descargue los mensajes NUEVOS,
///   en lugar de descargar todos los mensajes de la consulta cada vez.
/// 
/// ¿Cómo se usa en Kotlin + Room?
/// 1. Room guarda localmente todos los mensajes ya descargados.
/// 2. La app obtiene de Room el timestamp del último mensaje guardado.
/// 3. Llama a este query con afterTimestamp = ese valor.
/// 4. El backend devuelve SOLO mensajes posteriores a esa fecha.
/// 5. La app guarda esos mensajes nuevos en Room.
/// 
/// Beneficio:
/// - Ahorro de datos móviles (solo se transfiere lo nuevo)
/// - Chat instantáneo (Room muestra datos locales mientras sincroniza)
/// - Sincronización offline-first
/// </summary>
public record GetMessagesAfterQuery(
    string ConsultationId,
    string RequesterId,
    long AfterTimestamp,
    int? Limit = null
);