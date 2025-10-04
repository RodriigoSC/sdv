using System;
using Microsoft.AspNetCore.Mvc;
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
            // Lógica para iniciar o processo de assinatura
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetSubscription(string clientId)
        {
            // Lógica para obter a assinatura do cliente
        }

        [HttpPost("payment/callback")]
        public async Task<IActionResult> PaymentCallback([FromBody] object payload)
        {
            // Endpoint para o Mercado Pago notificar o status do pagamento
        }
    }
