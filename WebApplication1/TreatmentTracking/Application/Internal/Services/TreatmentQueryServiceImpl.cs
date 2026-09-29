using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.Queries;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.TreatmentTracking.Domain.Services;

namespace WebApplication1.TreatmentTracking.Application.Internal.Services;

public class TreatmentQueryServiceImpl : ITreatmentQueryService
{
    private readonly ITreatmentRepository _treatmentRepository;
    private readonly IDailyDoseRepository _dailyDoseRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<TreatmentQueryServiceImpl> _logger;

    public TreatmentQueryServiceImpl(
        ITreatmentRepository treatmentRepository,
        IDailyDoseRepository dailyDoseRepository,
        IPatientRepository patientRepository,
        ILogger<TreatmentQueryServiceImpl> logger)
    {
        _treatmentRepository = treatmentRepository;
        _dailyDoseRepository = dailyDoseRepository;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    public async Task<object> GetPatientDoseHistoryAsync(GetPatientDoseHistoryQuery query)
    {
        var patient = await _patientRepository.FindByIdAsync(query.PatientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var treatment = await _treatmentRepository.FindActiveByPatientIdAsync(query.PatientId);
        if (treatment == null)
        {
            throw new Exception("Patient does not have an active treatment");
        }

        var doses = await _dailyDoseRepository.FindByTreatmentIdAsync(treatment.Id);

        var confirmedAndOmittedDoses = doses
            .Where(d => d.Status == DoseStatus.CONFIRMED || d.Status == DoseStatus.OMITTED)
            .OrderByDescending(d => d.ScheduledDate)
            .ToList();

        var patientData = patient.ToPrimitives();
        var treatmentData = treatment.ToPrimitives();

        return new
        {
            patientId = patientData.Id,
            patientName = $"{patientData.Name} {patientData.LastName}",
            supplementName = treatmentData.Supplement,
            quantity = treatmentData.Quantity,
            dosingHours = treatmentData.DosingHours,
            doses = confirmedAndOmittedDoses.Select(d => new
            {
                id = d.Id,
                scheduledDate = d.ScheduledDate,
                confirmedAt = d.ConfirmedAt,
                status = d.Status.ToStringValue(),
                hoursWithoutConfirmation = d.CalculateHoursWithoutConfirmation()
            })
        };
    }

    public async Task<object> GetPatientTreatmentDetailAsync(GetPatientTreatmentDetailQuery query)
    {
        var patient = await _patientRepository.FindByIdAsync(query.PatientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var treatment = await _treatmentRepository.FindActiveByPatientIdAsync(query.PatientId);
        if (treatment == null)
        {
            throw new Exception("Patient does not have an active treatment");
        }

        var patientData = patient.ToPrimitives();
        var treatmentData = treatment.ToPrimitives();

        return new
        {
            patientId = patientData.Id,
            patientName = $"{patientData.Name} {patientData.LastName}",
            riskLevel = treatment.RiskScore.RiskLevel.ToStringValue(),
            score = treatment.RiskScore.Score,
            adherenceScore = treatmentData.AdherenceScore,
            totalConfirmed = treatmentData.TotalConfirmed,
            totalOmitted = treatmentData.TotalOmitted,
            treatment = new
            {
                supplementName = treatmentData.Supplement,
                quantity = treatmentData.Quantity,
                dosingHours = treatmentData.DosingHours,
                durationDays = treatmentData.DurationDays,
                startDate = treatmentData.StartDate,
                endDate = treatmentData.EndDate
            }
        };
    }

    public async Task<object> GetPatientsByRiskLevelAsync(GetPatientsByRiskLevelQuery query)
    {
        var treatments = await _treatmentRepository.FindByRiskLevelAsync(query.RiskLevel, query.NurseId);

        if (treatments.Count == 0)
        {
            return new
            {
                riskLevel = query.RiskLevel.ToStringValue(),
                total = 0,
                patients = new List<object>()
            };
        }

        var patients = new List<object>();

        foreach (var treatment in treatments)
        {
            var treatmentData = treatment.ToPrimitives();
            var patient = await _patientRepository.FindByIdAsync(treatmentData.PatientId);

            if (patient == null) continue;

            var patientData = patient.ToPrimitives();

            var doses = await _dailyDoseRepository.FindByTreatmentIdAsync(treatmentData.Id);
            var pendingDoses = doses.Where(d => d.Status == DoseStatus.PENDING).ToList();

            double? hoursWithoutConfirmation = null;
            if (pendingDoses.Count > 0 && query.RiskLevel != RiskLevel.LOW)
            {
                var oldestPending = pendingDoses.OrderBy(d => d.ScheduledDate).First();
                hoursWithoutConfirmation = oldestPending.CalculateHoursWithoutConfirmation();
            }

            // ✅ Corregido: DateTime no tiene HasValue, es un tipo no nullable
            int? patientAge = null;
            if (patientData.BirthDate != default(DateTime))
            {
                var today = DateTime.UtcNow;
                var age = today.Year - patientData.BirthDate.Year;
                if (patientData.BirthDate.Date > today.AddYears(-age)) age--;
                patientAge = age;
            }

            patients.Add(new
            {
                patientId = patientData.Id,
                patientName = $"{patientData.Name} {patientData.LastName}",
                patientAge,
                score = treatment.RiskScore.Score,
                hoursWithoutConfirmation
            });
        }

        return new
        {
            riskLevel = query.RiskLevel.ToStringValue(),
            total = patients.Count,
            patients
        };
    }

    public async Task<object> GetPendingPatientsByNurseAsync(GetPendingPatientsByNurseQuery query)
    {
        var patients = await _patientRepository.FindByNurseIdAsync(query.NurseId);

        if (patients.Count == 0)
        {
            return new
            {
                nurseId = query.NurseId,
                hasPatientsAssigned = false,
                hasPendingPatients = false,
                pendingPatients = new List<object>(),
                message = "No tienes pacientes asignados. Para comenzar, debes asignar pacientes a tu lista de trabajo."
            };
        }

        var pendingPatients = new List<object>();

        foreach (var patient in patients)
        {
            var patientData = patient.ToPrimitives();
            var activeTreatment = await _treatmentRepository.FindActiveByPatientIdAsync(patientData.Id);

            if (activeTreatment == null)
            {
                pendingPatients.Add(new
                {
                    patientId = patientData.Id,
                    patientName = $"{patientData.Name} {patientData.LastName}"
                });
            }
        }

        if (pendingPatients.Count == 0)
        {
            return new
            {
                nurseId = query.NurseId,
                hasPatientsAssigned = true,
                hasPendingPatients = false,
                pendingPatients = new List<object>(),
                message = "Ya todos los pacientes han iniciado su tratamiento."
            };
        }

        return new
        {
            nurseId = query.NurseId,
            hasPatientsAssigned = true,
            hasPendingPatients = true,
            pendingPatients
        };
    }

    public async Task<object> GetRiskLevelOverviewAsync(GetRiskLevelOverviewQuery query)
    {
        List<Treatment> treatments;
        if (!string.IsNullOrEmpty(query.NurseId))
        {
            treatments = await _treatmentRepository.FindByNurseIdAsync(query.NurseId, TreatmentStatus.ACTIVE);
        }
        else
        {
            treatments = await _treatmentRepository.FindAllActiveAsync();
        }

        int highCount = 0;
        int mediumCount = 0;
        int lowCount = 0;

        foreach (var treatment in treatments)
        {
            var riskLevel = treatment.RiskScore.RiskLevel;
        
            switch (riskLevel)
            {
                case RiskLevel.HIGH:
                    highCount++;
                    break;
                case RiskLevel.MEDIUM:
                    mediumCount++;
                    break;
                case RiskLevel.LOW:
                    lowCount++;
                    break;
            }
        }

        // Usar diccionario para forzar las claves en mayúsculas
        var result = new Dictionary<string, object>
        {
            ["summary"] = new Dictionary<string, object>
            {
                ["HIGH"] = new Dictionary<string, object>
                {
                    ["count"] = highCount,
                    ["description"] = "score mayor de 70"
                },
                ["MEDIUM"] = new Dictionary<string, object>
                {
                    ["count"] = mediumCount,
                    ["description"] = "score entre 30 y 70"
                },
                ["LOW"] = new Dictionary<string, object>
                {
                    ["count"] = lowCount,
                    ["description"] = "score menor de 30"
                },
                ["total"] = treatments.Count
            }
        };

        return result;
    }    

    public async Task<object> GetTodayDoseAsync(GetTodayDoseQuery query)
    {
        var patient = await _patientRepository.FindByIdAsync(query.PatientId);
        if (patient == null)
        {
            return new
            {
                canConfirm = false,
                message = "Register your patient first"
            };
        }

        var patientData = patient.ToPrimitives();
        if (patientData.MotherId != query.MotherId)
        {
            throw new Exception("Mother is not assigned to this patient");
        }

        var treatment = await _treatmentRepository.FindActiveByPatientIdAsync(query.PatientId);
        if (treatment == null)
        {
            return new
            {
                canConfirm = false,
                message = "Treatment has not started yet"
            };
        }

        var todayDose = await _dailyDoseRepository.FindTodayDoseAsync(treatment.Id);
        if (todayDose == null)
        {
            return new
            {
                canConfirm = false,
                message = "No scheduled dose for today"
            };
        }

        var treatmentData = treatment.ToPrimitives();

        return new
        {
            patientId = query.PatientId,
            treatmentId = treatment.Id,
            dailyDoseId = todayDose.Id,
            scheduledDate = todayDose.ScheduledDate,
            status = todayDose.Status.ToStringValue(),
            canConfirm = todayDose.Status == DoseStatus.PENDING,
            dosingHours = treatmentData.DosingHours
        };
    }

    public async Task<object> GetTreatmentDetailsAsync(GetTreatmentDetailsQuery query)
    {
        var treatment = await _treatmentRepository.FindByIdAsync(query.TreatmentId);
        if (treatment == null)
        {
            throw new Exception("Treatment not found");
        }

        var treatmentData = treatment.ToPrimitives();

        var patient = await _patientRepository.FindByIdAsync(treatmentData.PatientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var doses = await _dailyDoseRepository.FindByTreatmentIdAsync(treatmentData.Id);
        var sortedDoses = doses.OrderByDescending(d => d.ScheduledDate).ToList();

        var patientData = patient.ToPrimitives();

        return new
        {
            treatmentId = treatmentData.Id,
            patientId = treatmentData.PatientId,
            patientName = $"{patientData.Name} {patientData.LastName}",
            status = treatmentData.Status,
            supplementName = treatmentData.Supplement,
            quantity = treatmentData.Quantity,
            dosingHours = treatmentData.DosingHours,
            durationDays = treatmentData.DurationDays,
            startDate = treatmentData.StartDate,
            endDate = treatmentData.EndDate,
            adherenceScore = treatmentData.AdherenceScore,
            totalConfirmed = treatmentData.TotalConfirmed,
            totalOmitted = treatmentData.TotalOmitted,
            completionObservation = treatmentData.CompletionObservation,
            abandonmentObservation = treatmentData.AbandonmentObservation
        };
    }

    public async Task<object> GetTreatmentsByNurseAsync(GetTreatmentsByNurseQuery query)
    {
        try
        {
            var treatments = await _treatmentRepository.FindByNurseIdAsync(query.NurseId, query.Status);

            if (treatments.Count == 0)
            {
                return new
                {
                    nurseId = query.NurseId,
                    treatments = new List<object>(),
                    message = "No treatments found"
                };
            }

            var mappedTreatments = new List<object>();

            foreach (var treatment in treatments)
            {
                try
                {
                    var treatmentData = treatment.ToPrimitives();
                    var patient = await _patientRepository.FindByIdAsync(treatmentData.PatientId);
                    var patientData = patient?.ToPrimitives();

                    mappedTreatments.Add(new
                    {
                        treatmentId = treatmentData.Id,
                        patientId = treatmentData.PatientId,
                        patientName = patientData != null ? $"{patientData.Name} {patientData.LastName}" : "Unknown patient",
                        status = treatmentData.Status,
                        supplementName = treatmentData.Supplement
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al mapear tratamiento {TreatmentId}", treatment.Id);
                    // Continuar con el siguiente
                }
            }

            return new
            {
                nurseId = query.NurseId,
                treatments = mappedTreatments
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetTreatmentsByNurseAsync");
            throw;
        }
    }
}