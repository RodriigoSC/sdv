using SDV.Application.Dtos.Payments;
using SDV.Application.Dtos.Subscriptions;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Interfaces.Clients;
using SDV.Domain.Interfaces.Plans;
using SDV.Domain.Interfaces.Subscriptions;

namespace SDV.Application.Services;

public class SubscriptionApplication : ISubscriptionApplication
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IClientRepository _clientRepository;
    private readonly IPlanRepository _planRepository;


    public SubscriptionApplication(ISubscriptionService subscriptionService, IClientRepository clientRepository, IPlanRepository planRepository)
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

        var subscriptionResult = await _subscriptionService.CreateSubscriptionAsync(client, plan);

        if (!subscriptionResult.IsSuccess || subscriptionResult.Value == null) // Verificação de nulidade
        {
            return OperationResult<CreatePaymentDto>.Failed(null, subscriptionResult.Error ?? "Falha ao criar a assinatura.", 400);
        }

        // Agora o compilador sabe que .Value não é nulo aqui
        var responseDto = new CreatePaymentDto(
            subscriptionResult.Value.Id.ToString(),
            subscriptionResult.Value.TransactionId
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

    public async Task<OperationResult<SubscriptionDto>> GetSubscriptionByClientId(string clientId)
    {
        if (!Guid.TryParse(clientId, out var clientGuid))
        {
            return OperationResult<SubscriptionDto>.Failed(null, "ID de cliente inválido.", 400);
        }

        var result = await _subscriptionService.GetSubscriptionByClientAsync(clientGuid);

        if (!result.IsSuccess || result.Value == null)
        {
            return OperationResult<SubscriptionDto>.Failed(null, result.Error ?? "Assinatura não encontrada.", 404);
        }

        return OperationResult<SubscriptionDto>.Succeeded(result.Value.ToSubscriptionDto(), "Assinatura encontrada.");
    }
}
