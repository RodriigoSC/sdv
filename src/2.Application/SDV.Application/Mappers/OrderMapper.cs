using SDV.Application.Dtos.Orders;
using SDV.Domain.Entities.Orders;

namespace SDV.Application.Mappers;

public static class OrderMapper
{
    public static OrderDto ToOrderDto(this Order order)
    {
        if (order == null) return null!;

        return new OrderDto
        {
            Id = order.Id.ToString(),
            ClientId = order.ClientId.ToString(),
            PlanId = order.PlanId.ToString(),
            StartDate = order.StartDate,
            EndDate = order.EndDate,
            Status = order.Status
        };
    }
    
    public static IEnumerable<OrderDto> ToOrderDtoList(this IEnumerable<Order> orders)
    {
        if (orders == null) yield break;

        foreach (var order in orders)
        {
            yield return order.ToOrderDto();
        }
    }

}
