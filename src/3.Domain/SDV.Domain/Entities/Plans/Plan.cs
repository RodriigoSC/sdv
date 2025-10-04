using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums.Plans;

namespace SDV.Domain.Entities.Plans;

public class Plan : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public PlanType PlanType { get; private set; }
    public bool IsActive { get; private set; }

    public Plan(string name, string description, decimal price, PlanType planType)
    {
        Name = name;
        Description = description;
        Price = price;
        PlanType = planType;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    
    public DateTime CalculateEndDate(PlanType planType)
    {
        return planType switch
        {
            PlanType.Monthly => DateTime.UtcNow.AddMonths(1),
            PlanType.Semiannually => DateTime.UtcNow.AddMonths(6),
            PlanType.Annually => DateTime.UtcNow.AddYears(1),
            _ => DateTime.UtcNow.AddMonths(1)
        };
    }
}
