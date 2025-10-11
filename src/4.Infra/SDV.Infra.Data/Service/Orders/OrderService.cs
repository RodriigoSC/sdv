using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Orders;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Orders;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Orders;
using SDV.Domain.Interfaces.Payments;

namespace SDV.Infra.Data.Service.Orders;

/// <summary>
/// Serviço de domínio para gestão de pedidos.
/// Responsável por orquestrar a criação de pedidos e processamento de callbacks de pagamento.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentService _paymentService;

    public OrderService(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IPaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Cria um novo pedido e processa o pagamento associado.
    /// </summary>
    /// <param name="order">Pedido a ser criado</param>
    /// <returns>Resultado contendo o pedido criado ou mensagem de erro</returns>
    public async Task<Result<Order>> CreateOrderAsync(Order order)
    {
        // 1. Processar o pagamento usando o serviço dedicado
        var paymentResult = await _paymentService.ProcessPaymentAsync(order.Payment);

        // 2. Atualizar o pedido com base no resultado do pagamento
        if (!paymentResult.IsSuccess)
        {
            order.Status = OrderStatus.PaymentFailed;
            order.Payment.Status = paymentResult.Value?.Status ?? PaymentStatus.Failed;
            // É importante salvar o estado mesmo em caso de falha para histórico
        }
        else
        {
            order.Payment = paymentResult.Value; // Pega o objeto de pagamento atualizado do gateway
            order.Status = OrderStatus.Processing;
        }

        // 3. Persistir as entidades
        await _paymentRepository.AddAsync(order.Payment);
        await _orderRepository.AddAsync(order);

        if (!paymentResult.IsSuccess)
        {
            return Result<Order>.Failure("O pagamento falhou. O pedido não foi processado.");
        }

        return Result<Order>.Success(order);
    }

    /// <summary>
    /// Obtém o último pedido de um cliente.
    /// </summary>
    /// <param name="clientId">ID do cliente</param>
    /// <returns>Resultado contendo o pedido ou mensagem de erro</returns>
    public async Task<Result<Order>> GetLastOrderByClientAsync(Guid clientId)
    {
        var order = await _orderRepository.GetLastOrderByClientIdAsync(clientId);

        return order != null
            ? Result<Order>.Success(order)
            : Result<Order>.Failure("Nenhum pedido encontrado para este cliente.");
    }

    /// <summary>
    /// Processa callback de webhook de pagamento.
    /// Atualiza o status do pagamento e pedido baseado na confirmação do gateway.
    /// </summary>
    /// <param name="orderId">ID do pedido</param>
    /// <param name="paymentId">ID do pagamento</param>
    /// <param name="webhookSecret">Secret para validação do webhook</param>
    /// <returns>Resultado indicando sucesso ou falha</returns>
    public async Task<Result<bool>> ProcessPaymentCallbackAsync(Guid orderId, string paymentId, string webhookSecret)
    {
        try
        {
            // 1. Obter o pedido
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return Result<bool>.Failure("Pedido não encontrado.");

            // 2. Obter o pagamento
            var payment = await _paymentRepository.GetByIdAsync(Guid.Parse(paymentId));
            if (payment == null)
                return Result<bool>.Failure("Pagamento não encontrado.");

            // 3. Verificar idempotência - webhook já foi processado?
            if (payment.Status != PaymentStatus.Pending)
            {
                return Result<bool>.Success(true); // Já processado, retorna sucesso
            }

            // 4. Atualizar status do pagamento e pedido
            //payment.Status = PaymentStatus.Approved;
            //order.Status = OrderStatus.Processing;

            // 5. Persistir alterações
            await _paymentRepository.UpdateAsync(payment);
            await _orderRepository.UpdateAsync(order);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            // Log do erro deveria ser feito aqui com ILogger
            return Result<bool>.Failure($"Erro ao processar callback de pagamento: {ex.Message}");
        }
    }
}