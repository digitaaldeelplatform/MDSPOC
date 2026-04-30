using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    internal class DecisionOutcome
    {
        public string SelectedAlternative { get; set; } = string.Empty;
        public List<EvaluationResult> EvaluationResults { get; set; } = new List<EvaluationResult>();
        public List<Assumption> Assumptions { get; set; } = new List<Assumption>();
    }
}
