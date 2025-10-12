using Microsoft.AspNetCore.Mvc;
using SDV.Api.Middlewares;
using SDV.Application.Dtos.Orders;
using SDV.Application.Interfaces;
using SDV.Application.Results;

namespace SDV.Api.Controllers
{
    /// <summary>
    /// Controller para gerenciar operações de pedidos/assinaturas
    /// Responsável por CRUD e gerenciamento de estado de pedidos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : BaseController
    {
        private readonly IOrderApplication _orderApplication;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderApplication orderApplication, ILogger<OrdersController> logger)
        {
            _orderApplication = orderApplication;
            _logger = logger;
        }

        #region Criação

        /// <summary>
        /// Cria um novo pedido
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(OperationResult<OrderDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderRequestDto request)
        {
            var response = await _orderApplication.CreateOrderAsync(request);
            int statusCode = response.OperationCode;

            if (response.IsSuccess && (response.Data == null))
                return NoContent();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtém detalhes de um pedido específico
        /// </summary>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(OperationResult<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderAsync(string orderId)
        {
            _logger.LogInformation("Obtendo pedido: {OrderId}", orderId);

            var response = await _orderApplication.GetOrderAsync(orderId);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Obtém o pedido ativo de um cliente
        /// </summary>
        [HttpGet("client/{clientId}/active")]
        [ProducesResponseType(typeof(OperationResult<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActiveOrderAsync(string clientId)
        {
            _logger.LogInformation("Obtendo pedido ativo do cliente: {ClientId}", clientId);

            var response = await _orderApplication.GetActiveOrderAsync(clientId);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Obtém histórico completo de pedidos de um cliente
        /// </summary>
        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(OperationResult<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetOrderHistoryAsync(string clientId)
        {
            _logger.LogInformation("Obtendo histórico de pedidos do cliente: {ClientId}", clientId);

            var response = await _orderApplication.GetOrderHistoryAsync(clientId);
            int statusCode = response.OperationCode;

            if (response.IsSuccess && (response.Data == null || !response.Data.Any()))
                return NoContent();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Verifica se um pedido está ativo e válido
        /// </summary>
        [HttpGet("{orderId}/is-active")]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> IsOrderActiveAsync(string orderId)
        {
            _logger.LogInformation("Verificando se pedido está ativo: {OrderId}", orderId);

            var response = await _orderApplication.IsOrderActiveAsync(orderId);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Cancelamento

        /// <summary>
        /// Cancela um pedido existente
        /// </summary>
        [HttpDelete("{orderId}")]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrderAsync(string orderId)
        {
            _logger.LogInformation("Cancelando pedido: {OrderId}", orderId);

            var response = await _orderApplication.CancelOrderAsync(orderId);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion
    }
}
