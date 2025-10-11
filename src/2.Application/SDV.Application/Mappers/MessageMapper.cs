using SDV.Application.Dtos.Messages;
using SDV.Domain.Entities.Messages;
using SDV.Domain.Entities.Messages.ValueObjects;

namespace SDV.Application.Mappers;

public static class MessageMapper
{
    public static MessageDto ToMessageDto(this Message message)
    {
        if (message == null) return null!;

        return new MessageDto
        {
            Id = message.Id.ToString(),
            ClientId = message.ClientId.ToString(),
            Title = message.Title,
            MessageDays = message.Messages?
                .Select(h => new MessageDayDto
                {
                    Content = h.Content,
                    Date = h.Date
                })
                .ToList() ?? new List<MessageDayDto>()
        };
    }

    public static Message ToMessage(this MessageDto dto)
    {
        if (dto == null) return null!;

        var message = new Message(Guid.Parse(dto.ClientId), dto.Title);

        if (dto.MessageDays != null && dto.MessageDays.Any())
        {
            var messages = dto.MessageDays
                .Select(h => new MessageDay(h.Content, h.Date))
                .ToList();

            message.ReplaceMessages(messages);
        }

        return message;
    }

    public static void UpdateFromDto(this Message message, MessageDto dto)
    {
        if (message == null || dto == null) return;

        if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != message.Title)
            message.UpdateTitle(dto.Title);

        var messages = dto.MessageDays != null
            ? dto.MessageDays.Select(h => new MessageDay(h.Content, h.Date)).ToList()
            : new List<MessageDay>();

        message.ReplaceMessages(messages);
    }


    public static IEnumerable<MessageDto> ToMessageDtoList(this IEnumerable<Message> messages)
        => messages.Select(c => c.ToMessageDto());
}

