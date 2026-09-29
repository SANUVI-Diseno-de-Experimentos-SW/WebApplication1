using WebApplication1.Consultation.Domain.Models.Enum;

namespace WebApplication1.Consultation.Domain.Models.Entities;


public class Message
{
    public string Id { get; private set; }
    public string SenderId { get; private set; }
    public MessageSender SenderRole { get; private set; }
    public string Content { get; private set; }
    public DateTime SentAt { get; private set; }

    public Message(
        string id,
        string senderId,
        MessageSender senderRole,
        string content,
        DateTime sentAt)
    {
        Id = id;
        SenderId = senderId;
        SenderRole = senderRole;
        Content = content;
        SentAt = sentAt;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Message id is required", nameof(Id));

        if (string.IsNullOrWhiteSpace(SenderId))
            throw new ArgumentException("Sender id is required", nameof(SenderId));

        if (string.IsNullOrWhiteSpace(Content))
            throw new ArgumentException("Message content cannot be empty", nameof(Content));

        if (SentAt == default)
            throw new ArgumentException("Message sent date is required", nameof(SentAt));
    }

    public MessagePrimitives ToPrimitives()
    {
        return new MessagePrimitives
        {
            Id = Id,
            SenderId = SenderId,
            SenderRole = SenderRole.ToStringValue(),
            Content = Content,
            SentAt = SentAt
        };
    }

    public class MessagePrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}