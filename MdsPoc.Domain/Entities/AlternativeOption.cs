using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    public class AlternativeOption
    {
        public string Name { get; set; } = string.Empty;    // Build / Buy / Free
        public string Type { get; set; } = string.Empty;
    }
}
