namespace SDV.Application.Dtos.Messages;

public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<MessageDayDto> MessageDays { get; set; } = new();
}
