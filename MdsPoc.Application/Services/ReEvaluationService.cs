using MdsPoc.Application.Dtos;
using MdsPoc.Application.Interfaces;
using MdsPoc.Domain.Entities;

namespace MdsPoc.Application.Services
{
    /*
        ReEvaluationService voert de herevaluatie uit.

        Deze service bevat alle backendlogica:
        - temperatuur berekenen;
        - bepalen of herevaluatie nodig is;
        - feedback verwerken in de bestaande beslissing;
        - bestaande DecisionEvaluationService opnieuw aanroepen;
        - oude en nieuwe beslissing vergelijken;
        - NIAA toepassen op niet-geselecteerde alternatieven.

        De frontend bepaalt dus niet of herevaluatie nodig is.
    */
    public class ReEvaluationService : IReEvaluationService
    {
        private readonly IDecisionEvaluationService _decisionEvaluationService;
        private readonly ITemperatureService _temperatureService;
        private readonly CatalogService _catalogService;

        public ReEvaluationService(
            IDecisionEvaluationService decisionEvaluationService,
            ITemperatureService temperatureService,
            CatalogService catalogService)
        {
            _decisionEvaluationService = decisionEvaluationService;
            _temperatureService = temperatureService;
            _catalogService = catalogService;
        }

        public ReEvaluationResponse ReEvaluate(ReEvaluationRequest request)
        {
            var decisionRecord = request.DecisionRecord;
            var feedbackSignal = request.FeedbackSignal;

            var temperature = _temperatureService.CalculateTemperature(
                feedbackSignal,
                decisionRecord.Weights);

            if (!temperature.ShouldReEvaluate)
            {
                return new ReEvaluationResponse
                {
                    ReEvaluationTriggered = false,
                    Temperature = temperature,
                    PreviousSelectedAlternative = decisionRecord.SelectedAlternative,
                    NewSelectedAlternative = decisionRecord.SelectedAlternative,
                    DecisionChanged = false,
                    NewEvaluation = null,
                    NonSelectedBetterAlternatives = new List<AlternativeAwarenessResult>(),
                    Explanation =
                        "Geen herevaluatie uitgevoerd. De automatisch berekende temperatuur bleef onder de triggerwaarde."
                };
            }

            /*
                1. Maak een nieuwe evaluatierequest op basis van de bestaande beslissing.
                2. Pas de externe wijziging toe.
                3. Voer de normale MDS-berekening opnieuw uit.
            */
            var updatedRequest = CreateEvaluateRequestFromDecisionRecord(decisionRecord);

            ApplyFeedbackToRequest(updatedRequest, feedbackSignal);

            var newEvaluation = _decisionEvaluationService.Evaluate(updatedRequest);

            var previousAlternative = decisionRecord.SelectedAlternative;
            var newAlternative = newEvaluation.SelectedAlternative;

            /*
                NIAA:
                Controleer na de herevaluatie of niet-geselecteerde catalogusalternatieven
                met dezelfde criteria en gewichten nu beter scoren dan de nieuwe winnaar.

                Deze alternatieven beïnvloeden de beslissing niet automatisch.
                Ze worden alleen teruggegeven als awareness-signaal.
            */
            var nonSelectedBetterAlternatives =
                CalculateNonSelectedBetterAlternativesAfterReEvaluation(
                    decisionRecord,
                    feedbackSignal,
                    newEvaluation);

            newEvaluation.NonSelectedBetterAlternatives = nonSelectedBetterAlternatives;

            return new ReEvaluationResponse
            {
                ReEvaluationTriggered = true,
                Temperature = temperature,
                PreviousSelectedAlternative = previousAlternative,
                NewSelectedAlternative = newAlternative,
                DecisionChanged = previousAlternative != newAlternative,
                NewEvaluation = newEvaluation,
                NonSelectedBetterAlternatives = nonSelectedBetterAlternatives,
                Explanation =
                    "Herevaluatie uitgevoerd. De automatisch berekende temperatuur overschreed de triggerwaarde."
            };
        }

        private static EvaluateDecisionRequest CreateEvaluateRequestFromDecisionRecord(
            DecisionRecord decisionRecord)
        {
            return new EvaluateDecisionRequest
            {
                Context = decisionRecord.Context,

                Alternatives = decisionRecord.Alternatives
                    .Select(a => new AlternativeOption
                    {
                        Name = a.Name,
                        Type = a.Type
                    })
                    .ToList(),

                Criteria = decisionRecord.Criteria
                    .Select(c => new Criterion
                    {
                        Name = c.Name,
                        Category = c.Category
                    })
                    .ToList(),

                Weights = decisionRecord.Weights
                    .Select(w => new CriterionWeight
                    {
                        CriterionName = w.CriterionName,
                        Value = w.Value
                    })
                    .ToList(),

                Scores = decisionRecord.Scores
                    .Select(s => new CriterionScore
                    {
                        AlternativeName = s.AlternativeName,
                        CriterionName = s.CriterionName,
                        Score = s.Score,
                        Uncertainty = s.Uncertainty
                    })
                    .ToList()
            };
        }

