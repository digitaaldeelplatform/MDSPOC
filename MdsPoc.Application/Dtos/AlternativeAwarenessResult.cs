using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Application.Dtos
{
    public class AlternativeAwarenessResult
    {
        public string AlternativeName { get; set; } = string.Empty;
        public string AlternativeType { get; set; } = string.Empty;
        public double FinalScore { get; set; }
        public double DifferenceWithSelected { get; set; }
    }
}
