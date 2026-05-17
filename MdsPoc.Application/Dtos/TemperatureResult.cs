using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Application.Dtos
{
    /*
        TemperatureResult bevat de automatisch berekende temperatuur
        van een feedbacksignaal.

        Temperature >= 1.0 betekent dat herevaluatie nodig is.
    */
    public class TemperatureResult
    {
        public string CriterionName { get; set; } = string.Empty;

        public double CriterionWeight { get; set; }

        public double ChangeValue { get; set; }

        public double TriggerThreshold { get; set; }

        public double Temperature { get; set; }

        public bool ShouldReEvaluate { get; set; }
    }
}
