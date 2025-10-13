using Microsoft.AspNetCore.Mvc;
using SDV.Api.Middlewares;
using SDV.Application.Dtos.Payments;
using SDV.Application.Interfaces;
using SDV.Application.Results;

namespace SDV.Api.Controllers
{
    /// <summary>
    /// Controller para gerenciar operações de pagamento
    /// Responsável por endpoints de checkout e processamento de webhooks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentApplication _paymentApplication;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentApplication paymentApplication, ILogger<PaymentsController> logger)
        {
            _paymentApplication = paymentApplication;
            _logger = logger;
        }

        #region Checkout

        /// <summary>
        /// Inicia um checkout de pagamento
        /// </summary>
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(OperationResult<PaymentCheckoutResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitiateCheckoutAsync([FromBody] InitiatePaymentRequestDto request)
        {
            _logger.LogInformation("Iniciando checkout para Cliente: {ClientId}, Plano: {PlanId}", request.ClientId, request.PlanId);

            var response = await _paymentApplication.InitiatePaymentCheckoutAsync(request);
            int statusCode = response.OperationCode;

            if (response.IsSuccess && response.Data == null)
                return NoContent();            

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Webhooks

        /// <summary>
        /// Processa webhook de aprovação de pagamento
        /// </summary>
        [HttpPost("webhook/approval")]
        [ProducesResponseType(typeof(OperationResult<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessPaymentApprovalAsync([FromBody] PaymentWebhookCallbackDto callback)
        {
            _logger.LogInformation("Processando aprovação de pagamento: {PaymentId}", callback.PaymentId);

            var response = await _paymentApplication.ProcessPaymentApprovalCallbackAsync(callback);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Processa webhook de falha de pagamento
        /// </summary>
        [HttpPost("webhook/failure")]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessPaymentFailureAsync([FromBody] PaymentWebhookCallbackDto callback)
        {
            _logger.LogWarning("Processando falha de pagamento: {PaymentId}, Motivo: {Status}", callback.PaymentId, callback.Status);

            var response = await _paymentApplication.ProcessPaymentFailureCallbackAsync(callback);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtém detalhes de um pagamento específico
        /// </summary>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(OperationResult<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPaymentAsync(string paymentId)
        {
            _logger.LogInformation("Obtendo detalhes do pagamento: {PaymentId}", paymentId);

            var response = await _paymentApplication.GetPaymentAsync(paymentId);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Obtém histórico de pagamentos de um cliente
        /// </summary>
        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(OperationResult<IEnumerable<PaymentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetPaymentHistoryAsync(string clientId)
        {
            _logger.LogInformation("Obtendo histórico de pagamentos do cliente: {ClientId}", clientId);

            var response = await _paymentApplication.GetPaymentHistoryAsync(clientId);
            int statusCode = response.OperationCode;

            if (response.IsSuccess && (response.Data == null || !response.Data.Any()))
                return NoContent();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion
    }
}
