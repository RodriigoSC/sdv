using SDV.Application.Dtos.Clients;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface IClientApplication
{
    #region Consultas
    Task<OperationResult<IEnumerable<ClientDto>>> GetAllClients();
    Task<OperationResult<IEnumerable<ClientDto>>> GetActiveClients();
    Task<OperationResult<ClientDto>> GetOneClient(string id);
    Task<OperationResult<ClientDto>> FindClientByEmail(string email);
    #endregion

    #region Criação
    Task<OperationResult<ClientDto>> CreateClient(ClientDto dto);
    #endregion

    #region Atualizações
    Task<OperationResult<ClientDto>> UpdateClient(string id, ClientDto dto);
    Task<OperationResult<ClientDto>> UpdateClientEmail(string id, string newEmail);
    Task<OperationResult<ClientDto>> ChangeClientPassword(string id, string newPassword);
    Task<OperationResult<ClientDto>> ResetClientPassword(string id, string tempPassword);
    #endregion

    #region Ativação / Desativação
    Task<OperationResult<ClientDto>> ActivateClient(string id);
    Task<OperationResult<bool>> DeactivateClient(string id);
    #endregion

    #region Verificação de Email
    Task<OperationResult<ClientDto>> GenerateEmailVerificationToken(string id);
    Task<OperationResult<ClientDto>> VerifyClientEmail(string id, string token);
    #endregion
}
