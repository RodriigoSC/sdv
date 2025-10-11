using SDV.Application.Dtos.Plans;
using SDV.Domain.Entities.Plans;

namespace SDV.Application.Mappers;

public static class PlanMapper
{
    public static PlanDto ToPlanDto(this Plan plan)
    {
        if (plan == null) return null!;

        return new PlanDto
        {
            Id = plan.Id.ToString(),
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            PlanType = plan.PlanType,
            IsActive = plan.IsActive
        };
    }

    public static Plan ToPlan(this PlanDto dto)
    {
        if (dto == null) return null!;

        return new Plan(
            name: dto.Name,
            description: dto.Description,
            price: dto.Price,
            planType: dto.PlanType
        );
    }
    
    public static IEnumerable<PlanDto> ToPlanDtoList(this IEnumerable<Plan> plans)
    {
        if (plans == null) yield break;
        foreach (var plan in plans)
            yield return plan.ToPlanDto();
    }
}