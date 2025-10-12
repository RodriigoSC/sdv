using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Enums.Orders;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Enums.Plans;

namespace SDV.Domain.Entities.Orders;

/// <summary>
/// Representa um pedido/assinatura de um cliente a um plano.
/// Gerencia o ciclo de vida completo da assinatura.
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// ID do cliente proprietário deste pedido
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Referência ao cliente (navegação)
    /// </summary>
    public virtual Client? Client { get; private set; }

    /// <summary>
    /// ID do plano escolhido
    /// </summary>
    public Guid PlanId { get; private set; }

    /// <summary>
    /// Referência ao plano (navegação)
    /// </summary>
    public virtual Plan? Plan { get; private set; }

    /// <summary>
    /// Data de início da assinatura
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Data de encerramento previsto da assinatura
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Status atual do pedido
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Coleção de pagamentos associados a este pedido
    /// </summary>
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    public Order(Client client, Plan plan)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client), "Cliente é obrigatório");
        if (plan == null)
            throw new ArgumentNullException(nameof(plan), "Plano é obrigatório");

        Id = Guid.NewGuid();
        ClientId = client.Id;
        Client = client;
        PlanId = plan.Id;
        Plan = plan;
        StartDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;

        CalculateEndDate();
        MarkAsUpdated();
    }

    /// <summary>
    /// Adiciona um novo pagamento a este pedido
    /// </summary>
    public void AddPayment(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment), "Pagamento é obrigatório");

        if (payment.OrderId != Id)
            throw new InvalidOperationException($"O pagamento não pertence a este pedido. Esperado: {Id}, Recebido: {payment.OrderId}");

        Payments.Add(payment);
        MarkAsUpdated();
    }

    /// <summary>
    /// Obtém o último pagamento realizado para este pedido
    /// </summary>
    public Payment? GetLastPayment() => Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

    /// <summary>
    /// Obtém todos os pagamentos aprovados
    /// </summary>
    public IEnumerable<Payment> GetApprovedPayments() => Payments.Where(p => p.Status == PaymentStatus.Approved);

    /// <summary>
    /// Ativa o pedido (marca como ativo após aprovação de pagamento)
    /// </summary>
    public void Activate()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Não é possível ativar um pedido cancelado");

        if (Status == OrderStatus.PaymentFailed)
            throw new InvalidOperationException("Não é possível ativar um pedido com falha no pagamento");

        Status = OrderStatus.Active;
        MarkAsUpdated();
    }

    /// <summary>
    /// Cancela o pedido
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            return;

        Status = OrderStatus.Cancelled;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marca o pedido com falha no pagamento
    /// </summary>
    public void MarkPaymentFailed()
    {
        if (Status == OrderStatus.Active)
            throw new InvalidOperationException("Não é possível marcar com falha um pedido já ativo");

        Status = OrderStatus.PaymentFailed;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marca o pedido como expirado
    /// </summary>
    public void MarkAsExpired()
    {
        if (Status == OrderStatus.Expired)
            return;

        Status = OrderStatus.Expired;
        MarkAsUpdated();
    }

    /// <summary>
    /// Verifica se o pedido está ativo e válido
    /// </summary>
    public bool IsActiveAndValid()
    {
        return Status == OrderStatus.Active && 
               EndDate.HasValue && 
               DateTime.UtcNow <= EndDate.Value;
    }

    /// <summary>
    /// Verifica se o pedido expirou
    /// </summary>
    public bool IsExpired()
    {
        return EndDate.HasValue && DateTime.UtcNow > EndDate.Value;
    }

    /// <summary>
    /// Calcula a data de expiração baseado no tipo de plano
    /// </summary>
    private void CalculateEndDate()
    {
        if (Plan == null)
            return;

        EndDate = Plan.PlanType switch
        {
            PlanType.Monthly => StartDate.AddMonths(1),
            PlanType.Semiannually => StartDate.AddMonths(6),
            PlanType.Annually => StartDate.AddYears(1),
            _ => StartDate.AddMonths(1)
        };
    }
}
