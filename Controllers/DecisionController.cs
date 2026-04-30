using Microsoft.AspNetCore.Mvc;
using MdsPoc.Application.Dtos;
using MdsPoc.Application.Interfaces;

namespace MdsPoc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DecisionController : ControllerBase
{
    private readonly IDecisionEvaluationService _decisionEvaluationService;

    public DecisionController(IDecisionEvaluationService decisionEvaluationService)
    {
        _decisionEvaluationService = decisionEvaluationService;
    }

    [HttpPost("evaluate")]
    public IActionResult Evaluate([FromBody] EvaluateDecisionRequest request)
    {
        var response = _decisionEvaluationService.Evaluate(request);

        if (response.ValidationErrors.Any())
            return BadRequest(response);

        return Ok(response);
    }
}