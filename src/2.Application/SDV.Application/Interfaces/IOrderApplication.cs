using SDV.Application.Dtos.Orders;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

/// <summary>
/// Interface de aplicação para operações de Pedidos
/// Responsável por gerenciar o ciclo de vida dos pedidos
/// </summary>
public interface IOrderApplication
{
    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    Task<OperationResult<OrderDto>> CreateOrderAsync(CreateOrderRequestDto request);

    /// <summary>
    /// Obtém um pedido pelo ID
    /// </summary>
    Task<OperationResult<OrderDto>> GetOrderAsync(string orderId);

    /// <summary>
    /// Obtém o pedido ativo de um cliente
    /// </summary>
    Task<OperationResult<OrderDto>> GetActiveOrderAsync(string clientId);

    /// <summary>
    /// Obtém histórico completo de pedidos de um cliente
    /// </summary>
    Task<OperationResult<IEnumerable<OrderDto>>> GetOrderHistoryAsync(string clientId);

    /// <summary>
    /// Cancela um pedido
    /// </summary>
    Task<OperationResult<bool>> CancelOrderAsync(string orderId);

    /// <summary>
    /// Verifica se um pedido está ativo e válido
    /// </summary>
    Task<OperationResult<bool>> IsOrderActiveAsync(string orderId);
}
