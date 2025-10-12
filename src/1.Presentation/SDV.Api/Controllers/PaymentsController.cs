using Microsoft.AspNetCore.Mvc;
using SDV.Application.Dtos.Payments;
using SDV.Application.Interfaces;

namespace SDV.Api.Controllers
{
    /// <summary>
    /// Controller para gerenciar operações de pagamento
    /// Responsável por endpoints de checkout e processamento de webhooks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentApplication _paymentApplication;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentApplication paymentApplication, ILogger<PaymentsController> logger)
        {
            _paymentApplication = paymentApplication;
            _logger = logger;
        }

        /// <summary>
        /// Inicia um checkout de pagamento
        /// </summary>
        /// <param name="request">Dados do cliente e plano</param>
        /// <returns>URL de checkout e detalhes do pagamento</returns>
        /// <response code="201">Checkout criado com sucesso</response>
        /// <response code="400">Erro de validação</response>
        /// <response code="404">Cliente ou plano não encontrado</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(PaymentCheckoutResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentCheckoutResponseDto>> InitiateCheckoutAsync(
            [FromBody] InitiatePaymentRequestDto request)
        {
            _logger.LogInformation("Iniciando checkout para Cliente: {ClientId}, Plano: {PlanId}", 
                request.ClientId, request.PlanId);

            var result = await _paymentApplication.InitiatePaymentCheckoutAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Falha ao iniciar checkout: {Message}", result.Message);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            _logger.LogInformation("Checkout criado com sucesso: {OrderId}", result.Data?.OrderId);
            return CreatedAtAction(nameof(GetPaymentAsync), 
                new { paymentId = result.Data!.PaymentId }, 
                result.Data);
        }

        /// <summary>
        /// Processa webhook de aprovação de pagamento
        /// Chamado pelo provedor de pagamento após confirmação
        /// </summary>
        /// <param name="callback">Dados do callback com transactionId</param>
        /// <returns>Detalhes do pagamento aprovado</returns>
        /// <response code="200">Pagamento aprovado com sucesso</response>
        /// <response code="400">Erro de validação ou processamento</response>
        /// <response code="404">Pagamento não encontrado</response>
        [HttpPost("webhook/approval")]
        [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> ProcessPaymentApprovalAsync(
            [FromBody] PaymentWebhookCallbackDto callback)
        {
            _logger.LogInformation("Processando aprovação de pagamento: {PaymentId}", callback.PaymentId);

            var result = await _paymentApplication.ProcessPaymentApprovalCallbackAsync(callback);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Falha ao processar aprovação: {Message}", result.Message);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            _logger.LogInformation("Pagamento aprovado: {PaymentId}", result.Data?.Id);
            return Ok(result.Data);
        }

        /// <summary>
        /// Processa webhook de falha de pagamento
        /// Chamado pelo provedor de pagamento em caso de rejeição
        /// </summary>
        /// <param name="callback">Dados do callback com motivo da falha</param>
        /// <returns>Status da operação</returns>
        /// <response code="200">Falha registrada com sucesso</response>
        /// <response code="400">Erro de validação ou processamento</response>
        [HttpPost("webhook/failure")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessPaymentFailureAsync(
            [FromBody] PaymentWebhookCallbackDto callback)
        {
            _logger.LogWarning("Processando falha de pagamento: {PaymentId}, Motivo: {Status}", 
                callback.PaymentId, callback.Status);

            var result = await _paymentApplication.ProcessPaymentFailureCallbackAsync(callback);

            if (!result.IsSuccess)
            {
                _logger.LogError("Erro ao processar falha: {Message}", result.Message);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            _logger.LogInformation("Falha de pagamento registrada: {PaymentId}", callback.PaymentId);
            return Ok(new { message = "Falha de pagamento processada" });
        }

        /// <summary>
        /// Obtém detalhes de um pagamento específico
        /// </summary>
        /// <param name="paymentId">ID do pagamento</param>
        /// <returns>Detalhes do pagamento</returns>
        /// <response code="200">Pagamento encontrado</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Pagamento não encontrado</response>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPaymentAsync(string paymentId)
        {
            _logger.LogInformation("Obtendo detalhes do pagamento: {PaymentId}", paymentId);

            var result = await _paymentApplication.GetPaymentAsync(paymentId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Pagamento não encontrado: {PaymentId}", paymentId);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Obtém histórico de pagamentos de um cliente
        /// </summary>
        /// <param name="clientId">ID do cliente</param>
        /// <returns>Lista de pagamentos do cliente</returns>
        /// <response code="200">Histórico encontrado</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Nenhum pagamento encontrado</response>
        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentHistoryAsync(string clientId)
        {
            _logger.LogInformation("Obtendo histórico de pagamentos do cliente: {ClientId}", clientId);

            var result = await _paymentApplication.GetPaymentHistoryAsync(clientId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Histórico não encontrado para cliente: {ClientId}", clientId);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            return Ok(result.Data);
        }
    }
}