        private List<AlternativeAwarenessResult> CalculateNonSelectedBetterAlternativesAfterReEvaluation(
            DecisionRecord decisionRecord,
            FeedbackSignal feedbackSignal,
            EvaluateDecisionResponse newEvaluation)
        {
            var selectedAlternativeScore = newEvaluation.Results
                .FirstOrDefault(result => result.AlternativeName == newEvaluation.SelectedAlternative)
                ?.FinalScore ?? 0;

            var selectedAlternativeNames = decisionRecord.Alternatives
                .Select(alternative => alternative.Name)
                .ToHashSet();

            var allCatalogAlternatives = _catalogService.GetAlternatives();

            var nonSelectedAlternatives = allCatalogAlternatives
                .Where(alternative => !selectedAlternativeNames.Contains(alternative.Name))
                .ToList();

            if (!nonSelectedAlternatives.Any())
                return new List<AlternativeAwarenessResult>();

            var awarenessRequest = CreateEvaluateRequestForNonSelectedAlternatives(
                decisionRecord,
                nonSelectedAlternatives);

            /*
                Belangrijk:
                Dezelfde externe wijziging wordt ook toegepast op de awareness-set.

                Daardoor kan bijvoorbeeld een niet-geselecteerd alternatief zoals Auth0
                alsnog worden beïnvloed door een kostenwijziging, performancewijziging
                of uncertaintywijziging.
            */
            ApplyFeedbackToRequest(awarenessRequest, feedbackSignal);

            var awarenessEvaluation = _decisionEvaluationService.Evaluate(awarenessRequest);

            if (awarenessEvaluation.ValidationErrors.Any())
                return new List<AlternativeAwarenessResult>();

            return awarenessEvaluation.Results
                .Where(result => result.FinalScore > selectedAlternativeScore)
                .Select(result =>
                {
                    var alternative = nonSelectedAlternatives
                        .First(a => a.Name == result.AlternativeName);

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

        private static EvaluateDecisionRequest CreateEvaluateRequestForNonSelectedAlternatives(
            DecisionRecord decisionRecord,
            List<AlternativeProfile> nonSelectedAlternatives)
        {
            var request = new EvaluateDecisionRequest
            {
                Context = decisionRecord.Context,

                Alternatives = nonSelectedAlternatives
                    .Select(alternative => new AlternativeOption
                    {
                        Name = alternative.Name,
                        Type = alternative.Type
                    })
                    .ToList(),

                Criteria = decisionRecord.Criteria
                    .Select(criterion => new Criterion
                    {
                        Name = criterion.Name,
                        Category = criterion.Category
                    })
                    .ToList(),

                Weights = decisionRecord.Weights
                    .Select(weight => new CriterionWeight
                    {
                        CriterionName = weight.CriterionName,
                        Value = weight.Value
                    })
                    .ToList()
            };

            foreach (var alternative in nonSelectedAlternatives)
            {
                foreach (var criterion in decisionRecord.Criteria)
                {
                    var baselineScore = alternative.BaselineScores
                        .FirstOrDefault(score => score.CriterionName == criterion.Name);

                    if (baselineScore is null)
                        continue;

                    request.Scores.Add(new CriterionScore
                    {
                        AlternativeName = alternative.Name,
                        CriterionName = baselineScore.CriterionName,
                        Score = baselineScore.Score,
                        Uncertainty = baselineScore.Uncertainty
                    });
                }
            }

            return request;
        }

        private static void ApplyFeedbackToRequest(
            EvaluateDecisionRequest request,
            FeedbackSignal feedbackSignal)
        {
            var score = request.Scores.FirstOrDefault(s =>
                s.AlternativeName == feedbackSignal.AffectedAlternative &&
                s.CriterionName == feedbackSignal.AffectedCriterion);

            if (score is null)
            {
                return;
            }

            switch (feedbackSignal.Type)
            {
                case "COST_INCREASE":
                    /*
                        Kostenstijging verlaagt de score op het kostencriterium.
                    */
                    score.Score = Math.Max(0, score.Score * (1 - feedbackSignal.ChangeValue));
                    break;

                case "PERFORMANCE_DROP":
                    /*
                        Performanceverslechtering verlaagt de performance-score.
                    */
                    score.Score = Math.Max(0, score.Score * (1 - feedbackSignal.ChangeValue));
                    break;

                case "MAINTENANCE_RISK_INCREASE":
                    /*
                        Onderhoudsrisico verlaagt de maintenance-score.
                    */
                    score.Score = Math.Max(0, score.Score * (1 - feedbackSignal.ChangeValue));
                    break;

                case "UNCERTAINTY_INCREASE":
                    /*
                        Hogere onzekerheid verlaagt niet direct de score,
                        maar verhoogt de uncertainty.
                        De bestaande EvaluationService verwerkt dit daarna.
                    */
                    score.Uncertainty = Math.Min(1, score.Uncertainty + feedbackSignal.ChangeValue);
                    break;

                default:
                    /*
                        Onbekende signalen veranderen de input niet.
                        De herevaluatie kan dan alsnog draaien, maar met dezelfde waarden.
                    */
                    break;
            }
        }
    }
}