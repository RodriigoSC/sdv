using System;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Messages;

namespace SDV.Domain.Interfaces.Messages;

public interface IMessageService
{
    #region Consultas
    Task<Result<IEnumerable<Message>>> GetAllMessagesAsync(Guid userId);
    Task<Result<Message>> GetMessageByIdAsync(Guid id);
    #endregion

    #region Criação
    Task<Result<Message>> CreateMessageAsync(Message message);
    #endregion

    #region Atualizações
    Task<Result<Message>> UpdateMessageAsync(Message message);
    #endregion

    #region Exclusão
    Task<Result<bool>> DeleteMessageAsync(Guid id);    
    #endregion

}
