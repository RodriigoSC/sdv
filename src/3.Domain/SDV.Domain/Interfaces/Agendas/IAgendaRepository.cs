using SDV.Domain.Entities.Agendas;

namespace SDV.Domain.Interfaces.Agendas;

public interface IAgendaRepository
{
    Task<IEnumerable<Agenda>> GetAllAsync(Guid clientId);
    Task<Agenda?> GetByIdAsync(Guid id);
    Task AddAsync(Agenda entity);
    Task UpdateAsync(Agenda entity);
    Task DeleteAsync(Guid id);
}
