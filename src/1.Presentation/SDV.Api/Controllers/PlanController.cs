using Microsoft.AspNetCore.Mvc;
using SDV.Api.Middlewares;
using SDV.Application.Dtos.Plans;
using SDV.Application.Interfaces;

namespace SDV.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlanController : BaseController
{
    private readonly IPlanApplication _planApplication;

    public PlanController(IPlanApplication planApplication)
    {
        _planApplication = planApplication;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _planApplication.GetAllPlans();
        int statusCode = response.OperationCode;

        if (response.IsSuccess && (response.Data == null || !response.Data.Any()))
            return NoContent();

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var response = await _planApplication.GetPlanById(id);
        int statusCode = response.OperationCode;

        if (!response.IsSuccess && statusCode == 404)
            return NotFound();

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlanDto dto)
    {
        var response = await _planApplication.CreatePlan(dto);
        int statusCode = response.OperationCode;

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] PlanDto dto)
    {
        var response = await _planApplication.UpdatePlan(id, dto);
        int statusCode = response.OperationCode;

        if (!response.IsSuccess && statusCode == 404)
            return NotFound();

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var response = await _planApplication.DeactivatePlan(id);
        int statusCode = response.OperationCode;

        return StatusCode(statusCode, CreateResponseObjectFromOperationResult(statusCode, response));
    }
}