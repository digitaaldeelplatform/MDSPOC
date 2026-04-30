using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    internal class FeedbackSignal
    {
        public string Type { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}
