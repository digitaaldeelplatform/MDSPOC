using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Domain.Entities;

namespace MdsPoc.Application.Dtos
{
    public class EvaluateDecisionRequest
    {
        public DecisionContext Context { get; set; } = new DecisionContext();
        public List<AlternativeOption> Alternatives { get; set; } = new List<AlternativeOption>();
        public List<Criterion> Criteria { get; set; } = new List<Criterion>();
        public List<CriterionWeight> Weights { get; set; } = new List<CriterionWeight>();
        public List<CriterionScore> Scores { get; set; } = new List<CriterionScore>();
        public List<Assumption> Assumptions { get; set; } = new List<Assumption>();
    }
}
