/*
BaselineScore = standaardbeoordeling van één alternatief op één criterium.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    public class BaselineScore
    {
        public string CriterionName { get; set; } = string.Empty;

        public double Score { get; set; }

        public double Uncertainty { get; set; }
    }
}