using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SDV.Api.Middlewares;
using SDV.Application.Interfaces;

namespace SDV.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : BaseController
{
    private readonly IOrderApplication _subscriptionApplication;
    private readonly ILogger<OrderController> _logger;


    public OrderController(IOrderApplication subscriptionApplication, ILogger<OrderController> logger)
    {
        _subscriptionApplication = subscriptionApplication;
        _logger = logger;

    }

    [HttpPost("subscribe/{planId}")]
    public async Task<IActionResult> Subscribe(string planId, [FromBody] string clientId)
    {
        var response = await _subscriptionApplication.Subscribe(planId, clientId);
        int statusCode = response.OperationCode;

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetOrder(string clientId)
    {
        var response = await _subscriptionApplication.GetOrderByClientId(clientId);
        int statusCode = response.OperationCode;

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }

    [HttpPost("payment/callback")]
    public async Task<IActionResult> PaymentCallback([FromBody] JObject payload, [FromQuery] string secret)
    {
       try
        {            
            // Log do payload completo para debug
            _logger.LogInformation("Webhook recebido do Mercado Pago: {Payload}", payload.ToString()); 

            // Validação básica do payload
            string? type = payload["type"]?.ToString();
            
            if (string.IsNullOrEmpty(type))
            {
                _logger.LogWarning("Webhook recebido sem tipo definido");
                return BadRequest("Tipo de notificação não especificado");
            }

            // O Mercado Pago envia diferentes tipos: payment, merchant_order, etc.
            if (type == "payment")
            {
                string? paymentId = payload["data"]?["id"]?.ToString();
                
                if (string.IsNullOrEmpty(paymentId))
                {
                    _logger.LogWarning("Webhook de pagamento sem ID");
                    return BadRequest("ID do pagamento não especificado");
                }

                _logger.LogInformation("Processando pagamento ID: {PaymentId}", paymentId);
                
                var result = await _subscriptionApplication.ProcessPaymentCallback(paymentId, secret);
                
                if (!result.IsSuccess)
                {
                    _logger.LogError("Erro ao processar callback: {Error}", result.Message);
                    return Ok(new { processed = false, error = result.Message });
                }

                _logger.LogInformation("Pagamento {PaymentId} processado com sucesso", paymentId);

                return Ok(new { processed = true });
            }

            // Outros tipos de notificação
            _logger.LogInformation("Tipo de notificação {Type} recebido mas não processado", type);
            return Ok(new { processed = false, reason = "tipo não processado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook do Mercado Pago");
            // Retorna 200 para o MP não continuar reenviando
            return Ok(new { processed = false, error = "erro interno" });
        }
    }
}
