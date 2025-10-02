using Microsoft.AspNetCore.Mvc;
using SDV.Api.Middlewares;
using SDV.Application.Dtos.Calendars;
using SDV.Application.Interfaces;
using SDV.Application.Results;

namespace SDV.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : BaseController
    {
        private readonly ICalendarApplication _calendarApplication;

        public CalendarController(ICalendarApplication calendarApplication)
        {
            _calendarApplication = calendarApplication;
        }

        #region Consultas

        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(OperationResult<IEnumerable<CalendarDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAllByClient(string clientId)
        {
            var response = await _calendarApplication.GetAllCalendars(clientId);
            int statusCode = response.OperationCode;

            if (response.IsSuccess && (response.Data == null || !response.Data.Any()))
                return NoContent();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OperationResult<CalendarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var response = await _calendarApplication.GetCalendarById(id);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Criação

        [HttpPost]
        [ProducesResponseType(typeof(OperationResult<CalendarDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(OperationResult<CalendarDto>), StatusCodes.Status406NotAcceptable)]
        public async Task<IActionResult> Create([FromBody] CalendarDto dto)
        {
            var response = await _calendarApplication.CreateCalendar(dto);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Atualizações

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(OperationResult<CalendarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<CalendarDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] CalendarDto dto)
        {
            var response = await _calendarApplication.UpdateCalendar(id, dto);
            int statusCode = response.OperationCode;

            if (!response.IsSuccess && statusCode == 404)
                return NotFound();

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion

        #region Exclusão

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _calendarApplication.DeleteCalendar(id);
            int statusCode = response.OperationCode;

            return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
        }

        #endregion
    }
}
