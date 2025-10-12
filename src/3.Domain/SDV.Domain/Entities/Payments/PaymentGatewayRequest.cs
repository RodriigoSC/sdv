using System;
using SDV.Domain.Entities.Orders;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Enums.Payments;

namespace SDV.Domain.Entities.Payments;

public class PaymentGatewayRequest
{

    public PaymentProvider PaymentProvider { get; set; }
    
    /// <summary>
    /// Identificador único da subscrição/pedido (reference externa)
    /// </summary>
    public string ExternalReference { get; set; } = string.Empty;

    /// <summary>
    /// Valor do pagamento em reais
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Nome do cliente
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Email do cliente
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do produto/serviço
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL para retorno após sucesso
    /// </summary>
    public string? ReturnSuccessUrl { get; set; }

    /// <summary>
    /// URL para retorno após falha
    /// </summary>
    public string? ReturnFailureUrl { get; set; }

    /// <summary>
    /// URL para retorno após pagamento pendente
    /// </summary>
    public string? ReturnPendingUrl { get; set; }

    /// <summary>
    /// URL para receber webhooks
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Secret do webhook para validação
    /// </summary>
    public string? WebhookSecret { get; set; }

    public PaymentGatewayRequest() { }

    public PaymentGatewayRequest(Order order, Plan plan, string returnSuccessUrl, 
        string returnFailureUrl, string returnPendingUrl, string webhookUrl, string webhookSecret)
    {
        ExternalReference = order.Id.ToString();
        Amount = plan.Price;
        CustomerName = order.Client?.Name ?? "Cliente";
        CustomerEmail = order.Client?.Email.Address ?? "";
        Description = plan.Description;
        ReturnSuccessUrl = returnSuccessUrl;
        ReturnFailureUrl = returnFailureUrl;
        ReturnPendingUrl = returnPendingUrl;
        WebhookUrl = webhookUrl;
        WebhookSecret = webhookSecret;
    }

}
