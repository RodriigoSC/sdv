using SDV.Application.Dtos.Orders;
using SDV.Domain.Entities.Orders;

namespace SDV.Application.Mappers;

public static class OrderMapper
{
    public static OrderDto ToDto(this Order order)
    {
        if (order == null) return null!;

        return new OrderDto
        {
            Id = order.Id.ToString(),
            ClientId = order.ClientId.ToString(),
            PlanId = order.PlanId.ToString(),
            PlanName = order.Plan?.Name ?? string.Empty,
            PlanPrice = order.Plan?.Price ?? 0,
            StartDate = order.StartDate,
            EndDate = order.EndDate,
            Status = order.Status.ToString(),
            ClientName = order.Client?.Name ?? string.Empty
        };
    }

    public static IEnumerable<OrderDto> ToDtoList(this IEnumerable<Order> orders)
    {
        if (orders == null) yield break;
        foreach (var order in orders)
            yield return order.ToDto();
    }
}
