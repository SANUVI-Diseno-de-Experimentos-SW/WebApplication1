﻿using WebApplication1.Consultation.Domain.Models.Queries;
using WebApplication1.Consultation.Domain.Repositories;
using WebApplication1.Consultation.Domain.Servicies;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.patient_management.Domain.Repositories;

namespace WebApplication1.Consultation.Application.Internal;

public class CommunicationQueryServiceImpl : ICommunicationQueryService
{
    private readonly IConsultationRepository _consultationRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;

    public CommunicationQueryServiceImpl(
        IConsultationRepository consultationRepository,
        IPatientRepository patientRepository,
        IUserRepository userRepository)
    {
        _consultationRepository = consultationRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
    }

    public async Task<object> GetPatientsWithNurseAssignmentAsync(GetPatientsWithNurseAssignmentQuery query)
    {
        var patients = await _patientRepository.FindByMotherIdAsync(query.MotherId);

        var result = new List<object>();

        foreach (var patient in patients)
        {
            var data = patient.ToPrimitives();
            string? nurseName = null;

            if (!string.IsNullOrEmpty(data.NurseId))
            {
                var nurse = await _userRepository.FindNurseByIdAsync(data.NurseId);
                if (nurse != null)
                {
                    var nurseData = nurse.ToPrimitives();
                    nurseName = nurseData.Name;
                }
            }

            result.Add(new
            {
                patientId = data.Id,
                patientName = $"{data.Name} {data.LastName}",
                hasNurseAssigned = !string.IsNullOrEmpty(data.NurseId),
                nurseId = data.NurseId,
                nurseName = nurseName
            });
        }

        return result;
    }

    public async Task<object> GetNurseInfoForConsultationAsync(GetNurseInfoForConsultationQuery query)
    {
        var patient = await _patientRepository.FindByIdAsync(query.PatientId);

        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();

        if (string.IsNullOrEmpty(patientData.NurseId))
        {
            throw new Exception("This patient no longer has an assigned nurse");
        }

        var nurse = await _userRepository.FindNurseByIdAsync(patientData.NurseId);
        var nurseData = nurse?.ToPrimitives();

        return new
        {
            patientId = patientData.Id,
            patientName = $"{patientData.Name} {patientData.LastName}",
            nurseId = patientData.NurseId,
            nurseName = nurseData?.Name
        };
    }

    public async Task<object> GetConsultationChatAsync(GetConsultationChatQuery query)
    {
        var consultation = await _consultationRepository.FindByIdAsync(query.ConsultationId);

        if (consultation == null)
        {
            throw new Exception("Consultation not found");
        }

        var data = consultation.ToPrimitives();

        var authorized = data.MotherId == query.RequesterId || data.NurseId == query.RequesterId;

        if (!authorized)
        {
            throw new Exception("Not authorized");
        }

        var sortedMessages = data.Messages.OrderBy(m => m.SentAt).ToList();

        // ✅ Formatear respuesta como espera el frontend ChatResponse
        return new
        {
            consultationId = data.Id,  // ← consultationId (NO id)
            messages = sortedMessages.Select(msg => new
            {
                id = msg.Id,
                senderId = msg.SenderId,
                senderRole = msg.SenderRole == "NURSE" ? "NURSE" : "MOTHER",
                content = msg.Content,
                sentAt = msg.SentAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            }).ToList()
        };
    }

