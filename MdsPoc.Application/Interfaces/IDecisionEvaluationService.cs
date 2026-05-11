using MdsPoc.Application.Dtos;
using MdsPoc.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Interface voor de service die de beslissingsevaluatie uitvoert.
// Implementatie: DecisionEvaluationService.cs

namespace MdsPoc.Application.Interfaces
{
    public interface IDecisionEvaluationService
    {
        EvaluateDecisionResponse Evaluate(EvaluateDecisionRequest request);
    }
}
