using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Orders;

namespace SDV.Domain.Interfaces.Orders;

/// <summary>
/// Interface para operações de domínio relacionadas a Pedidos
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Cria um novo pedido com status Pending
    /// </summary>
    Task<Result<Order>> CreateOrderAsync(Order order);

    /// <summary>
    /// Obtém um pedido pelo ID
    /// </summary>
    Task<Result<Order>> GetOrderByIdAsync(Guid orderId);

    /// <summary>
    /// Obtém o último pedido ativo/pendente de um cliente
    /// </summary>
    Task<Result<Order>> GetActiveOrderByClientAsync(Guid clientId);

    /// <summary>
    /// Obtém todo o histórico de pedidos de um cliente
    /// </summary>
    Task<Result<IEnumerable<Order>>> GetOrderHistoryByClientAsync(Guid clientId);

    /// <summary>
    /// Atualiza um pedido existente
    /// </summary>
    Task<Result<Order>> UpdateOrderAsync(Order order);

    /// <summary>
    /// Ativa um pedido após confirmação de pagamento
    /// </summary>
    Task<Result<Order>> ActivateOrderAsync(Guid orderId);

    /// <summary>
    /// Cancela um pedido
    /// </summary>
    Task<Result<bool>> CancelOrderAsync(Guid orderId);
}
