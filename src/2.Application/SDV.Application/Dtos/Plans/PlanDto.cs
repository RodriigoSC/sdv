using System;
using SDV.Domain.Enums.Plans;

namespace SDV.Application.Dtos.Plans;

public class PlanDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PlanType PlanType { get; set; }
    public bool IsActive { get; set; }

}