    public async Task<object> GetOpenConsultationsByMotherAsync(GetOpenConsultationsByMotherQuery query)
    {
        var consultations = await _consultationRepository.FindOpenByMotherIdAsync(query.MotherId);

        var enrichedConsultations = new List<object>();

        foreach (var consultation in consultations)
        {
            var consultationData = consultation.ToPrimitives();

            // Obtener datos del paciente
            var patient = await _patientRepository.FindByIdAsync(consultationData.PatientId);
            var patientData = patient?.ToPrimitives();

            // Obtener datos de la enfermera
            var nurse = await _userRepository.FindNurseByIdAsync(consultationData.NurseId);
            var nurseData = nurse?.ToPrimitives();

            // Obtener último mensaje
            var messages = consultationData.Messages;
            var lastMessage = messages.Count > 0 ? messages.Last() : null;

            // ✅ Formato CORRECTO que espera el frontend
            enrichedConsultations.Add(new
            {
                consultationId = consultationData.Id,  // ← ¡IMPORTANTE! "consultationId" NO "id"
                patientId = consultationData.PatientId,
                patientName = patientData != null ? $"{patientData.Name} {patientData.LastName}".Trim() : "Unknown",
                nurseId = consultationData.NurseId,    // ← ¡IMPORTANTE! "nurseId" separado
                nurseName = nurseData?.Name ?? "Unknown",  // ← ¡IMPORTANTE! "nurseName" separado
                // ✅ También incluir los campos que el mapper puede usar
                lastMessage = lastMessage?.Content,
                lastMessageDate = lastMessage?.SentAt,
                lastMessageSenderRole = lastMessage?.SenderRole,
                createdAt = consultationData.CreatedAt,
                messageCount = messages.Count
            });
        }

        return enrichedConsultations;
    }
    public async Task<object> GetOpenConsultationsByNurseAsync(GetOpenConsultationsByNurseQuery query)
    {
        var consultations = await _consultationRepository.FindOpenByNurseIdAsync(query.NurseId);

        var assignedPatients = await _patientRepository.FindByNurseIdAsync(query.NurseId);

        var enrichedConsultations = new List<object>();

        foreach (var consultation in consultations)
        {
            var consultationData = consultation.ToPrimitives();

            var patient = await _patientRepository.FindByIdAsync(consultationData.PatientId);
            var patientData = patient?.ToPrimitives();

            var mother = await _userRepository.FindMotherByIdAsync(consultationData.MotherId);
            var motherData = mother?.ToPrimitives();

            var messages = consultationData.Messages;
            var lastMessage = messages.Count > 0 ? messages.Last() : null;

            enrichedConsultations.Add(new
            {
                consultationId = consultationData.Id,
                patientId = consultationData.PatientId,
                patientName = patientData != null ? $"{patientData.Name} {patientData.LastName}".Trim() : "Unknown",
                motherId = consultationData.MotherId,
                motherName = motherData?.Name ?? "Unknown",
                nurseId = consultationData.NurseId,
                lastMessage = lastMessage?.Content,
                lastMessageDate = lastMessage?.SentAt,
                createdAt = consultationData.CreatedAt,
                messageCount = messages.Count
            });
        }

        var hasAssignedPatients = assignedPatients.Count > 0;
        var hasConsultations = consultations.Count > 0;

        if (!hasAssignedPatients)
        {
            return new
            {
                consultations = new List<object>(),
                message = "No tienes pacientes asignados en tu cartera",
                detail = "Puedes asignar pacientes a tu cartera desde el módulo de pacientes. Ve a 'Pacientes' y selecciona 'Asignar a mi cartera'.",
                action = "Asignar pacientes",
                status = "SIN_PACIENTES"
            };
        }

        if (!hasConsultations)
        {
            return new
            {
                consultations = new List<object>(),
                message = "No tienes consultas activas aún",
                detail = "Las madres pueden iniciar consultas para sus hijos. Cuando una madre inicie una consulta, aparecerá aquí.",
                status = "NO_CONSULTAS"
            };
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchLower = query.SearchTerm.ToLower().Trim();

            var filteredConsultations = enrichedConsultations
                .Where(c =>
                {
                    var cType = c.GetType();
                    var patientName = cType.GetProperty("patientName")?.GetValue(c)?.ToString()?.ToLower() ?? "";
                    var motherName = cType.GetProperty("motherName")?.GetValue(c)?.ToString()?.ToLower() ?? "";
                    return patientName.Contains(searchLower) || motherName.Contains(searchLower);
                })
                .ToList();

            if (filteredConsultations.Count == 0)
            {
                return new
                {
                    consultations = new List<object>(),
                    message = "No se encontraron consultas que coincidan con tu búsqueda",
                    detail = $"No hay consultas con \"{query.SearchTerm}\" en el nombre del paciente o de la madre. Intenta con otro término.",
                    searchTerm = query.SearchTerm,
                    status = "BUSQUEDA_SIN_RESULTADOS"
                };
            }

            return filteredConsultations;
        }

        return enrichedConsultations;
    }

    public async Task<object> GetMessagesAfterAsync(GetMessagesAfterQuery query)
    {
        var consultation = await _consultationRepository.FindByIdAsync(query.ConsultationId);

        if (consultation == null)
        {
            throw new Exception("Consultation not found");
        }

        var data = consultation.ToPrimitives();

        var authorized = data.MotherId == query.RequesterId || data.NurseId == query.RequesterId;

        if (!authorized)
        {
            throw new Exception("Not authorized");
        }

        var limit = query.Limit ?? 100;

        var filteredMessages = data.Messages
            .Where(m => m.SentAt.ToUniversalTime() > DateTimeOffset.FromUnixTimeMilliseconds(query.AfterTimestamp).UtcDateTime)
            .OrderBy(m => m.SentAt)
            .Take(limit)
            .ToList();

        // ✅ CORREGIDO: comparar string con string (NURSE en mayúsculas)
        return filteredMessages.Select(msg => new
        {
            id = msg.Id,
            text = msg.Content,
            isFromNurse = msg.SenderRole == "NURSE",
            time = msg.SentAt.ToString("HH:mm")
        }).ToList();
    }
}