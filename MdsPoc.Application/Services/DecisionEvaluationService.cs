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
            // Controleert eerst of de request geldig is.
            // Bij fouten stopt de evaluatie direct.
            var validationErrors = DecisionRequestValidator.Validate(request);

            if (validationErrors.Any())
            {
                return new EvaluateDecisionResponse
                {
                    ValidationErrors = validationErrors
                };
            }

            // Hier worden alle evaluatieresultaten opgeslagen.
            var results = new List<EvaluationResult>();

            // Evaluatie start per alternatief (Build / Buy / Free).
            foreach (var alternative in request.Alternatives)
            {
                double weightedScore = 0.0;
                double knownWeightSum = 0.0;

                // Houdt criteria bij waarvoor geen score beschikbaar is.
                var missingCriteria = new List<string>();

                // Doorloopt alle criteria van de beslissing.
                foreach (var criterion in request.Criteria)
                {
                    // Haalt het gewicht van het huidige criterium op.
                    var weight = request.Weights
                        .First(w => w.CriterionName == criterion.Name).Value;

                    // Zoekt de score van dit alternatief op dit criterium.
                    var scoreEntry = request.Scores.FirstOrDefault(s =>
                        s.AlternativeName == alternative.Name &&
                        s.CriterionName == criterion.Name);

                    // Wanneer geen score bestaat,
                    // wordt het criterium als ontbrekend gemarkeerd.
                    if (scoreEntry is null)
                    {
                        missingCriteria.Add(criterion.Name);
                        continue;
                    }

                    // Past de score aan op basis van onzekerheid.
                    // Hogere onzekerheid verlaagt de effectieve score.
                    var adjustedScore = scoreEntry.Score * (1 - scoreEntry.Uncertainty);

                    // Weighted sum berekening.
                    weightedScore += weight * adjustedScore;

                    // Houdt bij hoeveel bekende gewichten beschikbaar zijn.
                    knownWeightSum += weight;
                }


                double finalScore;
                double coverage;
                double normalizedScore;
                double missingWeight;
                double correctionFactor;

                // Wanneer er geen bekende gewichten zijn, wordt de score 0.
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
                    // Normaliseert de score op basis van de bekende gewichten.
                    normalizedScore = weightedScore / knownWeightSum;
                    // Bereken de dekking van de bekende gewichten
                    // ten opzichte van het totaal.
                    coverage = knownWeightSum;
                    // Bereken het gewicht van de ontbrekende criteria.
                    missingWeight = 1 - coverage;
                    //  Past een correctiefactor toe
                    //  om de impact van ontbrekende criteria te verminderen.
                    correctionFactor = 1 - (missingWeight * missingWeight);
                    // Bereken de uiteindelijke score na correctie.
                    finalScore = normalizedScore * correctionFactor;
                }

                // Slaat het resultaat van dit alternatief op.
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

            // Bepaalt de hoogste score onder de alternatieven.
            var bestScore = results.Max(r => r.FinalScore);
            // Zoekt alle alternatieven die deze hoogste score hebben
            // (inclusief gelijke scores).
            var bestAlternatives = results
                .Where(r => Math.Abs(r.FinalScore - bestScore) < 0.0001)
                .ToList();

            string selectedAlternative;

            // Wanneer er maar één beste alternatief is, wordt deze direct geselecteerd.
            if (bestAlternatives.Count == 1)
            {
                selectedAlternative = bestAlternatives[0].AlternativeName;
            }
            else
            {
                // Bij gelijke scores wordt een tie-breaker toegepast.
                selectedAlternative = BreakTie(bestAlternatives, request);
            }

            // Geeft het geselecteerde alternatief en de evaluatieresultaten terug in de response.
            return new EvaluateDecisionResponse
            {
                SelectedAlternative = selectedAlternative,
                Results = results
            };
        }


        // Tie-breaker functie die het alternatief selecteert
        // met de hoogste score op het criterium met het hoogste gewicht.

        // Wat als er meerdere criteria met hetzelfde hoogste gewicht zijn?
        // In dat geval zou je kunnen overwegen om een secundaire tie-breaker toe te passen,
        // zoals het aantal bekende criteria of de totale onzekerheid.
        // Maar voor nu gaan we ervan uit dat er één criterium is met het hoogste gewicht.

        // Als alle alternatieven nog steeds gelijk zijn na deze tie-breaker,
        // overweeg ik 2 alternatieven als mee te geven in de response,
        // of een random keuze te maken.

        // Meerder diferentiaties tussen alternatieven kunnen ook worden overwogen, bijvoorbeeld kosten of onderhoudbaarheid, afhankelijk van de context van de beslissing.
        private static string BreakTie(
            List<EvaluationResult> tiedResults,
            EvaluateDecisionRequest request)
        {
            // Bepaalt welk criterium het hoogste gewicht heeft.
            var highestWeightCriterion = request.Weights
                .OrderByDescending(w => w.Value)
                .First()
                .CriterionName;

            double highestScore = double.MinValue;

            // Standaard wordt het eerste alternatief gekozen,
            // tenzij een ander alternatief beter scoort
            // op het zwaarst gewogen criterium.
            string selected = tiedResults.First().AlternativeName;

            // Vergelijkt alleen de alternatieven die gelijk geëindigd zijn.
            foreach (var result in tiedResults)
            {
                // Zoekt de score van dit alternatief op het criterium met het hoogste gewicht.
                var scoreEntry = request.Scores.FirstOrDefault(s =>
                    s.AlternativeName == result.AlternativeName &&
                    s.CriterionName == highestWeightCriterion);

                // Wanneer er geen score is voor dit criterium, wordt deze als 0 beschouwd.
                var score = scoreEntry?.Score ?? 0.0;

                // Het alternatief met de hoogste score
                // op het belangrijkste criterium wordt gekozen.
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