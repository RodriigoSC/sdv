using SDV.Application.Dtos.Orders;
using SDV.Domain.Entities.Orders;

namespace SDV.Application.Mappers;

public static class SubscriptionMapper
{
    // <summary>
    /// Converte uma entidade Subscription para um SubscriptionDto.
    /// </summary>
    public static OrderDto ToSubscriptionDto(this Order subscription)
    {
        if (subscription == null) return null!;

        return new OrderDto
        {
            Id = subscription.Id.ToString(),
            ClientId = subscription.ClientId.ToString(),
            PlanId = subscription.PlanId.ToString(),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            Status = subscription.Status
        };
    }

    /// <summary>
    /// Converte uma lista de entidades Subscription para uma lista de SubscriptionDto.
    /// </summary>
    public static IEnumerable<OrderDto> ToSubscriptionDtoList(this IEnumerable<Order> subscriptions)
    {
        if (subscriptions == null) yield break;

        foreach (var subscription in subscriptions)
        {
            yield return subscription.ToSubscriptionDto();
        }
    }

}
