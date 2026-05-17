using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Domain.Entities;

namespace MdsPoc.Application.Dtos
{
    /*
        ReEvaluationRequest is de input voor herevaluatie.

        De frontend stuurt:
        - de eerder vastgelegde beslissing;
        - een feedbacksignaal.

        De frontend stuurt géén temperatuur en géén triggerbesluit.
        Dat wordt volledig in de backend berekend.
    */
    public class ReEvaluationRequest
    {
        public DecisionRecord DecisionRecord { get; set; } = new();

        public FeedbackSignal FeedbackSignal { get; set; } = new();
    }
}
