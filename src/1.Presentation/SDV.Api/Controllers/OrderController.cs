using Microsoft.AspNetCore.Mvc;
using SDV.Application.Dtos.Orders;
using SDV.Application.Interfaces;

namespace SDV.Api.Controllers
{
    /// <summary>
    /// Controller para gerenciar operações de pedidos/assinaturas
    /// Responsável por CRUD e gerenciamento de estado de pedidos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderApplication _orderApplication;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderApplication orderApplication, ILogger<OrdersController> logger)
        {
            _orderApplication = orderApplication;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo pedido
        /// </summary>
        /// <param name="request">Dados do cliente e plano</param>
        /// <returns>Detalhes do pedido criado</returns>
        /// <response code="201">Pedido criado com sucesso</response>
        /// <response code="400">Erro de validação</response>
        /// <response code="404">Cliente ou plano não encontrado</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> CreateOrderAsync(
            [FromBody] CreateOrderRequestDto request)
        {
            _logger.LogInformation("Criando novo pedido para Cliente: {ClientId}, Plano: {PlanId}", 
                request.ClientId, request.PlanId);

            var result = await _orderApplication.CreateOrderAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Falha ao criar pedido: {Message}", result.Message);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            _logger.LogInformation("Pedido criado com sucesso: {OrderId}", result.Data?.Id);
            return CreatedAtAction(nameof(GetOrderAsync), 
                new { orderId = result.Data!.Id }, 
                result.Data);
        }

        /// <summary>
        /// Obtém detalhes de um pedido específico
        /// </summary>
        /// <param name="orderId">ID do pedido</param>
        /// <returns>Detalhes do pedido</returns>
        /// <response code="200">Pedido encontrado</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Pedido não encontrado</response>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderAsync(string orderId)
        {
            _logger.LogInformation("Obtendo pedido: {OrderId}", orderId);

            var result = await _orderApplication.GetOrderAsync(orderId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Pedido não encontrado: {OrderId}", orderId);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Obtém o pedido ativo de um cliente
        /// Retorna o pedido com status "Active" se existir
        /// </summary>
        /// <param name="clientId">ID do cliente</param>
        /// <returns>Detalhes do pedido ativo</returns>
        /// <response code="200">Pedido ativo encontrado</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Nenhum pedido ativo encontrado</response>
        [HttpGet("client/{clientId}/active")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetActiveOrderAsync(string clientId)
        {
            _logger.LogInformation("Obtendo pedido ativo do cliente: {ClientId}", clientId);

            var result = await _orderApplication.GetActiveOrderAsync(clientId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Pedido ativo não encontrado para cliente: {ClientId}", clientId);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Obtém histórico completo de pedidos de um cliente
        /// </summary>
        /// <param name="clientId">ID do cliente</param>
        /// <returns>Lista de todos os pedidos do cliente</returns>
        /// <response code="200">Histórico encontrado</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Nenhum pedido encontrado</response>
        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrderHistoryAsync(string clientId)
        {
            _logger.LogInformation("Obtendo histórico de pedidos do cliente: {ClientId}", clientId);

            var result = await _orderApplication.GetOrderHistoryAsync(clientId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Histórico não encontrado para cliente: {ClientId}", clientId);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Verifica se um pedido está ativo e válido
        /// </summary>
        /// <param name="orderId">ID do pedido</param>
        /// <returns>Status de atividade do pedido</returns>
        /// <response code="200">Verificação realizada com sucesso</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Pedido não encontrado</response>
        [HttpGet("{orderId}/is-active")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> IsOrderActiveAsync(string orderId)
        {
            _logger.LogInformation("Verificando se pedido está ativo: {OrderId}", orderId);

            var result = await _orderApplication.IsOrderActiveAsync(orderId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao verificar atividade do pedido: {OrderId}", orderId);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            return Ok(new { isActive = result.Data, message = result.Message });
        }

        /// <summary>
        /// Cancela um pedido
        /// </summary>
        /// <param name="orderId">ID do pedido a cancelar</param>
        /// <returns>Status da operação</returns>
        /// <response code="200">Pedido cancelado com sucesso</response>
        /// <response code="400">Erro de validação</response>
        /// <response code="404">Pedido não encontrado</response>
        [HttpDelete("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrderAsync(string orderId)
        {
            _logger.LogInformation("Cancelando pedido: {OrderId}", orderId);

            var result = await _orderApplication.CancelOrderAsync(orderId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Falha ao cancelar pedido: {OrderId}, Motivo: {Message}", 
                    orderId, result.Message);
                return StatusCode(result.OperationCode, new { message = result.Message });
            }

            _logger.LogInformation("Pedido cancelado com sucesso: {OrderId}", orderId);
            return Ok(new { message = "Pedido cancelado com sucesso" });
        }
    }
}


