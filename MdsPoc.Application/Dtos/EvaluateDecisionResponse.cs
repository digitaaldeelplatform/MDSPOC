using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Domain.Entities;

// Bevat de resultaten van de evaluatie,
// inclusief de geselecteerde beste optie,

namespace MdsPoc.Application.Dtos
{
    public class EvaluateDecisionResponse
    {
        public string SelectedAlternative { get; set; } = string.Empty;
        public List<EvaluationResult> Results { get; set; } = new List<EvaluationResult>();
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public List<AlternativeAwarenessResult> NonSelectedBetterAlternatives { get; set; } = new();

    }
}
