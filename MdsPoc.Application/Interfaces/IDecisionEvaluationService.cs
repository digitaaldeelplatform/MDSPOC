using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Application.Dtos;

namespace MdsPoc.Application.Interfaces
{
    public interface IDecisionEvaluationService
    {
        EvaluateDecisionResponse Evaluate(EvaluateDecisionRequest request);
    }
}
