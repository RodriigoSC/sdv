using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;

namespace SDV.Domain.Interfaces.Payments;

public interface IPaymentService
{
    Task<Result<string>> CreatePaymentAsync(PaymentGatewayRequest request);

    Task<Result<Payment>> ProcessPaymentAsync(Payment payment);
}
