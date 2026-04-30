using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    public class EvaluationResult
    {
        public string AlternativeName { get; set; } = string.Empty;
        public double FinalScore { get; set; }
        public double Coverage { get; set; }
        public List<string> MissingCriteria { get; set; } = new List<string>();
        public double NormalizedScore { get; set; }
        public double MissingWeight { get; set; }
        public double CorrectionFactor { get; set; }
    }
}
