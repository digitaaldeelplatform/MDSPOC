using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Application.Dtos
{
    /*
        ReEvaluationResponse geeft terug wat de backend heeft besloten.

        Deze response laat zien:
        - welke temperatuur is berekend;
        - of herevaluatie is uitgevoerd;
        - wat de oude beslissing was;
        - wat de nieuwe beslissing is;
        - of de beslissing veranderd is.
    */
    public class ReEvaluationResponse
    {
        public bool ReEvaluationTriggered { get; set; }

        public TemperatureResult Temperature { get; set; } = new();

        public string PreviousSelectedAlternative { get; set; } = string.Empty;

        public string NewSelectedAlternative { get; set; } = string.Empty;

        public bool DecisionChanged { get; set; }

        public string Explanation { get; set; } = string.Empty;

        public EvaluateDecisionResponse? NewEvaluation { get; set; }

        public List<AlternativeAwarenessResult> NonSelectedBetterAlternatives { get; set; } = new();
    }
}
