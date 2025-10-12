using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Orders;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Orders;
using SDV.Domain.Interfaces.Payments;

namespace SDV.Infra.Data.Service.Orders;

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

    public async Task<Result<Order>> CreateOrderAsync(Order order)
    {
        var payment = new Payment(order.ClientId, order.Id, order.Plan.Price, PaymentProvider.MercadoPago);
        order.AddPayment(payment);

        // Salva o estado inicial antes de chamar o gateway
        await _orderRepository.AddAsync(order);
        await _paymentRepository.AddAsync(payment);

        // ✔️ CORRIGIDO: O request para o gateway DEVE conter nosso ID interno
        var checkoutResult = await _paymentService.GeneratePaymentCheckoutAsync(new PaymentGatewayRequest
        {
            Amount = payment.Amount,
            ExternalReference = payment.Id.ToString(), 
        });

        if (!checkoutResult.IsSuccess)
        {
            payment.Fail(checkoutResult.Error ?? "Falha na comunicação com o gateway.");
            order.MarkPaymentFailed();
            await _paymentRepository.UpdateAsync(payment);
            await _orderRepository.UpdateAsync(order);
            return Result<Order>.Failure("Falha ao criar pagamento. O pedido foi registrado como falho.");
        }

        // ✔️ CORRIGIDO: O resultado é apenas a string da URL
        var checkoutUrl = checkoutResult.Value;
        payment.SetCheckoutUrl(checkoutUrl); // Atualiza a entidade apenas com a URL
        await _paymentRepository.UpdateAsync(payment);
        
        return Result<Order>.Success(order);
    }

    // ✔️ CORRIGIDO: Lógica do Webhook totalmente refeita para ser robusta
    public async Task<Result<bool>> ProcessPaymentCallbackAsync(string gatewayTransactionId)
    {
        // Passo 1: Perguntar ao PaymentService qual é o nosso pagamento correspondente
        var paymentInfoResult = await _paymentService.GetPaymentInfoFromGatewayAsync(gatewayTransactionId);

        if (!paymentInfoResult.IsSuccess)
        {
            return Result<bool>.Failure(paymentInfoResult.Error);
        }

        var (internalPaymentId, statusFromGateway) = paymentInfoResult.Value;

        // Passo 2: Agora, com nosso ID interno, buscamos o pagamento no nosso banco
        var payment = await _paymentRepository.GetByIdAsync(internalPaymentId);
        if (payment == null)
        {
            return Result<bool>.Failure("Pagamento com a referência interna não encontrado no banco de dados.");
        }

        // Idempotência: Se o pagamento já foi processado, não faz nada
        if (payment.Status != PaymentStatus.Pending)
        {
            return Result<bool>.Success(true);
        }

        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
        if (order == null)
        {
            return Result<bool>.Failure("Pedido associado ao pagamento não encontrado.");
        }
        
        // Passo 3: Atualizar o estado com base na informação do gateway
        if (statusFromGateway == PaymentStatus.Approved)
        {
            // Agora finalmente temos o TransactionId do gateway para salvar
            payment.Approve(gatewayTransactionId); 
            order.Activate();
        }
        else
        {
            payment.Fail($"Pagamento rejeitado pelo gateway com status: {statusFromGateway}");
            order.MarkPaymentFailed();
        }

        await _paymentRepository.UpdateAsync(payment);
        await _orderRepository.UpdateAsync(order);

        return Result<bool>.Success(true);
    }
    
    #region Métodos de Leitura e Ações Simples (sem alterações)
    public async Task<Result<Order>> ActivateOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return Result<Order>.Failure("Pedido não encontrado.");
        try
        {
            order.Activate();
            await _orderRepository.UpdateAsync(order);
            return Result<Order>.Success(order);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Order>.Failure(ex.Message);
        }
    }
    
    public async Task<Result<bool>> CancelOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return Result<bool>.Failure("Pedido não encontrado.");
        order.Cancel(); 
        await _orderRepository.UpdateAsync(order);
        return Result<bool>.Success(true);
    }
    
    public async Task<Result<Order>> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order != null ? Result<Order>.Success(order) : Result<Order>.Failure("Pedido não encontrado.");
    }

    public async Task<Result<Order>> GetActiveOrderByClientAsync(Guid clientId)
    {
        var order = await _orderRepository.GetActiveOrderByClientIdAsync(clientId);
        return order != null ? Result<Order>.Success(order) : Result<Order>.Failure("Nenhum pedido ativo ou pendente encontrado para este cliente.");
    }

    public async Task<Result<IEnumerable<Order>>> GetOrderHistoryByClientAsync(Guid clientId)
    {
        var orderHistory = await _orderRepository.GetOrderHistoryByClientIdAsync(clientId);
        return Result<IEnumerable<Order>>.Success(orderHistory);
    }

    public async Task<Result<Order>> UpdateOrderAsync(Order order)
    {
        await _orderRepository.UpdateAsync(order);
        return Result<Order>.Success(order);
    }
    #endregion
}