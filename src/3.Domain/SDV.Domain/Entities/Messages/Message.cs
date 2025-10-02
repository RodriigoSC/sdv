using MongoDB.Bson.Serialization.Attributes;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Messages.ValueObjects;

namespace SDV.Domain.Entities.Messages;

public class Message : BaseEntity
{
    public Guid ClientId { get; private set; }
    public string Title { get; private set; }

    [BsonElement("MessageDay")]
    private List<MessageDay> _messages;
    public IReadOnlyCollection<MessageDay> Messages => _messages.AsReadOnly();

    public Message(Guid clientId, string title)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId is required.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Id = Guid.NewGuid();
        ClientId = clientId;
        Title = title;
        _messages = new List<MessageDay>();

        MarkAsUpdated();
    }

    #region Título

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty.", nameof(newTitle));

        if (Title != newTitle)
        {
            Title = newTitle;
            MarkAsUpdated();
        }
    }

    #endregion

    #region Mensagens

    public void AddMessage(MessageDay message)
    {
        if (_messages.Any(h => h.Date.Date == message.Date.Date))
            throw new ArgumentException("Message already exists on this date.");

        _messages.Add(message);
        MarkAsUpdated();
    }

    public void RemoveMessage(DateTime date)
    {
        var item = _messages.FirstOrDefault(h => h.Date.Date == date.Date);
        if (item != null)
        {
            _messages.Remove(item);
            MarkAsUpdated();
        }
    }

    public void ReplaceMessages(IEnumerable<MessageDay> newMessages)
    {
        if (newMessages == null)
            throw new ArgumentNullException(nameof(newMessages));

        var distinct = newMessages
            .GroupBy(h => h.Date.Date)
            .Select(g => g.First())
            .ToList();

        _messages.Clear();
        _messages.AddRange(distinct);
        MarkAsUpdated();
    }

    public void ClearMessages()
    {
        if (_messages.Any())
        {
            _messages.Clear();
            MarkAsUpdated();
        }
    }

    #endregion
}
