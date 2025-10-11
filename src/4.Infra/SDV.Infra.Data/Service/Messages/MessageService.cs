using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Messages;
using SDV.Domain.Exceptions;
using SDV.Domain.Interfaces.Messages;
using SDV.Domain.Specification;

namespace SDV.Infra.Data.Service.Messages;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;

    public MessageService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    #region Consultas

    public async Task<Result<IEnumerable<Message>>> GetAllMessagesAsync(Guid userId)
    {
        var messages = await _messageRepository.GetAllAsync(userId);
        return Result<IEnumerable<Message>>.Success(messages);
    }

    public async Task<Result<Message>> GetMessageByIdAsync(Guid id)
    {
        var message = await _messageRepository.GetByIdAsync(id);
        return message is null
            ? Result<Message>.Failure("Feriado não encontrado")
            : Result<Message>.Success(message);
    }

    #endregion

    #region Criação

    public async Task<Result<Message>> CreateMessageAsync(Message message)
    {
        try
        {
            new MessageValidationSpecification().IsValid(message);
        }
        catch (EntityValidationException ex)
        {
            return Result<Message>.Failure(ex.Message);
        }

        await _messageRepository.AddAsync(message);
        return Result<Message>.Success(message);
    }

    #endregion

    #region Atualizações

    public async Task<Result<Message>> UpdateMessageAsync(Message message)
    {
        var existing = await _messageRepository.GetByIdAsync(message.Id);
        if (existing is null)
            return Result<Message>.Failure("Feriado não encontrado");

        try
        {
            new MessageValidationSpecification().IsValid(message);
        }
        catch (EntityValidationException ex)
        {
            return Result<Message>.Failure(ex.Message);
        }

        await _messageRepository.UpdateAsync(message);
        return Result<Message>.Success(message);
    }


    #endregion

    #region Exclusão

    public async Task<Result<bool>> DeleteMessageAsync(Guid id)
    {
        var message = await _messageRepository.GetByIdAsync(id);
        if (message is null)
            return Result<bool>.Failure("Feriado não encontrado");

        await _messageRepository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }

    #endregion

}
