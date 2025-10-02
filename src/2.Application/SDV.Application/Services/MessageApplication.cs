using SDV.Application.Dtos.Messages;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Interfaces.Messages;

namespace SDV.Application.Services;

public class MessageApplication : IMessageApplication
{
    private readonly IMessageService _messageService;

    public MessageApplication(IMessageService messageService)
    {
        _messageService = messageService;
    }

    #region Consultas

    public async Task<OperationResult<IEnumerable<MessageDto>>> GetAllMessages(string clientId)
    {
        if (!Guid.TryParse(clientId, out var clientGuid))
            return OperationResult<IEnumerable<MessageDto>>.Failed(null, "Invalid client ID format", 400);

        var result = await _messageService.GetAllMessagesAsync(clientGuid);
        if (!result.IsSuccess)
            return OperationResult<IEnumerable<MessageDto>>.Failed(null, result.Error ?? "Messages not retrieved", 400);

        var dtos = result.Value?.ToMessageDtoList() ?? Enumerable.Empty<MessageDto>();
        return OperationResult<IEnumerable<MessageDto>>.Succeeded(dtos, "Messages retrieved", 200);
    }

    public async Task<OperationResult<MessageDto>> GetMessageById(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<MessageDto>.Failed(null, "Invalid ID format", 400);

        var result = await _messageService.GetMessageByIdAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<MessageDto>.Failed(null, result.Error ?? "Message not found", 404);

        return OperationResult<MessageDto>.Succeeded(result.Value!.ToMessageDto(), "Message retrieved", 200);
    }

    #endregion

    #region Criação

    public async Task<OperationResult<MessageDto>> CreateMessage(MessageDto dto)
    {
        try
        {
            var message = dto.ToMessage();
            var result = await _messageService.CreateMessageAsync(message);

            if (!result.IsSuccess)
                return OperationResult<MessageDto>.Failed(null, result.Error ?? "Message not created", 406);

            return OperationResult<MessageDto>.Succeeded(message.ToMessageDto(), "Message created", 201);
        }
        catch (Exception ex)
        {
            return OperationResult<MessageDto>.Failed(null, ex.Message, 400);
        }
    }

    #endregion

    #region Atualizações

    public async Task<OperationResult<MessageDto>> UpdateMessage(string id, MessageDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<MessageDto>.Failed(null, "Invalid ID format", 400);

        var existingResult = await _messageService.GetMessageByIdAsync(guid);
        if (!existingResult.IsSuccess)
            return OperationResult<MessageDto>.Failed(null, existingResult.Error!, 404);

        var message = existingResult.Value!;
        message.UpdateFromDto(dto);

        var updateResult = await _messageService.UpdateMessageAsync(message);
        if (!updateResult.IsSuccess)
            return OperationResult<MessageDto>.Failed(null, updateResult.Error ?? "Message not updated", 400);

        return OperationResult<MessageDto>.Succeeded(message.ToMessageDto(), "Message updated", 200);
    }


    #endregion

    #region Exclusão

    public async Task<OperationResult<bool>> DeleteMessage(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<bool>.Failed(false, "Invalid ID format", 400);

        var result = await _messageService.DeleteMessageAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<bool>.Failed(false, result.Error ?? "Message not deleted", 400);

        return OperationResult<bool>.Succeeded(true, "Message deleted", 200);
    }

    #endregion

}
