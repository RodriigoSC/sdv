using System;
using SDV.Application.Dtos.Payments;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Entities.Orders;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Clients;
using SDV.Domain.Interfaces.Orders;
using SDV.Domain.Interfaces.Payments;
using SDV.Domain.Interfaces.Plans;

namespace SDV.Application.Services;

/// <summary>
/// Serviço de aplicação para gerenciar pagamentos
/// Coordena a criação de checkouts e processamento de callbacks
/// </summary>
public class PaymentApplication : IPaymentApplication
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly IClientRepository _clientRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;

    public PaymentApplication(
        IPaymentService paymentService,
        IOrderService orderService,
        IClientRepository clientRepository,
        IPlanRepository planRepository,
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository)
    {
        _paymentService = paymentService;
        _orderService = orderService;
        _clientRepository = clientRepository;
        _planRepository = planRepository;
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<OperationResult<PaymentCheckoutResponseDto>> InitiatePaymentCheckoutAsync(InitiatePaymentRequestDto request)
    {
        try
        {
            // Validações
            if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.PlanId))
                return OperationResult<PaymentCheckoutResponseDto>.Failed(null, "ClientId e PlanId são obrigatórios", 400);

            if (!Guid.TryParse(request.ClientId, out var clientGuid) || !Guid.TryParse(request.PlanId, out var planGuid))
                return OperationResult<PaymentCheckoutResponseDto>.Failed(null, "IDs inválidos", 400);

            // Obter cliente e plano
            var client = await _clientRepository.GetByIdAsync(clientGuid);
            var plan = await _planRepository.GetByIdAsync(planGuid);

            if (client == null)
                return OperationResult<PaymentCheckoutResponseDto>.Failed(null, "Cliente não encontrado", 404);

            if (plan == null)
                return OperationResult<PaymentCheckoutResponseDto>.Failed(null, "Plano não encontrado", 404);

            // Criar pedido
            var order = new Order(client, plan);
            var orderResult = await _orderService.CreateOrderAsync(order);

            if (!orderResult.IsSuccess || orderResult.Value == null)
                return OperationResult<PaymentCheckoutResponseDto>.Failed(null, orderResult.Error ?? "Falha ao criar pedido", 400);

            var createdOrder = orderResult.Value;

            var payment = new Payment(client.Id, createdOrder.Id, plan.Price, PaymentProvider.MercadoPago);

            // Gerar request para gateway
            var gatewayRequest = new PaymentGatewayRequest(createdOrder, plan, "", "", "", "", "");
            var checkoutResult = await _paymentService.GeneratePaymentCheckoutAsync(gatewayRequest);

            if (!checkoutResult.IsSuccess || string.IsNullOrEmpty(checkoutResult.Value))
                return OperationResult<PaymentCheckoutResponseDto>.Failed(null, checkoutResult.Error ?? "Falha ao gerar checkout", 400);

            // Adicionar URL de checkout ao pagamento
            payment.SetCheckoutUrl(checkoutResult.Value);
            createdOrder.AddPayment(payment);

            // Persistir
            await _paymentRepository.AddAsync(payment);
            await _orderRepository.UpdateAsync(createdOrder);

            var response = new PaymentCheckoutResponseDto
            {
                OrderId = createdOrder.Id.ToString(),
                PaymentId = payment.Id.ToString(),
                CheckoutUrl = checkoutResult.Value,
                Amount = plan.Price,
                Status = payment.Status.ToString()
            };

            return OperationResult<PaymentCheckoutResponseDto>.Succeeded(response, "Checkout criado com sucesso", 201);
        }
        catch (Exception ex)
        {
            return OperationResult<PaymentCheckoutResponseDto>.Failed(null, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<PaymentDto>> ProcessPaymentApprovalCallbackAsync(PaymentWebhookCallbackDto callback)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(callback.PaymentId))
                return OperationResult<PaymentDto>.Failed(null, "PaymentId é obrigatório", 400);

            if (!Guid.TryParse(callback.PaymentId, out var paymentGuid))
                return OperationResult<PaymentDto>.Failed(null, "PaymentId inválido", 400);

            // Obter pagamento
            var payment = await _paymentRepository.GetByIdAsync(paymentGuid);
            if (payment == null)
                return OperationResult<PaymentDto>.Failed(null, "Pagamento não encontrado", 404);

            // Processar aprovação
            var result = await _paymentService.ProcessPaymentApprovalAsync(payment.Id.ToString());
            if (!result.IsSuccess)
                return OperationResult<PaymentDto>.Failed(null, result.Error, 400);

            var updatedPayment = result.Value;

            // Ativar pedido associado
            var orderResult = await _orderService.ActivateOrderAsync(updatedPayment.OrderId);
            if (!orderResult.IsSuccess)
                return OperationResult<PaymentDto>.Failed(null, "Falha ao ativar pedido", 400);

            return OperationResult<PaymentDto>.Succeeded(updatedPayment.ToDto(), "Pagamento aprovado com sucesso", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<PaymentDto>.Failed(null, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<bool>> ProcessPaymentFailureCallbackAsync(PaymentWebhookCallbackDto callback)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(callback.PaymentId))
                return OperationResult<bool>.Failed(false, "PaymentId é obrigatório", 400);

            var result = await _paymentService.ProcessPaymentFailureAsync(callback.PaymentId, callback.Status ?? "Motivo desconhecido");

            if (!result.IsSuccess)
                return OperationResult<bool>.Failed(false, result.Error, 400);

            return OperationResult<bool>.Succeeded(true, "Falha de pagamento processada", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failed(false, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<PaymentDto>> GetPaymentAsync(string paymentId)
    {
        try
        {
            if (!Guid.TryParse(paymentId, out var guid))
                return OperationResult<PaymentDto>.Failed(null, "PaymentId inválido", 400);

            var payment = await _paymentRepository.GetByIdAsync(guid);
            if (payment == null)
                return OperationResult<PaymentDto>.Failed(null, "Pagamento não encontrado", 404);

            return OperationResult<PaymentDto>.Succeeded(payment.ToDto(), "Pagamento encontrado", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<PaymentDto>.Failed(null, $"Erro: {ex.Message}", 500);
        }
    }

    public async Task<OperationResult<IEnumerable<PaymentDto>>> GetPaymentHistoryAsync(string clientId)
    {
        try
        {
            if (!Guid.TryParse(clientId, out var clientGuid))
                return OperationResult<IEnumerable<PaymentDto>>.Failed(null, "ClientId inválido", 400);

            var payments = await _paymentRepository.GetPaymentsByClientIdAsync(clientGuid);
            if (payments == null || !payments.Any())
                return OperationResult<IEnumerable<PaymentDto>>.Failed(null, "Nenhum pagamento encontrado", 404);

            var dtos = payments.ToDtoList();
            return OperationResult<IEnumerable<PaymentDto>>.Succeeded(dtos, "Histórico de pagamentos encontrado", 200);
        }
        catch (Exception ex)
        {
            return OperationResult<IEnumerable<PaymentDto>>.Failed(null, $"Erro: {ex.Message}", 500);
        }
    }
}
