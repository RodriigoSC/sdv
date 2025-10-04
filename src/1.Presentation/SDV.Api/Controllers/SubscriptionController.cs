using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SDV.Api.Middlewares;
using SDV.Application.Interfaces;

namespace SDV.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionController : BaseController
{
    private readonly ISubscriptionApplication _subscriptionApplication;

    public SubscriptionController(ISubscriptionApplication subscriptionApplication)
    {
        _subscriptionApplication = subscriptionApplication;
    }

    [HttpPost("subscribe/{planId}")]
    public async Task<IActionResult> Subscribe(string planId, [FromBody] string clientId)
    {
        var response = await _subscriptionApplication.Subscribe(planId, clientId);
        int statusCode = response.OperationCode;

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetSubscription(string clientId)
    {
        var response = await _subscriptionApplication.GetSubscriptionByClientId(clientId);
        int statusCode = response.OperationCode;

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }

    [HttpPost("payment/callback")]
    public async Task<IActionResult> PaymentCallback([FromBody] JObject payload)
    {
        // O Mercado Pago envia um payload com o ID do pagamento dentro de "data.id" quando o tipo é "payment"
        string? type = payload["type"]?.ToString();
        if (type == "payment")
        {
            string? paymentId = payload["data"]?["id"]?.ToString();
            if (!string.IsNullOrEmpty(paymentId))
            {
                await _subscriptionApplication.ProcessPaymentCallback(paymentId);
            }
        }
        
        // Retorna 200 OK para o Mercado Pago para confirmar o recebimento.
        return Ok();
    }
}
