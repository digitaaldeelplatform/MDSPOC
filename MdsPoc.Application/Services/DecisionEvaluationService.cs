using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Application.Dtos;
using MdsPoc.Application.Interfaces;
using MdsPoc.Application.Validators;
using MdsPoc.Domain.Entities;

namespace MdsPoc.Application.Services
{
    public class DecisionEvaluationService : IDecisionEvaluationService
    {
        public EvaluateDecisionResponse Evaluate(EvaluateDecisionRequest request)
        {
            var validationErrors = DecisionRequestValidator.Validate(request);

            if (validationErrors.Any())
            {
                return new EvaluateDecisionResponse
                {
                    ValidationErrors = validationErrors
                };
            }

            var results = new List<EvaluationResult>();

            foreach (var alternative in request.Alternatives)
            {
                double weightedScore = 0.0;
                double knownWeightSum = 0.0;
                var missingCriteria = new List<string>();

                foreach (var criterion in request.Criteria)
                {
                    var weight = request.Weights
                        .First(w => w.CriterionName == criterion.Name).Value;

                    var scoreEntry = request.Scores.FirstOrDefault(s =>
                        s.AlternativeName == alternative.Name &&
                        s.CriterionName == criterion.Name);

                    if (scoreEntry is null)
                    {
                        missingCriteria.Add(criterion.Name);
                        continue;
                    }

                    var adjustedScore = scoreEntry.Score * (1 - scoreEntry.Uncertainty);
                    weightedScore += weight * adjustedScore;
                    knownWeightSum += weight;
                }

                double finalScore;
                double coverage;
                double normalizedScore;
                double missingWeight;
                double correctionFactor;

                if (knownWeightSum == 0)
                {
                    finalScore = 0;
                    coverage = 0;
                    normalizedScore = 0;
                    missingWeight = 1;
                    correctionFactor = 0;
                }
                else
                {
                    normalizedScore = weightedScore / knownWeightSum;
                    coverage = knownWeightSum;
                    missingWeight = 1 - coverage;
                    correctionFactor = 1 - (missingWeight * missingWeight);
                    finalScore = normalizedScore * correctionFactor;
                }

                results.Add(new EvaluationResult
                {
                    AlternativeName = alternative.Name,
                    FinalScore = Math.Round(finalScore, 4),
                    Coverage = Math.Round(coverage, 4),
                    NormalizedScore = Math.Round(normalizedScore, 4),
                    MissingWeight = Math.Round(missingWeight, 4),
                    CorrectionFactor = Math.Round(correctionFactor, 4),
                    MissingCriteria = missingCriteria
                });
            }

            var bestScore = results.Max(r => r.FinalScore);
            var bestAlternatives = results
                .Where(r => Math.Abs(r.FinalScore - bestScore) < 0.0001)
                .ToList();

            string selectedAlternative;

            if (bestAlternatives.Count == 1)
            {
                selectedAlternative = bestAlternatives[0].AlternativeName;
            }
            else
            {
                selectedAlternative = BreakTie(bestAlternatives, request);
            }

            return new EvaluateDecisionResponse
            {
                SelectedAlternative = selectedAlternative,
                Results = results
            };
        }

        private static string BreakTie(
            List<EvaluationResult> tiedResults,
            EvaluateDecisionRequest request)
        {
            var highestWeightCriterion = request.Weights
                .OrderByDescending(w => w.Value)
                .First()
                .CriterionName;

            double highestScore = double.MinValue;
            string selected = tiedResults.First().AlternativeName;

            foreach (var result in tiedResults)
            {
                var scoreEntry = request.Scores.FirstOrDefault(s =>
                    s.AlternativeName == result.AlternativeName &&
                    s.CriterionName == highestWeightCriterion);

                var score = scoreEntry?.Score ?? 0.0;

                if (score > highestScore)
                {
                    highestScore = score;
                    selected = result.AlternativeName;
                }
            }

            return selected;
        }
    }
}