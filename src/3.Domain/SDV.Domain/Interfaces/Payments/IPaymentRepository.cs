using System;
using SDV.Domain.Entities.Payments;

namespace SDV.Domain.Interfaces.Payments;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(Guid id);
    Task AddAsync(Payment entity);
    Task UpdateAsync(Payment entity);
    Task DeleteAsync(Guid id);
}
