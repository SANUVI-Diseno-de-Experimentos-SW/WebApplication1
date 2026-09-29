using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.Model.DTos;
using WebApplication1.patient_management.Domain.Queries;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Domain.Services;
using WebApplication1.patient_management.Infrastructure.Services;

namespace WebApplication1.patient_management.Application.Internal;

public class PatientQueryServiceImpl : IPatientQueryService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IUserRepository _userRepository;

    public PatientQueryServiceImpl(
        IPatientRepository patientRepository,
        IMedicalRecordRepository medicalRecordRepository,
        IUserRepository userRepository)
    {
        _patientRepository = patientRepository;
        _medicalRecordRepository = medicalRecordRepository;
        _userRepository = userRepository;
    }

    // ==========================================
    // FILTROS PRIVADOS
    // ==========================================

    private List<T> FilterPatientsBySearchTerm<T>(List<T> items, string? searchTerm) where T : IPatientWithName
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return items;

        var terms = searchTerm.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return items.Where(item =>
        {
            var fullName = $"{item.Name} {item.LastName}".ToLower();
            var nameLower = item.Name.ToLower();
            var lastNameLower = item.LastName.ToLower();

            if (terms.Length == 1)
            {
                var term = terms[0];
                return nameLower.Contains(term) ||
                       lastNameLower.Contains(term) ||
                       fullName.Contains(term);
            }

            return terms.All(term =>
                nameLower.Contains(term) ||
                lastNameLower.Contains(term) ||
                fullName.Contains(term));
        }).ToList();
    }

    // ==========================================
    // IMPLEMENTACIÓN DE LA INTERFAZ
    // ==========================================

    public async Task<Patient?> GetPatientBasicInfoAsync(GetPatientBasicInfoQuery query)
    {
        return await _patientRepository.FindByIdAsync(query.PatientId);
    }

    public async Task<object> GetMotherPatientsSummaryAsync(GetMotherPatientsSummaryQuery query)
    {
        var patients = await _patientRepository.FindByMotherIdAsync(query.MotherId);

        return patients.Select(patient =>
        {
            var data = patient.ToPrimitives();
            return new
            {
                id = data.Id,
                name = data.Name
            };
        }).ToList();
    }

    public async Task<int> GetActivePatientsCountAsync(GetActivePatientsCountQuery query)
    {
        var patients = await _patientRepository.FindByNurseIdAsync(query.NurseId);
        return patients.Count(p => p.ToPrimitives().Status != "DISCHARGED");
    }

    public async Task<byte[]> DownloadHemoglobinReportPdfAsync(DownloadHemoglobinReportPdfQuery query)
    {
        var medicalRecord = await _medicalRecordRepository.FindByIdAsync(query.MedicalRecordId);

        if (medicalRecord == null)
        {
            throw new Exception("Medical record not found");
        }

        return await PdfService.GenerateHemoglobinReportPdfAsync(medicalRecord);
    }

    public async Task<byte[]> DownloadMedicalRecordPdfAsync(DownloadMedicalRecordPdfQuery query)
    {
        var medicalRecord = await _medicalRecordRepository.FindByIdAsync(query.MedicalRecordId);

        if (medicalRecord == null)
        {
            throw new Exception("Medical record not found");
        }

        var patient = await _patientRepository.FindByIdAsync(medicalRecord.PatientId);

        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        return await PdfService.GenerateMedicalRecordPdfAsync(patient, medicalRecord);
    }

    // PatientQueryServiceImpl.cs
    
public async Task<object> GetHemoglobinControlsHistoryAsync(GetHemoglobinControlsHistoryQuery query)
{
    var medicalRecord = await _medicalRecordRepository.FindByIdAsync(query.MedicalRecordId);

    if (medicalRecord == null)
    {
        throw new Exception("Medical record not found");
    }

    // ✅ Obtener el paciente para el nombre
    var patient = await _patientRepository.FindByIdAsync(medicalRecord.PatientId);
    var patientName = patient != null ? $"{patient.Name} {patient.LastName}" : "Paciente";

    var controls = medicalRecord.Controls.OrderBy(c => c.Date).ToList();
    
    // ✅ Calcular promedio
    double? averageHemoglobin = controls.Count > 0 
        ? Math.Round(controls.Average(c => c.HemoglobinLevel.Value ?? 0), 2) 
        : (double?)null;
    
    // ✅ Calcular evolución (último - primero)
    double? evolution = null;
    if (controls.Count >= 2)
    {
        var first = controls.First().HemoglobinLevel.Value ?? 0;
        var last = controls.Last().HemoglobinLevel.Value ?? 0;
        evolution = Math.Round(last - first, 2);
    }

    // ✅ Calcular tendencia
    string? trend = null;
    if (controls.Count >= 3)
    {
        var values = controls.Select(c => c.HemoglobinLevel.Value ?? 0).ToList();
        var firstHalf = values.Take(values.Count / 2).Average();
        var secondHalf = values.Skip(values.Count / 2).Average();
        trend = secondHalf > firstHalf ? "UP" : secondHalf < firstHalf ? "DOWN" : "STABLE";
    }

    return new
    {
        patientId = medicalRecord.PatientId,
        patientName = patientName,
        controls = controls.Select(c => new
        {
            id = c.Id,
            date = c.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            hemoglobinLevel = c.HemoglobinLevel.Value,
            anemiaStatus = c.AnemiaStatus.ToStringValue()
        }).ToList(),
        averageHemoglobin = averageHemoglobin,
        totalControls = controls.Count,
        evolution = evolution,
        trend = trend
    };
}

