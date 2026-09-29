using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.ValueObjects;

namespace WebApplication1.patient_management.Domain.Entities;



/// <summary>
/// Historia Clínica del paciente
/// </summary>
public class MedicalRecord
{
    public string Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public HemoglobinLevel? HemoglobinLevel { get; private set; }
    public Weight Weight { get; private set; }
    public Height Height { get; private set; }
    public Gender Gender { get; private set; }
    public List<Antecedente> Antecedentes { get; private set; }
    public MotivoConsulta MotivoConsulta { get; private set; }
    public Observaciones Observaciones { get; private set; }
    public List<string> Sintomas { get; private set; }
    public List<Control> Controls { get; private set; }
    public string PatientId { get; private set; }
    public string? NurseId { get; private set; }

    public MedicalRecord(
        string id,
        DateTime createdAt,
        Weight weight,
        Height height,
        Gender gender,
        MotivoConsulta motivoConsulta,
        Observaciones observaciones,
        string patientId,
        string? nurseId = null,
        HemoglobinLevel? hemoglobinLevel = null,
        List<Antecedente>? antecedentes = null,
        List<string>? sintomas = null,
        List<Control>? controls = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Medical record ID is required", nameof(id));

        if (string.IsNullOrWhiteSpace(patientId))
            throw new ArgumentException("Patient ID is required", nameof(patientId));

        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Weight = weight ?? throw new ArgumentNullException(nameof(weight));
        Height = height ?? throw new ArgumentNullException(nameof(height));
        Gender = gender;
        MotivoConsulta = motivoConsulta ?? throw new ArgumentNullException(nameof(motivoConsulta));
        Observaciones = observaciones ?? throw new ArgumentNullException(nameof(observaciones));
        PatientId = patientId;
        NurseId = nurseId;
        HemoglobinLevel = hemoglobinLevel;
        Antecedentes = antecedentes ?? new List<Antecedente>();
        Sintomas = sintomas ?? new List<string>();
        Controls = controls ?? new List<Control>();

        ValidateDuplicateAntecedentes(Antecedentes);
    }

    // Constructor privado para serialización
    private MedicalRecord() { }

    public void AddControl(Control control)
    {
        if (control == null)
            throw new ArgumentNullException(nameof(control));

        Controls.Add(control);
        // ✅ Actualizar el nivel de hemoglobina con el último control
        HemoglobinLevel = control.HemoglobinLevel;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateClinicalInformation(
        Weight? weight = null,
        Height? height = null,
        MotivoConsulta? motivoConsulta = null,
        Observaciones? observaciones = null,
        List<Antecedente>? antecedentes = null,
        List<string>? sintomas = null)
    {
        if (weight != null) Weight = weight;
        if (height != null) Height = height;
        if (motivoConsulta != null) MotivoConsulta = motivoConsulta;
        if (observaciones != null) Observaciones = observaciones;

        if (antecedentes != null)
        {
            ValidateDuplicateAntecedentes(antecedentes);
            Antecedentes = antecedentes;
        }

        if (sintomas != null)
        {
            Sintomas = sintomas;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateDuplicateAntecedentes(List<Antecedente> antecedentes)
    {
        var types = antecedentes.Select(a => a.Type).ToList();
        var uniqueTypes = new HashSet<string>(types);

        if (types.Count != uniqueTypes.Count)
        {
            throw new InvalidOperationException("Duplicate antecedents are not allowed");
        }
    }

    public MedicalRecordPrimitives ToPrimitives()
    {
        return new MedicalRecordPrimitives
        {
            Id = Id,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            HemoglobinLevel = HemoglobinLevel?.Value,
            Weight = Weight.Value,
            Height = Height.Value,
            Gender = Gender.ToStringValue(),
            Antecedentes = Antecedentes.Select(a => a.ToPrimitives()).ToList(),
            MotivoConsulta = MotivoConsulta.Value,
            Observaciones = Observaciones.Value ?? string.Empty,
            Sintomas = Sintomas,
            Controls = Controls.Select(c => c.ToPrimitives()).ToList(),
            PatientId = PatientId,
            NurseId = NurseId
        };
    }

    public class MedicalRecordPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public double? HemoglobinLevel { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public string Gender { get; set; } = string.Empty;
        public List<Antecedente.AntecedentePrimitives> Antecedentes { get; set; } = new();
        public string MotivoConsulta { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public List<string> Sintomas { get; set; } = new();
        public List<Control.ControlPrimitives> Controls { get; set; } = new();
        public string PatientId { get; set; } = string.Empty;
        public string? NurseId { get; set; }
    }
}