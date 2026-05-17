using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    /*
        FeedbackSignal beschrijft nieuwe informatie die invloed kan hebben
        op een eerder genomen MDS-beslissing.

        Dit object bevat zelf geen beslislogica.
        De backend gebruikt dit signaal om automatisch een temperatuur
        te berekenen en te bepalen of herevaluatie nodig is.
    */
    public class FeedbackSignal
    {
        public string Type { get; set; } = string.Empty;

        public string AffectedAlternative { get; set; } = string.Empty;

        public string AffectedCriterion { get; set; } = string.Empty;

        /*
            ChangeValue geeft de relatieve verandering aan.

            Voorbeelden:
            0.20 = 20% kostenstijging
            0.30 = 30% performanceverslechtering
            0.10 = 10% hogere onzekerheid
        */
        public double ChangeValue { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
