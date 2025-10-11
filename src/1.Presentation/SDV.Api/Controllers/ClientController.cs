using Microsoft.AspNetCore.Mvc;
using SDV.Api.Middlewares;
using SDV.Application.Dtos.Clients;
using SDV.Application.Interfaces;
using SDV.Application.Results;

namespace SDV.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : BaseController
    {
        private readonly IClientApplication _clientApplication;

        public ClientController(IClientApplication clientApplication)
        {
            _clientApplication = clientApplication;
        }

        #region Queries

        /// <summary>
        /// Returns all clients.
        /// </summary>
        /// <returns>List of clients.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(OperationResult<IEnumerable<ClientDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _clientApplication.GetAllClients();
            int statusCode = response.OperationCode;

            if (response.IsSuccess && (response.Data == null || !response.Data.Any()))
                return NoContent();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Returns all active clients.
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(OperationResult<IEnumerable<ClientDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetActive()
        {
            var response = await _clientApplication.GetActiveClients();
            int statusCode = response.OperationCode;

            if (response.IsSuccess && (response.Data == null || !response.Data.Any()))
                return NoContent();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Returns a specific client by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOne(string id)
        {
            var response = await _clientApplication.GetOneClient(id);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Returns a specific client by email.
        /// </summary>
        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var response = await _clientApplication.FindClientByEmail(email);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Creation

        /// <summary>
        /// Creates a new client.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status406NotAcceptable)]
        public async Task<IActionResult> Create([FromBody] ClientDto clientDto)
        {
            var response = await _clientApplication.CreateClient(clientDto);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Updates

        /// <summary>
        /// Updates a client by ID.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status406NotAcceptable)]
        public async Task<IActionResult> Update(string id, [FromBody] ClientDto clientDto)
        {
            var response = await _clientApplication.UpdateClient(id, clientDto);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Updates a client's email.
        /// </summary>
        [HttpPut("{id}/email")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEmail(string id, [FromBody] string newEmail)
        {
            var response = await _clientApplication.UpdateClientEmail(id, newEmail);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Changes a client's password.
        /// </summary>
        [HttpPut("{id}/password")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] string newPassword)
        {
            var response = await _clientApplication.ChangeClientPassword(id, newPassword);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Resets a client's password with a temporary password.
        /// </summary>
        [HttpPut("{id}/password/reset")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] string tempPassword)
        {
            var response = await _clientApplication.ResetClientPassword(id, tempPassword);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Activation / Deactivation

        /// <summary>
        /// Activates a client.
        /// </summary>
        [HttpPut("{id}/activate")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Activate(string id)
        {
            var response = await _clientApplication.ActivateClient(id);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Deactivates a client.
        /// </summary>
        [HttpPut("{id}/deactivate")]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deactivate(string id)
        {
            var response = await _clientApplication.DeactivateClient(id);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Email Verification

        /// <summary>
        /// Generates an email verification token for the client.
        /// </summary>
        [HttpPost("{id}/email/token")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateEmailToken(string id)
        {
            var response = await _clientApplication.GenerateEmailVerificationToken(id);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        /// <summary>
        /// Verifies a client's email using the sent token.
        /// </summary>
        [HttpPost("{id}/email/verify")]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<ClientDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail(string id, [FromBody] string token)
        {
            var response = await _clientApplication.VerifyClientEmail(id, token);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion
    }
}