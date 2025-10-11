using SDV.Domain.Entities.Clients;

namespace SDV.Domain.Interfaces.Clients;

public interface IClientRepository
{
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(Guid id);
    Task AddAsync(Client entity);
    Task UpdateAsync(Client entity);
    Task DeleteAsync(Guid id);
}
