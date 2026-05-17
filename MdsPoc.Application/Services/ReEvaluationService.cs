using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        - oude en nieuwe beslissing vergelijken.

        De frontend bepaalt dus niet of herevaluatie nodig is.
    */
    public class ReEvaluationService : IReEvaluationService
    {
        private readonly IDecisionEvaluationService _decisionEvaluationService;
        private readonly ITemperatureService _temperatureService;

        public ReEvaluationService(
            IDecisionEvaluationService decisionEvaluationService,
            ITemperatureService temperatureService)
        {
            _decisionEvaluationService = decisionEvaluationService;
            _temperatureService = temperatureService;
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
                    Explanation =
                        "Geen herevaluatie uitgevoerd. De automatisch berekende temperatuur bleef onder de triggerwaarde."
                };
            }

            var updatedRequest = CreateEvaluateRequestFromDecisionRecord(decisionRecord);

            ApplyFeedbackToRequest(updatedRequest, feedbackSignal);

            var newEvaluation = _decisionEvaluationService.Evaluate(updatedRequest);

            var previousAlternative = decisionRecord.SelectedAlternative;
            var newAlternative = newEvaluation.SelectedAlternative;

            return new ReEvaluationResponse
            {
                ReEvaluationTriggered = true,
                Temperature = temperature,
                PreviousSelectedAlternative = previousAlternative,
                NewSelectedAlternative = newAlternative,
                DecisionChanged = previousAlternative != newAlternative,
                NewEvaluation = newEvaluation,
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