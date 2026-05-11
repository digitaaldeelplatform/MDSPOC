using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    /
    public class CriterionScore
    {
        public string AlternativeName { get; set; } = string.Empty;
        public string CriterionName { get; set; } = string.Empty;
        public double Score { get; set; }       //0..1
        public double Uncertainty { get; set; } //0..1
    }
}
