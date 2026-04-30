using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Domain.Entities;

namespace MdsPoc.Application.Dtos
{
    public class EvaluateDecisionResponse
    {
        public string SelectedAlternative { get; set; } = string.Empty;
        public List<EvaluationResult> Results { get; set; } = new List<EvaluationResult>();
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}
