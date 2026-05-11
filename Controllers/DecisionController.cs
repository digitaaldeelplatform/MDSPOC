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
        // Service die de daadwerkelijke MDS-evaluatie uitvoert.
        private readonly IDecisionEvaluationService _decisionEvaluationService;
        
        // Service die de vaste PoC-catalogus met alternatieven
        // en criteria beschikbaar maakt.
        private readonly CatalogService _catalogService;

        // Constructor injecteert de benodigde services.
        public DecisionController(
            IDecisionEvaluationService decisionEvaluationService,
            CatalogService catalogService)
        {
            _decisionEvaluationService = decisionEvaluationService;
            _catalogService = catalogService;
        }

        // Endpoint om een beslissing te evalueren op basis van een volledig verzoek.
        [HttpPost("evaluate")]
        public IActionResult Evaluate([FromBody] EvaluateDecisionRequest request)
        {
            var response = _decisionEvaluationService.Evaluate(request);

            if (response.ValidationErrors.Any())
                return BadRequest(response);

            return Ok(response);
        }

        // Endpoint voor evaluatie op basis van de vaste catalogus.
        // De frontend hoeft dan alleen gekozen alternatieven,
        // criteria, gewichten en aannames mee te geven.
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

            // Zet de catalogusselectie om naar een gewone EvaluateDecisionRequest.
            // Vanaf hier gebruikt het systeem dezelfde evaluatielogica
            // als bij het handmatige endpoint.
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

            // Voeg de baseline scores van de geselecteerde alternatieven toe aan het evaluatieverzoek.
            foreach (var alternative in selectedAlternatives)
            {
                foreach (var baselineScore in alternative.BaselineScores)
                {
                    // Alleen scores toevoegen voor de geselecteerde criteria. De rest wordt genegeerd
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
            // Stuurt het samengestelde evaluatieverzoek naar de service die de MDS-evaluatie uitvoert.
            var response = _decisionEvaluationService.Evaluate(evaluateRequest);

            if (response.ValidationErrors.Any())
                return BadRequest(response);

            return Ok(response);
        }
    }
}