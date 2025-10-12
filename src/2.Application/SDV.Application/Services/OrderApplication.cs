using SDV.Application.Dtos.Orders;
using SDV.Application.Dtos.Payments;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Entities.Orders;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Clients;
using SDV.Domain.Interfaces.Orders;
using SDV.Domain.Interfaces.Plans;

namespace SDV.Application.Services;

public class OrderApplication : IOrderApplication
{
    private readonly IOrderService _subscriptionService;
    private readonly IClientRepository _clientRepository;
    private readonly IPlanRepository _planRepository;


    public OrderApplication(IOrderService subscriptionService, IClientRepository clientRepository, IPlanRepository planRepository)
    {
        _subscriptionService = subscriptionService;
        _clientRepository = clientRepository;
        _planRepository = planRepository;
    }

    public async Task<OperationResult<CreatePaymentDto>> Subscribe(string planId, string clientId)
    {
        if (!Guid.TryParse(clientId, out var clientGuid) || !Guid.TryParse(planId, out var planGuid))
        {
            return OperationResult<CreatePaymentDto>.Failed(null, "IDs inválidos", 400);
        }

        var client = await _clientRepository.GetByIdAsync(clientGuid);
        var plan = await _planRepository.GetByIdAsync(planGuid);

        if (client == null || plan == null)
        {
            return OperationResult<CreatePaymentDto>.Failed(null, "Cliente ou Plano não encontrado", 404);
        }

        var order = new Order(client, plan);
        var payment = new Payment(order.Id, plan.Price, PaymentProvider.MercadoPago);
        order.AddPayment(payment);
        
        var subscriptionResult = await _subscriptionService.CreateOrderAsync(order);

        if (!subscriptionResult.IsSuccess || subscriptionResult.Value == null) 
        {
            return OperationResult<CreatePaymentDto>.Failed(null, subscriptionResult.Error ?? "Falha ao criar a assinatura.", 400);
        }
        
        var responseDto = new CreatePaymentDto(
            subscriptionResult.Value.Id.ToString(),
            subscriptionResult.Value.Payments.First().PaymentUrl
        );

        return OperationResult<CreatePaymentDto>.Succeeded(responseDto, "Assinatura criada com sucesso", 201);
    }

    public async Task<OperationResult<bool>> ProcessPaymentCallback(string paymentId, string secret)
    {
        if (string.IsNullOrEmpty(paymentId))
        {
            return OperationResult<bool>.Failed(false, "ID do pagamento inválido.", 400);
        }
        var result = await _subscriptionService.ProcessPaymentCallbackAsync(paymentId, secret);

        if (!result.IsSuccess)
        {
            if (result.Error == "Secret inválido.")
            {
                return OperationResult<bool>.Failed(false, result.Error, 401);
            }
            
            return OperationResult<bool>.Failed(false, result.Error ?? "", 400);
        }
        return OperationResult<bool>.Succeeded(true, "Callback processado com sucesso.", 200);
    }

    public async Task<OperationResult<OrderDto>> GetOrderByClientId(string clientId)
    {
        if (!Guid.TryParse(clientId, out var clientGuid))
        {
            return OperationResult<OrderDto>.Failed(null, "ID de cliente inválido.", 400);
        }

        var result = await _subscriptionService.GetLastOrderByClientAsync(clientGuid);

        if (!result.IsSuccess || result.Value == null)
        {
            return OperationResult<OrderDto>.Failed(null, result.Error ?? "Assinatura não encontrada.", 404);
        }

        return OperationResult<OrderDto>.Succeeded(result.Value.ToOrderDto(), "Assinatura encontrada.");
    }
}