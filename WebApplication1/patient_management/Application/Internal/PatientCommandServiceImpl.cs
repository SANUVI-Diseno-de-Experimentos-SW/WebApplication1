using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.Consultation.Domain.Repositories;
using WebApplication1.Contexts.PatientManagement.Domain.Commands;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.patient_management.Domain;
using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.patient_management.Domain.Commands;
using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Domain.Services;
using WebApplication1.patient_management.Domain.ValueObjects;

namespace WebApplication1.patient_management.Application.Internal;




public class PatientCommandServiceImpl : IPatientCommandService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly INurseAssignmentRepository _nurseAssignmentRepository;
    private readonly ITreatmentRepository _treatmentRepository;
    private readonly IDailyDoseRepository _dailyDoseRepository;
    private readonly IAchievementRepository _achievementRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly IConsultationRepository _consultationRepository;

    public PatientCommandServiceImpl(
        IPatientRepository patientRepository,
        IMedicalRecordRepository medicalRecordRepository,
        INurseAssignmentRepository nurseAssignmentRepository,
        ITreatmentRepository treatmentRepository,
        IDailyDoseRepository dailyDoseRepository,
        IAchievementRepository achievementRepository,
        IBadgeRepository badgeRepository,
        IConsultationRepository consultationRepository)
    {
        _patientRepository = patientRepository;
        _medicalRecordRepository = medicalRecordRepository;
        _nurseAssignmentRepository = nurseAssignmentRepository;
        _treatmentRepository = treatmentRepository;
        _dailyDoseRepository = dailyDoseRepository;
        _achievementRepository = achievementRepository;
        _badgeRepository = badgeRepository;
        _consultationRepository = consultationRepository;
    }

    public async Task AssignPatientToNurseAsync(AssignPatientToNurseCommand command)
    {
        // ✅ LOG para depuración
        Console.WriteLine($"🔍 AssignPatientToNurseAsync - PatientId: {command.PatientId}");
        Console.WriteLine($"🔍 AssignPatientToNurseAsync - NurseId: {command.NurseId}");

        var patient = await _patientRepository.FindByIdAsync(command.PatientId);

        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        // ✅ CORREGIDO: Usar FindActiveByNurseIdAsync en lugar de FindByNurseIdAsync
        var assignment = await _nurseAssignmentRepository.FindActiveByNurseIdAsync(command.NurseId);

        if (assignment == null)
        {
            throw new Exception("Nurse is not assigned to any facility");
        }

        var assignmentData = assignment.ToPrimitives();
        var patientData = patient.ToPrimitives();
        var currentNurseId = patientData.NurseId;

        Console.WriteLine($"🔍 assignment - FacilityId: {assignmentData.FacilityId}");
        Console.WriteLine($"🔍 patient - currentNurseId: {currentNurseId}");

        if (currentNurseId == command.NurseId)
        {
            Console.WriteLine($"[AssignPatientToNurse] Patient {command.PatientId} already assigned to nurse {command.NurseId}");
            return;
        }

        patient.AssignNurse(command.NurseId, assignmentData.FacilityId);

        await _patientRepository.UpdateAsync(patient);
    }

    public async Task CreateInitialMedicalRecordAsync(CreateInitialMedicalRecordCommand command)
    {
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);

        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var existingRecord = await _medicalRecordRepository.FindByPatientIdAsync(command.PatientId);

        if (existingRecord != null)
        {
            throw new Exception("Medical record already exists");
        }

        var patientData = patient.ToPrimitives();

        var medicalRecord = new MedicalRecord(
            Guid.NewGuid().ToString(),
            DateTime.UtcNow,
            new Weight(command.Weight),
            new Height(command.Height),
            GenderExtensions.FromString(patientData.Gender),
            new MotivoConsulta(command.MotivoConsulta),
            new Observaciones(command.Observaciones),
            command.PatientId,
            patientData.NurseId,
            null,
            command.Antecedentes?.Select(a => new Antecedente(a.Type, a.Description)).ToList() ?? new List<Antecedente>(),
            command.Sintomas ?? new List<string>(),
            new List<Control>()
        );

        await _medicalRecordRepository.SaveAsync(medicalRecord);
    }

    public async Task DischargePatientAsync(DischargePatientCommand command)
    {
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);

        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        patient.Discharge(command.NurseId);

        await DeleteAllTreatmentsForPatientAsync(command.PatientId);
        await DeleteConsultationsForPatientAsync(command.PatientId);
        await DeleteMedicalRecordForPatientAsync(command.PatientId);

        await _patientRepository.UpdateAsync(patient);
    }

    // PatientCommandServiceImpl.cs - Método RegisterHemoglobinControlAsync

    public async Task RegisterHemoglobinControlAsync(RegisterHemoglobinControlCommand command)
    {
        try
        {
            Console.WriteLine($"🔍 RegisterHemoglobinControlAsync - Iniciando...");
            Console.WriteLine($"🔍 PatientId: {command?.PatientId}");
            Console.WriteLine($"🔍 HemoglobinLevel: {command?.HemoglobinLevel}");

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.IsNullOrEmpty(command.PatientId))
                throw new ArgumentException("PatientId es requerido");

            var medicalRecord = await _medicalRecordRepository.FindByPatientIdAsync(command.PatientId);

            if (medicalRecord == null)
            {
                Console.WriteLine($"❌ Medical record not found for patient {command.PatientId}");
                throw new Exception("Medical record not found");
            }

            Console.WriteLine($"✅ Medical record encontrado: {medicalRecord.Id}");

            var control = new Control(
                Guid.NewGuid().ToString(),
                DateTime.UtcNow,
                new HemoglobinLevel(command.HemoglobinLevel)
            );

            Console.WriteLine($"✅ Control creado: {control.Id}");

            medicalRecord.AddControl(control);
            Console.WriteLine($"✅ Control agregado al medical record");

            await _medicalRecordRepository.UpdateAsync(medicalRecord);
            Console.WriteLine($"✅ Medical record actualizado");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en RegisterHemoglobinControlAsync: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            throw;
        }
    }
    
    public async Task RegisterPatientAsync(RegisterPatientCommand command)
    {
        var patient = new Patient(
            Guid.NewGuid().ToString(),
            command.Name,
            command.LastName,
            new BirthDate(command.BirthDate),
            new Weight(command.Weight),
            new Height(command.Height),
            command.MotherId,
            GenderExtensions.FromString(command.Gender),
            null,
            null,
            PatientStatus.Active
        );

        await _patientRepository.SaveAsync(patient);
    }

    public async Task UpdateMedicalRecordAsync(UpdateMedicalRecordCommand command)
    {
        var medicalRecord = await _medicalRecordRepository.FindByPatientIdAsync(command.PatientId);

        if (medicalRecord == null)
        {
            throw new Exception("Medical record not found");
        }

        var weight = command.Weight.HasValue ? new Weight(command.Weight.Value) : null;
        var height = command.Height.HasValue ? new Height(command.Height.Value) : null;
        var motivoConsulta = !string.IsNullOrWhiteSpace(command.MotivoConsulta) ? new MotivoConsulta(command.MotivoConsulta) : null;
        var observaciones = command.Observaciones != null ? new Observaciones(command.Observaciones) : null;
        var antecedentes = command.Antecedentes != null
            ? command.Antecedentes.Select(a => new Antecedente(a.Type, a.Description)).ToList()
            : null;
        var sintomas = command.Sintomas;

        medicalRecord.UpdateClinicalInformation(
            weight,
            height,
            motivoConsulta,
            observaciones,
            antecedentes,
            sintomas
        );

        await _medicalRecordRepository.UpdateAsync(medicalRecord);
    }

    // ==========================================
    // MÉTODOS PRIVADOS
    // ==========================================

    private async Task DeleteAllTreatmentsForPatientAsync(string patientId)
    {
        try
        {
            var treatments = await _treatmentRepository.FindByPatientIdAsync(patientId);

            if (treatments.Count == 0)
            {
                Console.WriteLine($"[DeleteAllTreatmentsForPatient] No treatments found for patient {patientId}");
                return;
            }

            Console.WriteLine($"[DeleteAllTreatmentsForPatient] Found {treatments.Count} treatment(s) for patient {patientId}");

            foreach (var treatment in treatments)
            {
                var treatmentId = treatment.Id;

                var doses = await _dailyDoseRepository.FindByTreatmentIdAsync(treatmentId);
                var doseIds = doses.Select(d => d.Id).ToList();

                if (doseIds.Count > 0)
                {
                    await _dailyDoseRepository.DeleteManyAsync(doseIds);
                    Console.WriteLine($"[DeleteAllTreatmentsForPatient] DELETED {doseIds.Count} doses for treatment {treatmentId}");
                }

                await DeleteAchievementAndBadgesForTreatmentAsync(treatmentId);

                await _treatmentRepository.DeleteAsync(treatmentId);
                Console.WriteLine($"[DeleteAllTreatmentsForPatient] DELETED treatment {treatmentId}");
            }

            Console.WriteLine($"[DeleteAllTreatmentsForPatient] All treatments deleted for patient {patientId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteAllTreatmentsForPatient] Error deleting treatments for patient {patientId}: {ex.Message}");
        }
    }

    private async Task DeleteAchievementAndBadgesForTreatmentAsync(string treatmentId)
    {
        try
        {
            var achievement = await _achievementRepository.FindByTreatmentIdAsync(treatmentId);

            if (achievement != null)
            {
                var achievementId = achievement.Id;
                Console.WriteLine($"[DeleteAchievementAndBadgesForTreatment] Found achievement {achievementId} for treatment {treatmentId}");

                await _badgeRepository.DeleteByAchievementIdAsync(achievementId);
                Console.WriteLine($"[DeleteAchievementAndBadgesForTreatment] Deleted badges for achievement {achievementId}");

                await _achievementRepository.DeleteAsync(achievementId);
                Console.WriteLine($"[DeleteAchievementAndBadgesForTreatment] Deleted achievement {achievementId}");
            }
            else
            {
                Console.WriteLine($"[DeleteAchievementAndBadgesForTreatment] No achievement found for treatment {treatmentId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteAchievementAndBadgesForTreatment] Error: {ex.Message}");
        }
    }

    private async Task DeleteConsultationsForPatientAsync(string patientId)
    {
        try
        {
            var consultation = await _consultationRepository.FindOpenByPatientIdAsync(patientId);

            if (consultation != null)
            {
                var consultationId = consultation.Id;
                Console.WriteLine($"[DeleteConsultationsForPatient] Found active consultation {consultationId} for patient {patientId}");

                await _consultationRepository.DeleteAsync(consultationId);
                Console.WriteLine($"[DeleteConsultationsForPatient] Deleted consultation {consultationId}");
            }
            else
            {
                Console.WriteLine($"[DeleteConsultationsForPatient] No active consultation found for patient {patientId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteConsultationsForPatient] Error: {ex.Message}");
        }
    }

    private async Task DeleteMedicalRecordForPatientAsync(string patientId)
    {
        try
        {
            var medicalRecord = await _medicalRecordRepository.FindByPatientIdAsync(patientId);

            if (medicalRecord != null)
            {
                var medicalRecordId = medicalRecord.Id;
                Console.WriteLine($"[DeleteMedicalRecordForPatient] Found medical record {medicalRecordId} for patient {patientId}");

                await _medicalRecordRepository.DeleteAsync(medicalRecordId);
                Console.WriteLine($"[DeleteMedicalRecordForPatient] Deleted medical record {medicalRecordId}");
            }
            else
            {
                Console.WriteLine($"[DeleteMedicalRecordForPatient] No medical record found for patient {patientId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteMedicalRecordForPatient] Error: {ex.Message}");
        }
    }
}