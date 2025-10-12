using SDV.Application.Dtos.Orders;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Entities.Orders;
using SDV.Domain.Interfaces.Clients;
using SDV.Domain.Interfaces.Orders;
using SDV.Domain.Interfaces.Plans;

namespace SDV.Application.Services;

/// <summary>
/// Serviço de aplicação para gerenciar pedidos
/// Responsável por operações de CRUD e gerenciamento de estado
/// </summary>
public class OrderApplication : IOrderApplication
{
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IPlanRepository _planRepository;

    public OrderApplication(
        IOrderService orderService,
        IOrderRepository orderRepository,
        IClientRepository clientRepository,
        IPlanRepository planRepository)
    {
        _orderService = orderService;
        _orderRepository = orderRepository;
        _clientRepository = clientRepository;
        _planRepository = planRepository;
    }

    public async Task<OperationResult<OrderDto>> CreateOrderAsync(CreateOrderRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.PlanId))
                return OperationResult<OrderDto>.Failed(null, "ClientId e PlanId são obrigatórios", 400);

            if (!Guid.TryParse(request.ClientId, out var clientGuid) || !Guid.TryParse(request.PlanId, out var planGuid))
                return OperationResult<OrderDto>.Failed(null, "IDs inválidos", 400);

            var client = await _clientRepository.GetByIdAsync(clientGuid);
            var plan = await _planRepository.GetByIdAsync(planGuid);

            if (client == null || plan == null)
                return OperationResult<OrderDto>.Failed(null, "Cliente ou Plano não encontrado", 404);

            var order = new Order(client, plan);
            var result = await _orderService.CreateOrderAsync(order);

            if (!result.IsSuccess || result.Value == null)
                return OperationResult<OrderDto>.Failed(null, result.Error ?? "Falha ao criar pedido", 400);

            return OperationResult<OrderDto>.Succeeded(result.Value.ToDto(), "Pedido criado com sucesso", 201);
        }
        catch (Exception ex)
        {
            return OperationResult<OrderDto>.Failed(null, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<OrderDto>> GetOrderAsync(string orderId)
    {
        try
        {
            if (!Guid.TryParse(orderId, out var guid))
                return OperationResult<OrderDto>.Failed(null, "OrderId inválido", 400);

            var order = await _orderRepository.GetByIdAsync(guid);
            if (order == null)
                return OperationResult<OrderDto>.Failed(null, "Pedido não encontrado", 404);

            return OperationResult<OrderDto>.Succeeded(order.ToDto(), "Pedido encontrado", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<OrderDto>.Failed(null, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<OrderDto>> GetActiveOrderAsync(string clientId)
    {
        try
        {
            if (!Guid.TryParse(clientId, out var clientGuid))
                return OperationResult<OrderDto>.Failed(null, "ClientId inválido", 400);

            var order = await _orderRepository.GetActiveOrderByClientIdAsync(clientGuid);
            if (order == null)
                return OperationResult<OrderDto>.Failed(null, "Pedido ativo não encontrado", 404);

            return OperationResult<OrderDto>.Succeeded(order.ToDto(), "Pedido ativo encontrado", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<OrderDto>.Failed(null, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<IEnumerable<OrderDto>>> GetOrderHistoryAsync(string clientId)
    {
        try
        {
            if (!Guid.TryParse(clientId, out var clientGuid))
                return OperationResult<IEnumerable<OrderDto>>.Failed(null, "ClientId inválido", 400);

            var orders = await _orderRepository.GetOrderHistoryByClientIdAsync(clientGuid);
            if (orders == null || !orders.Any())
                return OperationResult<IEnumerable<OrderDto>>.Failed(null, "Nenhum pedido encontrado", 404);

            var dtos = orders.ToDtoList();
            return OperationResult<IEnumerable<OrderDto>>.Succeeded(dtos, "Histórico de pedidos encontrado", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<IEnumerable<OrderDto>>.Failed(null, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<bool>> CancelOrderAsync(string orderId)
    {
        try
        {
            if (!Guid.TryParse(orderId, out var guid))
                return OperationResult<bool>.Failed(false, "OrderId inválido", 400);

            var order = await _orderRepository.GetByIdAsync(guid);
            if (order == null)
                return OperationResult<bool>.Failed(false, "Pedido não encontrado", 404);

            order.Cancel();
            await _orderRepository.UpdateAsync(order);

            return OperationResult<bool>.Succeeded(true, "Pedido cancelado com sucesso", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failed(false, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<bool>> IsOrderActiveAsync(string orderId)
    {
        try
        {
            if (!Guid.TryParse(orderId, out var guid))
                return OperationResult<bool>.Failed(false, "OrderId inválido", 400);

            var order = await _orderRepository.GetByIdAsync(guid);
            if (order == null)
                return OperationResult<bool>.Failed(false, "Pedido não encontrado", 404);

            var isActive = order.IsActiveAndValid();
            return OperationResult<bool>.Succeeded(isActive, isActive ? "Pedido está ativo" : "Pedido não está ativo", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failed(false, $"Erro: {ex.Message}", 500);
        }
    }
}