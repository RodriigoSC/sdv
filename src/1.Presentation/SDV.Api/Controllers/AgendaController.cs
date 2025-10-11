using Microsoft.AspNetCore.Mvc;
using SDV.Api.Middlewares;
using SDV.Application.Dtos.Agendas;
using SDV.Application.Interfaces;
using SDV.Application.Results;

namespace SDV.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgendaController : BaseController
    {
        private readonly IAgendaApplication _agendaApplication;

        public AgendaController(IAgendaApplication agendaApplication)
        {
            _agendaApplication = agendaApplication;
        }

        #region Consultas

        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(OperationResult<IEnumerable<AgendaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAllByClient(string clientId)
        {
            var response = await _agendaApplication.GetAllAgendas(clientId);
            int statusCode = response.OperationCode;

            if (response.IsSuccess && (response.Data == null || !response.Data.Any()))
                return NoContent();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var response = await _agendaApplication.GetAgendaById(id);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Criação

        [HttpPost]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status406NotAcceptable)]
        public async Task<IActionResult> Create([FromBody] AgendaDto dto)
        {
            var response = await _agendaApplication.CreateAgenda(dto);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Atualizações

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] AgendaDto dto)
        {
            var response = await _agendaApplication.UpdateAgenda(id, dto);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        [HttpPut("{id}/title")]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTitle(string id, [FromBody] string newTitle)
        {
            var response = await _agendaApplication.UpdateAgendaTitle(id, newTitle);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        [HttpPut("{id}/configuration")]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateConfiguration(string id, [FromBody] AgendaConfigurationDto newConfig)
        {
            var response = await _agendaApplication.UpdateAgendaConfiguration(id, newConfig);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        [HttpPut("{id}/season")]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<AgendaDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSeason(string id, [FromBody] AgendaSeasonDto newSeason)
        {
            var response = await _agendaApplication.UpdateAgendaSeason(id, newSeason);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Exclusão

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _agendaApplication.DeleteAgenda(id);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Geração
        [HttpGet("{id}/file")]
        [ProducesResponseType(typeof(OperationResult<byte[]>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateFile(string id)
        {
            var response = await _agendaApplication.GenerateAgendaFile(id);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            if (!response.IsSuccess)
                return BadRequest(CreateResponseObjectFromOperationResult(statusCode, response));

            if (response.Data == null)
                return BadRequest(CreateResponseObjectFromOperationResult(statusCode, response));

            // Retorna arquivo CSV
            var fileName = $"Agenda_{id}.csv";
            return File(response.Data, "text/csv", fileName);
        }

        #endregion
    }
}