public async Task<MedicalRecordWithPatientDto?> GetMedicalRecordAsync(GetMedicalRecordQuery query)
{
    var medicalRecord = await _medicalRecordRepository.FindByPatientIdAsync(query.PatientId);

    if (medicalRecord == null)
    {
        return null;
    }

    // ✅ Obtener el paciente para el nombre
    var patient = await _patientRepository.FindByIdAsync(query.PatientId);
    var patientName = patient != null ? $"{patient.Name} {patient.LastName}" : "Paciente";

    var data = medicalRecord.ToPrimitives();

    return new MedicalRecordWithPatientDto
    {
        Id = data.Id,
        PatientId = data.PatientId,
        PatientName = patientName,  // ✅ Agregar el nombre
        CreatedAt = data.CreatedAt,
        UpdatedAt = data.UpdatedAt,
        HemoglobinLevel = data.HemoglobinLevel,
        Weight = data.Weight,
        Height = data.Height,
        Gender = data.Gender,
        MotivoConsulta = data.MotivoConsulta,
        Observaciones = data.Observaciones,
        Sintomas = data.Sintomas,
        NurseId = data.NurseId,
        Antecedentes = data.Antecedentes.Select(a => new AntecedenteDto
        {
            Type = a.Type,
            Description = a.Description
        }).ToList(),
        Controls = data.Controls.Select(c => new ControlItemDto
        {
            Id = c.Id,
            Date = c.Date,
            HemoglobinLevel = c.HemoglobinLevel,
            AnemiaStatus = c.AnemiaStatus
        }).ToList()
    };
}

    public async Task<IReadOnlyList<Patient>> GetPatientsEligibleForDischargeAsync(GetPatientsEligibleForDischargeQuery query)
    {
        var patients = await _patientRepository.FindPatientsEligibleForDischargeAsync(query.NurseId);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var patientList = patients.ToList();
            var filtered = FilterPatientsBySearchTerm(patientList, query.SearchTerm);
            return filtered.AsReadOnly();
        }

        return patients;
    }

    public async Task<IReadOnlyList<Patient>> ListPatientsByMotherAsync(ListPatientsByMotherQuery query)
    {
        return await _patientRepository.FindByMotherIdAsync(query.MotherId);
    }

    public async Task<object> SearchMotherByDniAsync(SearchMotherByDniQuery query)
    {
        var searchTerm = query.SearchTerm;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new Exception("Search term is required");
        }

        var mothers = await _userRepository.FindMothersBySearchTermAsync(searchTerm);

        if (mothers == null || mothers.Count == 0)
        {
            throw new Exception("No mothers found matching the search criteria");
        }

        return mothers.Select(m => new
        {
            motherId = m.Id.Value,
            fullName = $"{m.Name} {m.Lastname}".Trim(),
            dni = m.Dni.Value
        }).ToList();
    }

    public async Task<IReadOnlyList<Patient>> GetPatientsAssignedToNurseAsync(GetPatientsAssignedToNurseQuery query)
    {
        var patients = await _patientRepository.FindByNurseIdAsync(query.NurseId);

        var activePatients = patients.Where(p => p.ToPrimitives().Status != "DISCHARGED").ToList();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var filtered = FilterPatientsBySearchTerm(activePatients, query.SearchTerm);
            return filtered.AsReadOnly();
        }

        return activePatients.AsReadOnly();
    }

    public async Task<object> GetHemoglobinEvolutionChartAsync(GetHemoglobinEvolutionChartQuery query)
    {
        var medicalRecord = await _medicalRecordRepository.FindByPatientIdAsync(query.PatientId);

        if (medicalRecord == null)
        {
            throw new Exception("Medical record not found");
        }

        var controls = medicalRecord.Controls.OrderBy(c => c.Date).ToList();

        var chartData = controls.Select(c => new
        {
            date = c.Date,
            hemoglobinLevel = c.HemoglobinLevel.Value
        }).ToList();

        double? latestValue = controls.Count > 0 ? controls.Last().HemoglobinLevel.Value : null;

        return new
        {
            currentHemoglobin = latestValue,
            chart = chartData
        };
    }

    public async Task<Patient?> GetPatientAsync(GetPatientQuery query)
    {
        return await _patientRepository.FindByIdAsync(query.PatientId);
    }

    public async Task<MedicalRecord?> GetMedicalRecordByIdAsync(string medicalRecordId)
    {
        return await _medicalRecordRepository.FindByIdAsync(medicalRecordId);
    }

    public async Task<bool> CheckPatientMedicalRecordAsync(CheckPatientMedicalRecordQuery query)
    {
        var medicalRecord = await _medicalRecordRepository.FindByPatientIdAsync(query.PatientId);
        return medicalRecord != null;
    }
}

// ==========================================
// INTERFAZ PARA FILTROS
// ==========================================

public interface IPatientWithName
{
    string Name { get; }
    string LastName { get; }
}