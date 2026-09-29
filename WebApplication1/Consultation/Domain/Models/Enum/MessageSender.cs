namespace WebApplication1.Consultation.Domain.Models.Enum;


public enum MessageSender
{
    MOTHER,
    NURSE
}

public static class MessageSenderExtensions
{
    public static string ToStringValue(this MessageSender sender)
    {
        return sender switch
        {
            MessageSender.MOTHER => "MOTHER",
            MessageSender.NURSE => "NURSE",
            _ => throw new ArgumentOutOfRangeException(nameof(sender), sender, null)
        };
    }

    public static MessageSender FromString(string value)
    {
        return value switch
        {
            "MOTHER" => MessageSender.MOTHER,
            "NURSE" => MessageSender.NURSE,
            _ => throw new ArgumentException($"Invalid message sender: {value}")
        };
    }
}