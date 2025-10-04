using System;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Entities.Subscriptions;
using SDV.Domain.Enums.Payments;

namespace SDV.Domain.Interfaces.Payments;

public interface IPaymentGateway
{
    Task<Result<string>> CreatePaymentAsync(Subscription subscription, Plan plan);
    Task<Result<PaymentStatus>> GetPaymentStatusAsync(string transactionId);

}
