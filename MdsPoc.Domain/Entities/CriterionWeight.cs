using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    public class CriterionWeight
    {
        public string CriterionName { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}
