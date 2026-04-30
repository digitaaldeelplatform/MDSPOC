using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    public class DecisionContext
    {

        public string Functionality { get; set; } = string.Empty;
        public List<string> Constraints { get; set; } = new();
        public string Environment { get; set; } = string.Empty;
    }

}
