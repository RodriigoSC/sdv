using System;
using SDV.Domain.Entities.Subscriptions;

namespace SDV.Domain.Interfaces.Subscriptions;

public interface ISubscriptionRepository
{
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<Subscription?> GetByIdAsync(Guid id);
    Task AddAsync(Subscription entity);
    Task UpdateAsync(Subscription entity);
    Task DeleteAsync(Guid id);

}
