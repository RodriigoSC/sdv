namespace SDV.Domain.Enums.Orders;

public enum OrderStatus
{
    /// <summary>
    /// Pedido criado, aguardando pagamento
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Pagamento confirmado, pedido ativo/processando
    /// </summary>
    Active = 1,

    /// <summary>
    /// Pedido cancelado pelo cliente ou expirado
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Pagamento falhou ou rejeitado
    /// </summary>
    PaymentFailed = 3,

    /// <summary>
    /// Pedido expirou (fora da validade do plano)
    /// </summary>
    Expired = 4
}