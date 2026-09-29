﻿using WebApplication1.Consultation.Domain.Models.Commands;
using WebApplication1.Consultation.Domain.Models.Entities;
using WebApplication1.Consultation.Domain.Models.Enum;
using WebApplication1.Consultation.Domain.Repositories;
using WebApplication1.Consultation.Domain.Servicies;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.patient_management.Domain.Repositories;

namespace WebApplication1.Consultation.Application.Internal;


public class CommunicationCommandServiceImpl : ICommunicationCommandService
{
    private readonly IConsultationRepository _consultationRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;

    public CommunicationCommandServiceImpl(
        IConsultationRepository consultationRepository,
        IPatientRepository patientRepository,
        IUserRepository userRepository)
    {
        _consultationRepository = consultationRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
    }

    public async Task<object> AddMessageAsync(AddMessageCommand command)
    {
        var consultation = await _consultationRepository.FindByIdAsync(command.ConsultationId);

        if (consultation == null)
        {
            throw new Exception("Consultation not found");
        }

        // ✅ Verificar que la consulta está abierta
        if (!consultation.IsOpen())
        {
            throw new Exception("Consultation is closed");
        }

        var data = consultation.ToPrimitives();
        
        // ✅ Verificar que el remitente es parte de la consulta
        if (command.SenderId != data.MotherId && command.SenderId != data.NurseId)
        {
            throw new Exception("Sender is not part of this consultation");
        }

        var message = new Message(
            Guid.NewGuid().ToString(),
            command.SenderId,
            command.SenderRole,
            command.Content,
            DateTime.UtcNow
        );

        consultation.SendMessage(message);

        await _consultationRepository.UpdateAsync(consultation);

        return new
        {
            messageId = message.Id,
            message = "Message sent successfully"
        };
    }

    public async Task<object> StartConsultationAsync(StartConsultationCommand command)
    {
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);

        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();

        if (patientData.MotherId != command.MotherId)
        {
            throw new Exception("Patient does not belong to this mother");
        }

        if (string.IsNullOrEmpty(patientData.NurseId))
        {
            throw new Exception("Patient has no assigned nurse");
        }

        var existingConsultation = await _consultationRepository.FindOpenByPatientIdAsync(command.PatientId);

        if (existingConsultation != null)
        {
            // ✅ Si ya existe, devolver su ID para que el frontend redirija
            return new
            {
                consultationId = existingConsultation.Id,
                message = "Active consultation already exists for this patient",
                redirect = true
            };
        }

        var firstMessage = new Message(
            Guid.NewGuid().ToString(),
            command.MotherId,
            MessageSender.MOTHER,
            command.FirstMessageContent,
            DateTime.UtcNow
        );

        var consultation = new Domain.Models.Aggregate.Consultation(
            Guid.NewGuid().ToString(),
            command.PatientId,
            command.MotherId,
            patientData.NurseId,
            new List<Message> { firstMessage },
            DateTime.UtcNow,
            null
        );

        await _consultationRepository.SaveAsync(consultation);

        return new
        {
            consultationId = consultation.Id,
            message = "Consultation created successfully"
        };
    }

    public async Task<object> CloseConsultationAsync(CloseConsultationCommand command)
    {
        var consultation = await _consultationRepository.FindByIdAsync(command.ConsultationId);

        if (consultation == null)
        {
            throw new Exception("Consultation not found");
        }

        var data = consultation.ToPrimitives();

        if (data.NurseId != command.NurseId)
        {
            throw new Exception("Only assigned nurse can close consultation");
        }

        var nurseMessages = data.Messages.Where(m => m.SenderRole == "NURSE").ToList();

        if (nurseMessages.Count == 0)
        {
            throw new Exception("Consultation must contain at least one nurse response before closing");
        }

        // ✅ ELIMINAR FÍSICAMENTE la consulta de MongoDB
        await _consultationRepository.DeleteAsync(command.ConsultationId);

        return new
        {
            message = "Consultation closed successfully"
        };
    }
}