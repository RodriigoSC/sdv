using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Enums.Orders;
using SDV.Domain.Enums.Plans;

namespace SDV.Domain.Entities.Orders;

public class Order : BaseEntity
{
    public Guid ClientId { get; private set; }
    public virtual Client Client { get; private set; }
    public Guid PlanId { get; private set; }
    public virtual Plan Plan { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    public Order(Client client, Plan plan)
    {
        Id = Guid.NewGuid();
        ClientId = client.Id;
        PlanId = plan.Id;
        Client = client;
        Plan = plan;
        
        StartDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;

        UpdateExpirationDate();
    }
    
    public void AddPayment(Payment payment)
    {
        if (payment.OrderId != this.Id)
        {
            throw new InvalidOperationException("Não é possível adicionar um pagamento de outra ordem.");
        }
        Payments.Add(payment);
        MarkAsUpdated();
    }

    public void Activate()
    {
        // O ideal é que uma ordem só fique ativa se não estiver cancelada ou com pagamento falho
        if (Status != OrderStatus.Cancelled && Status != OrderStatus.PaymentFailed)
        {
            Status = OrderStatus.Processing; // Usando "Processing" como "Ativo"
            MarkAsUpdated();
        }
    }
    
    public void Suspend()
    {
        Status = OrderStatus.PaymentFailed;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
        MarkAsUpdated();
    }    

    private void UpdateExpirationDate()
    {
        switch (Plan.PlanType)
        {
            case PlanType.Monthly:
                EndDate = StartDate.AddMonths(1);
                break;
            case PlanType.Semiannually: 
                EndDate = StartDate.AddMonths(6);
                break;
            case PlanType.Annually:
                EndDate = StartDate.AddYears(1);
                break;
            default:
                throw new ArgumentException("Tipo de plano inválido para calcular a data de expiração.");
        }
    }
}
