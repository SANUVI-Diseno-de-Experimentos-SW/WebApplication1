using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.ValueObjects;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.patient_management.Infrastructure.Mapper;

public static class PatientMapper
{
    public static Patient ToDomain(PatientDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        // ✅ Usar PatientId (no id)
        return new Patient(
            document.PatientId,
            document.Name,
            document.LastName,
            new BirthDate(document.BirthDate),
            new Weight(document.CurrentWeight),
            new Height(document.CurrentHeight),
            document.MotherId,
            GenderExtensions.FromString(document.Gender),
            document.NurseId,
            document.FacilityId,
            PatientStatusExtensions.FromString(document.Status)
        );
    }

    public static PatientDocument ToPersistence(Patient patient)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));

        var data = patient.ToPrimitives();

        return new PatientDocument
        {
            PatientId = data.Id,
            Name = data.Name,
            LastName = data.LastName,
            BirthDate = data.BirthDate,
            CurrentWeight = data.CurrentWeight,
            CurrentHeight = data.CurrentHeight,
            MotherId = data.MotherId,
            NurseId = data.NurseId,
            Gender = data.Gender,
            FacilityId = data.FacilityId,
            Status = data.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static List<Patient> ToDomainList(IEnumerable<PatientDocument> documents)
    {
        return documents.Select(ToDomain).ToList();
    }
}