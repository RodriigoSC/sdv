using SDV.Application.Dtos.Payments;
using SDV.Domain.Entities.Payments;

namespace SDV.Application.Mappers;

public static class PaymentMapper
{
    public static PaymentDto ToDto(this Payment payment)
    {
        if (payment == null) return null!;

        return new PaymentDto
        {
            Id = payment.Id.ToString(),
            OrderId = payment.OrderId.ToString(),
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            Provider = payment.Provider.ToString(),
            TransactionId = payment.TransactionId,
            CreatedAt = payment.CreatedAt,
            ApprovedAt = payment.ApprovedAt,
            FailureReason = payment.FailureReason
        };
    }

    public static IEnumerable<PaymentDto> ToDtoList(this IEnumerable<Payment> payments)
    {
        if (payments == null) yield break;
        foreach (var payment in payments)
            yield return payment.ToDto();
    }
}
