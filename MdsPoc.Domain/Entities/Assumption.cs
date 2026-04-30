using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    public class Assumption
    {
        public string Description { get; set; } = string.Empty;
        public string ImpactLevel { get; set; } = string.Empty;
    }
}
