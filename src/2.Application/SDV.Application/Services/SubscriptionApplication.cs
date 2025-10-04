using SDV.Application.Interfaces;
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

    public async Task<OperationResult<string>> Subscribe(string planId, string clientId)
    {
        if (!Guid.TryParse(clientId, out var clientGuid) || !Guid.TryParse(planId, out var planGuid))
        {
            return OperationResult<string>.Failed(null, "IDs inválidos", 400);
        }

        var client = await _clientRepository.GetByIdAsync(clientGuid);
        var plan = await _planRepository.GetByIdAsync(planGuid);

        if (client == null || plan == null)
        {
            return OperationResult<string>.Failed(null, "Cliente ou Plano não encontrado", 404);
        }

        var subscriptionResult = await _subscriptionService.CreateSubscriptionAsync(client, plan);

        if (!subscriptionResult.IsSuccess)
        {
            return OperationResult<string>.Failed(null, subscriptionResult.Error, 400);
        }

        return OperationResult<string>.Succeeded(subscriptionResult.Value.Id.ToString(), "Assinatura criada com sucesso", 201);
    }
}
