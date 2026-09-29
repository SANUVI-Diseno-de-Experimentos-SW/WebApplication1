using WebApplication1.Consultation.Domain.Models.Entities;

namespace WebApplication1.Consultation.Domain.Models.Aggregate;



public class Consultation
{
    public string Id { get; private set; }
    public string PatientId { get; private set; }
    public string MotherId { get; private set; }
    public string NurseId { get; private set; }
    public List<Message> Messages { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    public Consultation(
        string id,
        string patientId,
        string motherId,
        string nurseId,
        List<Message> messages,
        DateTime createdAt,
        DateTime? closedAt = null)
    {
        Id = id;
        PatientId = patientId;
        MotherId = motherId;
        NurseId = nurseId;
        Messages = messages ?? new List<Message>();
        CreatedAt = createdAt;
        ClosedAt = closedAt;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Consultation id is required", nameof(Id));

        if (string.IsNullOrWhiteSpace(PatientId))
            throw new ArgumentException("Patient id is required", nameof(PatientId));

        if (string.IsNullOrWhiteSpace(MotherId))
            throw new ArgumentException("Mother id is required", nameof(MotherId));

        if (string.IsNullOrWhiteSpace(NurseId))
            throw new ArgumentException("Nurse id is required", nameof(NurseId));

        if (CreatedAt == default)
            throw new ArgumentException("Created date is required", nameof(CreatedAt));
    }

    public void SendMessage(Message message)
    {
        var senderId = message.SenderId;

        if (senderId != MotherId && senderId != NurseId)
        {
            throw new InvalidOperationException("Sender is not part of this consultation");
        }

        Messages.Add(message);
    }

    public void Close()
    {
        if (ClosedAt.HasValue)
        {
            throw new InvalidOperationException("Consultation is already closed");
        }

        ClosedAt = DateTime.UtcNow;
    }

    public bool IsOpen() => !ClosedAt.HasValue;

    public ConsultationPrimitives ToPrimitives()
    {
        return new ConsultationPrimitives
        {
            Id = Id,
            PatientId = PatientId,
            MotherId = MotherId,
            NurseId = NurseId,
            Messages = Messages.Select(m => m.ToPrimitives()).ToList(),
            CreatedAt = CreatedAt,
            ClosedAt = ClosedAt,
            IsOpen = IsOpen()  // ✅ Incluir IsOpen en los primitivos
        };
    }

    public class ConsultationPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string MotherId { get; set; } = string.Empty;
        public string NurseId { get; set; } = string.Empty;
        public List<Message.MessagePrimitives> Messages { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsOpen { get; set; }  // ✅ Nuevo campo
    }
}