using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;
using SDV.Infra.Gateway.Factories;

namespace SDV.Infra.Data.Service.Payments;

public class PaymentService : IPaymentService
{
    private readonly GatewayProviderFactory _gatewayProviderFactory;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(GatewayProviderFactory gatewayProviderFactory, IPaymentRepository paymentRepository)
    {
        _gatewayProviderFactory = gatewayProviderFactory;
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<string>> GeneratePaymentCheckoutAsync(PaymentGatewayRequest request)
    {
        try
        {
            // A Factory nos dá a implementação correta do gateway (MercadoPago, Stripe, etc.)
            var gateway = _gatewayProviderFactory.GetProvider(request.PaymentProvider);
            var checkoutUrlResult = await gateway.CreatePaymentAsync(request);

            return checkoutUrlResult.IsSuccess
                ? Result<string>.Success(checkoutUrlResult.Value ?? string.Empty)
                : Result<string>.Failure(checkoutUrlResult.Error ?? "Erro desconhecido ao criar pagamento no gateway.");
        }
        catch (Exception ex)
        {
            // O ideal é logar a exceção detalhada aqui
            return Result<string>.Failure($"Erro interno ao tentar criar pagamento: {ex.Message}");
        }
    }

    // ✔️ LÓGICA IMPLEMENTADA: Processa um pagamento aprovado
    public async Task<Result<Payment>> ProcessPaymentApprovalAsync(string gatewayTransactionId)
    {
        // Precisamos saber qual gateway consultar. A melhor abordagem é ter um método de fallback.
        // O ideal é que o Controller do webhook passe o provedor.
        var paymentInfoResult = await GetPaymentInfoFromGatewayAsync(gatewayTransactionId, PaymentProvider.MercadoPago); // Exemplo, o provider deve vir do contexto
        if (!paymentInfoResult.IsSuccess)
        {
            return Result<Payment>.Failure(paymentInfoResult.Error ?? "Erro desconhecido ao obter informações do gateway.");
        }

        var (internalPaymentId, status) = paymentInfoResult.Value;
        if (status != PaymentStatus.Approved)
        {
            return Result<Payment>.Failure("O status do pagamento no gateway não é 'Aprovado'.");
        }
        
        var payment = await _paymentRepository.GetByIdAsync(internalPaymentId);
        if (payment == null)
        {
            return Result<Payment>.Failure("Pagamento não encontrado no sistema.");
        }

        // Idempotência: Se o pagamento não estiver pendente, ele já foi processado.
        if (payment.Status != PaymentStatus.Pending)
        {
            return Result<Payment>.Success(payment);
        }

        // Deleta para a entidade de domínio a lógica de mudança de estado
        payment.Approve(gatewayTransactionId);
        await _paymentRepository.UpdateAsync(payment);

        return Result<Payment>.Success(payment);
    }
    
    // ✔️ LÓGICA IMPLEMENTADA: Processa um pagamento que falhou
    public async Task<Result<bool>> ProcessPaymentFailureAsync(string gatewayTransactionId, string reason)
    {
        var paymentInfoResult = await GetPaymentInfoFromGatewayAsync(gatewayTransactionId, PaymentProvider.MercadoPago); // Exemplo
        if (!paymentInfoResult.IsSuccess)
        {
            return Result<bool>.Failure(paymentInfoResult.Error ?? "Erro desconhecido ao obter informações do gateway.");
        }

        var (internalPaymentId, _) = paymentInfoResult.Value;
        var payment = await _paymentRepository.GetByIdAsync(internalPaymentId);
        if (payment == null)
        {
            return Result<bool>.Failure("Pagamento não encontrado no sistema.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Result<bool>.Success(true);
        }

        payment.Fail(reason ?? "Motivo da falha não informado pelo gateway.");
        await _paymentRepository.UpdateAsync(payment);

        return Result<bool>.Success(true);
    }

    
    
    public async Task<Result<(Guid InternalPaymentId, PaymentStatus Status)>> GetPaymentInfoFromGatewayAsync(string gatewayTransactionId, PaymentProvider provider)
    {
        try
        {
            var gateway = _gatewayProviderFactory.GetProvider(provider);

            // Passo 1: Obter o status do pagamento
            var statusResult = await gateway.GetPaymentStatusAsync(gatewayTransactionId);
            if (!statusResult.IsSuccess)
            {
                // Se falhar ao obter o status, retorna o erro imediatamente
                return Result<(Guid, PaymentStatus)>.Failure(statusResult.Error ?? "Falha ao obter status do pagamento.");
            }

            // Passo 2: Obter a referência externa (nosso ID interno)
            var referenceResult = await gateway.GetPaymentExternalReferenceAsync(gatewayTransactionId);
            if (!referenceResult.IsSuccess)
            {
                // Se falhar ao obter a referência, retorna o erro
                return Result<(Guid, PaymentStatus)>.Failure(referenceResult.Error ?? "Falha ao obter referência externa do pagamento.");
            }

            // Passo 3: Converter a referência de string para Guid
            if (!Guid.TryParse(referenceResult.Value, out var internalPaymentId))
            {
                return Result<(Guid, PaymentStatus)>.Failure("A referência externa retornada pelo gateway não é um Guid válido.");
            }

            // Passo 4: Combinar os dois resultados em uma tupla e retornar com sucesso
            var paymentStatus = statusResult.Value;
            return Result<(Guid, PaymentStatus)>.Success((internalPaymentId, paymentStatus));
        }
        catch (Exception ex)
        {
            return Result<(Guid, PaymentStatus)>.Failure($"Erro ao consultar informações do gateway: {ex.Message}");
        }
    }
    
    #region Métodos de Leitura e Stubs (lógica simples ou a ser implementada)
    public async Task<Result<Payment>> GetPaymentByIdAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        return payment != null
            ? Result<Payment>.Success(payment)
            : Result<Payment>.Failure("Pagamento não encontrado.");
    }

    public async Task<Result<IEnumerable<Payment>>> GetPaymentsByClientAsync(Guid clientId)
    {
        var payments = await _paymentRepository.GetPaymentsByClientIdAsync(clientId);
        return Result<IEnumerable<Payment>>.Success(payments);
    }
    
    public IPaymentGateway GetPaymentGateway(PaymentProvider provider)
    {
        return _gatewayProviderFactory.GetProvider(provider);
    }

    public Task<Result<bool>> ValidatePaymentCallbackAsync(string paymentId, string secret)
    {
        // A validação de webhook real geralmente requer o payload completo da requisição e a assinatura do header.
        // Esta assinatura de método é insuficiente para uma implementação segura.
        throw new NotImplementedException("A validação de webhook precisa ser implementada no Controller com acesso ao request completo.");
    }
    
    // ⚠️ ATENÇÃO: Este método foi substituído pela versão que aceita o 'provider'.
    // Você deve remover esta assinatura da sua interface IPaymentService.
    public Task<Result<(Guid InternalPaymentId, PaymentStatus Status)>> GetPaymentInfoFromGatewayAsync(string gatewayTransactionId)
    {
        throw new NotSupportedException("É necessário especificar o provedor de pagamento para poder consultar o gateway correto.");
    }
    #endregion
}