using SDV.Domain.Entities.Orders;

namespace SDV.Domain.Interfaces.Orders;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(Guid id);
    Task AddAsync(Order entity);
    Task UpdateAsync(Order entity);
    Task DeleteAsync(Guid id);
    Task<Order?> GetActiveOrderByClientIdAsync(Guid clientId);
    Task<IEnumerable<Order>> GetOrderHistoryByClientIdAsync(Guid clientId);  
}
