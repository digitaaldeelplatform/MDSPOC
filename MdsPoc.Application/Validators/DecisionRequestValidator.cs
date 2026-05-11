using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Application.Dtos;

// Controleert of de binnengekomen request geldig is
// voordat de evaluatie start.
//
// Deze validator controleert:
// - of voldoende alternatieven aanwezig zijn;
// - of minimaal één criterium bestaat;
// - of elk criterium een gewicht heeft;
// - of alle gewichten samen optellen tot 1.0.
//
// Bij fouten wordt de evaluatie gestopt
// en worden validatiemeldingen teruggegeven aan de API.

namespace MdsPoc.Application.Validators
{
    public static class DecisionRequestValidator
    {
        public static List<string> Validate(EvaluateDecisionRequest request)
        {
            var errors = new List<string>();

            if (request.Alternatives.Count < 2)
                errors.Add("At least two alternatives are required.");

            if (request.Criteria.Count < 1)
                errors.Add("At least one criterion is required.");

            foreach (var criterion in request.Criteria)
            {
                var hasWeight = request.Weights.Any(w => w.CriterionName == criterion.Name);
                if (!hasWeight)
                    errors.Add($"Missing weight for criterion '{criterion.Name}'.");
            }

            var weightSum = request.Weights.Sum(w => w.Value);
            if (Math.Abs(weightSum - 1.0) > 0.0001)
                errors.Add("The sum of all criterion weights must be 1.0.");

            return errors;
        }
    }
}
