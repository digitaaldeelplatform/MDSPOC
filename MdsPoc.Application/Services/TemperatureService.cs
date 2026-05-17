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
        TemperatureService berekent automatisch of een feedbacksignaal
        sterk genoeg is om herevaluatie te activeren.

        De frontend bepaalt dit dus niet.

        Kernregel:
        Hoe hoger het gewicht van een criterium,
        hoe lager de triggerdrempel wordt,
        en dus hoe sneller herevaluatie plaatsvindt.
    */
    public class TemperatureService : ITemperatureService
    {
        private const double BaseThreshold = 0.25;

        public TemperatureResult CalculateTemperature(
            FeedbackSignal feedbackSignal,
            List<CriterionWeight> weights)
        {
            var weight = weights
                .FirstOrDefault(w => w.CriterionName == feedbackSignal.AffectedCriterion);

            if (weight is null)
            {
                return new TemperatureResult
                {
                    CriterionName = feedbackSignal.AffectedCriterion,
                    CriterionWeight = 0,
                    ChangeValue = feedbackSignal.ChangeValue,
                    TriggerThreshold = 1,
                    Temperature = 0,
                    ShouldReEvaluate = false
                };
            }

            /*
                Sensitivity bepaalt hoe gevoelig het systeem reageert.

                Bij hoog gewicht:
                weight = 0.40
                sensitivity = 0.60
                threshold = 0.25 * 0.60 = 0.15

                Bij laag gewicht:
                weight = 0.10
                sensitivity = 0.90
                threshold = 0.25 * 0.90 = 0.225

                Dus:
                belangrijk criterium = lagere drempel = sneller herevaluatie.
            */
            var sensitivity = 1 - weight.Value;

            var triggerThreshold = BaseThreshold * sensitivity;

            /*
                Veiligheidsgrens:
                voorkomt dat threshold 0 wordt bij extreem hoge gewichten.
            */
            triggerThreshold = Math.Max(0.05, triggerThreshold);

            var temperature = feedbackSignal.ChangeValue / triggerThreshold;

            return new TemperatureResult
            {
                CriterionName = feedbackSignal.AffectedCriterion,
                CriterionWeight = Math.Round(weight.Value, 4),
                ChangeValue = Math.Round(feedbackSignal.ChangeValue, 4),
                TriggerThreshold = Math.Round(triggerThreshold, 4),
                Temperature = Math.Round(temperature, 4),
                ShouldReEvaluate = temperature >= 1.0
            };
        }
    }
}