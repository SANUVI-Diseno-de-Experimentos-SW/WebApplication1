using WebApplication1.patient_management.Application.Internal;
using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.ValueObjects;

namespace WebApplication1.patient_management.Domain.Aggregate;

/// <summary>
/// Paciente (Agregado raíz)
/// </summary>
public class Patient : IPatientWithName
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string LastName { get; private set; }
    public BirthDate BirthDate { get; private set; }
    public Weight CurrentWeight { get; private set; }
    public Height CurrentHeight { get; private set; }
    public string MotherId { get; private set; }
    public string? NurseId { get; private set; }
    public Gender Gender { get; private set; }
    public string? FacilityId { get; private set; }
    public PatientStatus Status { get; private set; }
    public MedicalRecord? MedicalRecord { get; private set; }

    public Patient(
        string id,
        string name,
        string lastName,
        BirthDate birthDate,
        Weight currentWeight,
        Height currentHeight,
        string motherId,
        Gender gender,
        string? nurseId = null,
        string? facilityId = null,
        PatientStatus status = PatientStatus.Active,
        MedicalRecord? medicalRecord = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Patient ID is required", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (string.IsNullOrWhiteSpace(motherId))
            throw new ArgumentException("Mother ID is required", nameof(motherId));

        Id = id;
        Name = name.Trim();
        LastName = lastName.Trim();
        BirthDate = birthDate ?? throw new ArgumentNullException(nameof(birthDate));
        CurrentWeight = currentWeight ?? throw new ArgumentNullException(nameof(currentWeight));
        CurrentHeight = currentHeight ?? throw new ArgumentNullException(nameof(currentHeight));
        MotherId = motherId;
        NurseId = nurseId;
        Gender = gender;
        FacilityId = facilityId;
        Status = status;
        MedicalRecord = medicalRecord;
    }

    // Constructor privado para serialización
    private Patient() { }

    public void AssignNurse(string nurseId, string facilityId)
    {
        Console.WriteLine($"🔍 AssignNurse - nurseId: {nurseId}, facilityId: {facilityId}");
        Console.WriteLine($"🔍 AssignNurse - Status actual: {Status}, NurseId actual: {NurseId ?? "NULL"}");

        // Si el paciente ya tiene enfermera y NO está dado de alta, lanzar error
        if (NurseId != null && Status != PatientStatus.Discharged)
        {
            throw new InvalidOperationException("Patient already has an assigned nurse");
        }

        NurseId = nurseId;
        FacilityId = facilityId;
        Status = PatientStatus.Active;

        Console.WriteLine($"🔍 AssignNurse - Nuevo Status: {Status}, Nuevo NurseId: {NurseId}");
    }

    public void Discharge(string nurseId)
    {
        if (nurseId != NurseId)
        {
            throw new UnauthorizedAccessException("Only assigned nurse can discharge patient");
        }

        Status = PatientStatus.Discharged;
        NurseId = null;
        FacilityId = null;
    }

    public void UpdateWeight(Weight newWeight)
    {
        CurrentWeight = newWeight;
    }

    public void UpdateHeight(Height newHeight)
    {
        CurrentHeight = newHeight;
    }

    public void AssignMedicalRecord(MedicalRecord medicalRecord)
    {
        if (MedicalRecord != null)
            throw new InvalidOperationException("Patient already has a medical record");

        MedicalRecord = medicalRecord;
    }

    public bool HasMedicalRecord() => MedicalRecord != null;

    public bool IsActive() => Status == PatientStatus.Active;

    public bool IsDischarged() => Status == PatientStatus.Discharged;

    public bool IsAssignedToNurse() => NurseId != null;

    public PatientPrimitives ToPrimitives()
    {
        return new PatientPrimitives
        {
            Id = Id,
            Name = Name,
            LastName = LastName,
            BirthDate = BirthDate.Value,
            CurrentWeight = CurrentWeight.Value,
            CurrentHeight = CurrentHeight.Value,
            MotherId = MotherId,
            NurseId = NurseId,
            Gender = Gender.ToStringValue(),
            FacilityId = FacilityId,
            Status = Status.ToStringValue(),
            MedicalRecord = MedicalRecord?.ToPrimitives()
        };
    }

    public class PatientPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public double CurrentWeight { get; set; }
        public double CurrentHeight { get; set; }
        public string MotherId { get; set; } = string.Empty;
        public string? NurseId { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? FacilityId { get; set; }
        public string Status { get; set; } = string.Empty;
        public MedicalRecord.MedicalRecordPrimitives? MedicalRecord { get; set; }
    }
}