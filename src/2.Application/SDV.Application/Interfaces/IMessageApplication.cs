using SDV.Application.Dtos.Messages;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface IMessageApplication
{
    #region Consultas
    Task<OperationResult<IEnumerable<MessageDto>>> GetAllMessages(string clientId);
    Task<OperationResult<MessageDto>> GetMessageById(string id);
    #endregion

    #region Criação
    Task<OperationResult<MessageDto>> CreateMessage(MessageDto dto);
    #endregion

    #region Atualizações
    Task<OperationResult<MessageDto>> UpdateMessage(string id, MessageDto dto);
    
    #endregion

    #region Exclusão
    Task<OperationResult<bool>> DeleteMessage(string id);
    #endregion

}
