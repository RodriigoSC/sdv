using System;
using SDV.Domain.Entities.Messages;

namespace SDV.Domain.Interfaces.Messages;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetAllAsync(Guid clientId);
    Task<Message?> GetByIdAsync(Guid id);
    Task AddAsync(Message entity);
    Task UpdateAsync(Message entity);
    Task DeleteAsync(Guid id);
}
