namespace SDV.Domain.Entities.Messages.ValueObjects;

public record MessageDay
{
    public string Content { get; init; }
    public DateTime Date { get; init; }

    public MessageDay(string content, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        Content = content;
        Date = date.Date;
    }
}
