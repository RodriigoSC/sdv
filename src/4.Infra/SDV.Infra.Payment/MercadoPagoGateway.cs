using MercadoPago.Client.Payment;
using MercadoPago.Config;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Entities.Subscriptions;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;

namespace SDV.Infra.Payment;

public class MercadoPagoGateway : IPaymentGateway
{
    private readonly string _accessToken;

        public MercadoPagoGateway(string accessToken)
        {
            _accessToken = accessToken;
            MercadoPagoConfig.AccessToken = _accessToken;
        }

        public async Task<Result<string>> CreatePaymentAsync(Subscription subscription, Plan plan)
        {
            try
            {
                var request = new PaymentCreateRequest
                {
                    TransactionAmount = plan.Price,
                    Description = plan.Description,
                    ExternalReference = subscription.Id.ToString(),
                    PaymentMethodId = "pix", // Exemplo com Pix
                    Payer = new PaymentPayerRequest
                    {
                        // Adicionar informações do pagador se necessário
                    },
                };

                var client = new PaymentClient();
                Payment payment = await client.CreateAsync(request);

                return Result<string>.Success(payment.Id.ToString());
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Erro ao criar pagamento: {ex.Message}");
            }
        }

        public async Task<Result<PaymentStatus>> GetPaymentStatusAsync(string transactionId)
        {
            try
            {
                var client = new PaymentClient();
                Payment payment = await client.GetAsync(long.Parse(transactionId));

                return payment.Status switch
                {
                    "approved" => Result<PaymentStatus>.Success(PaymentStatus.Approved),
                    "refused" => Result<PaymentStatus>.Success(PaymentStatus.Refused),
                    _ => Result<PaymentStatus>.Success(PaymentStatus.Pending),
                };
            }
            catch (Exception ex)
            {
                return Result<PaymentStatus>.Failure($"Erro ao obter status do pagamento: {ex.Message}");
            }
        }
}
