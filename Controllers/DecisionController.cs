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
            var evaluateRequest = CreateEvaluateRequestFromCatalog(
                request,
                selectedAlternatives,
                selectedCriteria);

            // Stuurt het samengestelde evaluatieverzoek naar de service die de MDS-evaluatie uitvoert.
            var response = _decisionEvaluationService.Evaluate(evaluateRequest);

            if (response.ValidationErrors.Any())
                return BadRequest(response);

            // NIAA:
            // Niet-geselecteerde alternatieven worden apart geëvalueerd.
            // Ze beïnvloeden de gekozen beslissing niet.
            // Ze worden alleen teruggegeven als awareness-signaal wanneer ze beter scoren.
            response.NonSelectedBetterAlternatives =
                CalculateNonSelectedBetterAlternatives(
                    request,
                    allAlternatives,
                    selectedAlternatives,
                    selectedCriteria,
                    response);

            return Ok(response);
        }

        private static EvaluateDecisionRequest CreateEvaluateRequestFromCatalog(
            EvaluateFromCatalogRequest request,
            List<AlternativeProfile> alternatives,
            List<CriterionProfile> criteria)
        {
            var evaluateRequest = new EvaluateDecisionRequest
            {
                Context = request.Context,

                Alternatives = alternatives.Select(alternative => new AlternativeOption
                {
                    Name = alternative.Name,
                    Type = alternative.Type
                }).ToList(),

                Criteria = criteria.Select(criterion => new Criterion
                {
                    Name = criterion.Name,
                    Category = criterion.Category
                }).ToList(),

                Weights = request.Weights,
                Assumptions = request.Assumptions
            };

            // Voeg baseline scores toe voor de geselecteerde alternatieven
            // en alleen voor de geselecteerde criteria.
            foreach (var alternative in alternatives)
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

            return evaluateRequest;
        }

        private List<AlternativeAwarenessResult> CalculateNonSelectedBetterAlternatives(
            EvaluateFromCatalogRequest originalRequest,
            List<AlternativeProfile> allAlternatives,
            List<AlternativeProfile> selectedAlternatives,
            List<CriterionProfile> selectedCriteria,
            EvaluateDecisionResponse selectedResponse)
        {
            var selectedAlternativeScore = selectedResponse.Results
                .FirstOrDefault(result => result.AlternativeName == selectedResponse.SelectedAlternative)
                ?.FinalScore ?? 0;

            var selectedIds = selectedAlternatives
                .Select(alternative => alternative.Id)
                .ToHashSet();

            var nonSelectedAlternatives = allAlternatives
                .Where(alternative => !selectedIds.Contains(alternative.Id))
                .ToList();

            if (!nonSelectedAlternatives.Any())
                return new List<AlternativeAwarenessResult>();

            var awarenessRequest = CreateEvaluateRequestFromCatalog(
                originalRequest,
                nonSelectedAlternatives,
                selectedCriteria);

            var awarenessResponse = _decisionEvaluationService.Evaluate(awarenessRequest);

            if (awarenessResponse.ValidationErrors.Any())
                return new List<AlternativeAwarenessResult>();

            return awarenessResponse.Results
                .Where(result => result.FinalScore > selectedAlternativeScore)
                .Select(result =>
                {
                    var alternative = nonSelectedAlternatives
                        .First(alternative => alternative.Name == result.AlternativeName);

                    return new AlternativeAwarenessResult
                    {
                        AlternativeName = result.AlternativeName,
                        AlternativeType = alternative.Type,
                        FinalScore = result.FinalScore,
                        DifferenceWithSelected = Math.Round(
                            result.FinalScore - selectedAlternativeScore,
                            4)
                    };
                })
                .OrderByDescending(result => result.FinalScore)
                .ToList();
        }
    }
}