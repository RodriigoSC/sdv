using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Orders;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Orders;
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
        var payment = order.Payments.First();
        var paymentResult = await _paymentService.CreatePaymentAsync(new PaymentGatewayRequest
        {
            Amount = payment.Amount,
            Description = $"Plano {order.Plan.Name}",
            CustomerName = $"{order.Client.FirstName} {order.Client.LastName}",
            CustomerEmail = order.Client.Email.ToString(),
            ExternalReference = order.Id.ToString(),
            PaymentProvider = payment.PaymentProvider,
        });

        if (!paymentResult.IsSuccess)
        {
            order.Suspend();
            payment.MarkAsFailed();
        }
        else
        {
            payment.AddGatewayResponseDetails(paymentResult.Value, paymentResult.Value);
        }

        await _paymentRepository.AddAsync(payment);
        await _orderRepository.AddAsync(order);

        if (!paymentResult.IsSuccess)
        {
            return Result<Order>.Failure("O pagamento falhou. O pedido não foi processado.");
        }

        return Result<Order>.Success(order);
    }

    public async Task<Result<Order>> GetLastOrderByClientAsync(Guid clientId)
    {
        var order = await _orderRepository.GetLastOrderByClientIdAsync(clientId);

        return order != null
            ? Result<Order>.Success(order)
            : Result<Order>.Failure("Nenhum pedido encontrado para este cliente.");
    }

    public async Task<Result<bool>> ProcessPaymentCallbackAsync(string paymentId, string webhookSecret)
    {
        try
        {
            var paymentGateway = _paymentService.GetPaymentGateway(PaymentProvider.MercadoPago);
            var externalReferenceResult = await paymentGateway.GetPaymentExternalReferenceAsync(paymentId);

            if (!externalReferenceResult.IsSuccess)
            {
                return Result<bool>.Failure("Não foi possível obter a referência externa do pagamento.");
            }

            if (!Guid.TryParse(externalReferenceResult.Value, out var orderId))
            {
                return Result<bool>.Failure("Referência externa inválida.");
            }
            
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return Result<bool>.Failure("Pedido não encontrado.");

            var payment = order.Payments.FirstOrDefault(p => p.TransactionId == paymentId || p.Id.ToString() == paymentId);
            if (payment == null)
                return Result<bool>.Failure("Pagamento não encontrado.");
            
            if (payment.Status != PaymentStatus.Pending)
            {
                return Result<bool>.Success(true); 
            }
            
            var statusResult = await paymentGateway.GetPaymentStatusAsync(paymentId);

            if (!statusResult.IsSuccess)
            {
                return Result<bool>.Failure("Não foi possível obter o status do pagamento.");
            }

            if (statusResult.Value == PaymentStatus.Approved)
            {
                payment.MarkAsApproved(paymentId);
                order.Activate();
            }
            else
            {
                payment.MarkAsFailed();
                order.Suspend();
            }

            await _paymentRepository.UpdateAsync(payment);
            await _orderRepository.UpdateAsync(order);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Erro ao processar callback de pagamento: {ex.Message}");
        }
    }
}