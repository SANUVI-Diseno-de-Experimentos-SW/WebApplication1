using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.ValueObjects;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.patient_management.Infrastructure.Mapper;

public static class MedicalRecordMapper
{
    public static MedicalRecord ToDomain(MedicalRecordDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        var controls = (document.Controls ?? new List<ControlDocument>())
            .Select(c => new Control(
                c.Id,
                c.Date,
                new HemoglobinLevel(c.HemoglobinLevel)
            )).ToList();

        var antecedentes = (document.Antecedentes ?? new List<AntecedenteDocument>())
            .Select(a => new Antecedente(a.Type, a.Description)).ToList();

        var sintomas = document.Sintomas ?? new List<string>();

        return new MedicalRecord(
            document.MedicalRecordId,
            document.CreatedAt,
            new Weight(document.Weight),
            new Height(document.Height),
            GenderExtensions.FromString(document.Gender),
            new MotivoConsulta(document.MotivoConsulta),
            new Observaciones(document.Observaciones ?? string.Empty),
            document.PatientId,
            document.NurseId,
            document.HemoglobinLevel.HasValue 
                ? new HemoglobinLevel(document.HemoglobinLevel.Value) 
                : null,
            antecedentes,
            sintomas,
            controls
        );
    }

    public static MedicalRecordDocument ToPersistence(MedicalRecord medicalRecord)
    {
        var data = medicalRecord.ToPrimitives();

        return new MedicalRecordDocument
        {
            MedicalRecordId = data.Id,
            PatientId = data.PatientId,
            NurseId = data.NurseId,
            CreatedAt = data.CreatedAt,
            UpdatedAt = data.UpdatedAt,
            HemoglobinLevel = data.HemoglobinLevel,
            Weight = data.Weight,
            Height = data.Height,
            Gender = data.Gender,
            MotivoConsulta = data.MotivoConsulta,
            Observaciones = data.Observaciones,
            Antecedentes = data.Antecedentes.Select(a => new AntecedenteDocument 
            { 
                Type = a.Type, 
                Description = a.Description 
            }).ToList(),
            Sintomas = data.Sintomas,
            Controls = data.Controls.Select(c => new ControlDocument
            {
                Id = c.Id,
                Date = c.Date,
                HemoglobinLevel = c.HemoglobinLevel,
                AnemiaStatus = c.AnemiaStatus
            }).ToList()
        };
    }
}