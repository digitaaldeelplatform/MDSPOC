using Microsoft.AspNetCore.Mvc;
using MdsPoc.Application.Dtos;
using MdsPoc.Application.Interfaces;
using MdsPoc.Application.Services;
using MdsPoc.Domain.Entities;

namespace MdsPoc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DecisionController : ControllerBase
    {
        private readonly IDecisionEvaluationService _decisionEvaluationService;
        private readonly CatalogService _catalogService;

        public DecisionController(
            IDecisionEvaluationService decisionEvaluationService,
            CatalogService catalogService)
        {
            _decisionEvaluationService = decisionEvaluationService;
            _catalogService = catalogService;
        }

        [HttpPost("evaluate")]
        public IActionResult Evaluate([FromBody] EvaluateDecisionRequest request)
        {
            var response = _decisionEvaluationService.Evaluate(request);

            if (response.ValidationErrors.Any())
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("evaluate-from-catalog")]
        public IActionResult EvaluateFromCatalog([FromBody] EvaluateFromCatalogRequest request)
        {
            var allAlternatives = _catalogService.GetAlternatives();
            var allCriteria = _catalogService.GetCriteria();

            var selectedAlternatives = allAlternatives
                .Where(alternative => request.SelectedAlternativeIds.Contains(alternative.Id))
                .ToList();

            var selectedCriteria = allCriteria
                .Where(criterion => request.SelectedCriterionNames.Contains(criterion.Name))
                .ToList();

            var evaluateRequest = new EvaluateDecisionRequest
            {
                Context = request.Context,
                Alternatives = selectedAlternatives.Select(alternative => new AlternativeOption
                {
                    Name = alternative.Name,
                    Type = alternative.Type
                }).ToList(),
                Criteria = selectedCriteria.Select(criterion => new Criterion
                {
                    Name = criterion.Name,
                    Category = criterion.Category
                }).ToList(),
                Weights = request.Weights,
                Assumptions = request.Assumptions
            };

            foreach (var alternative in selectedAlternatives)
            {
                foreach (var baselineScore in alternative.BaselineScores)
                {
                    if (!request.SelectedCriterionNames.Contains(baselineScore.CriterionName))
                        continue;

                    evaluateRequest.Scores.Add(new CriterionScore
                    {
                        AlternativeName = alternative.Name,
                        CriterionName = baselineScore.CriterionName,
                        Score = baselineScore.Score,
                        Uncertainty = baselineScore.Uncertainty
                    });
                }
            }

            var response = _decisionEvaluationService.Evaluate(evaluateRequest);

            if (response.ValidationErrors.Any())
                return BadRequest(response);

            return Ok(response);
        }
    }
}