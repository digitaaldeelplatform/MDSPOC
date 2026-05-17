using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsPoc.Domain.Entities
{
    /*
        DecisionRecord bewaart een eerder genomen MDS-beslissing.

        Deze klasse blijft in Domain staan zodat de bestaande projectstructuur
        niet hoeft te worden aangepast.

        Belangrijk:
        Domain mag geen Application DTO's kennen.
        Daarom bevat deze klasse alleen domeinobjecten.
    */
    public class DecisionRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DecisionContext Context { get; set; } = new();

        public string SelectedAlternative { get; set; } = string.Empty;

        public List<AlternativeOption> Alternatives { get; set; } = new();

        public List<Criterion> Criteria { get; set; } = new();

        public List<CriterionWeight> Weights { get; set; } = new();

        public List<CriterionScore> Scores { get; set; } = new();

        public List<EvaluationResult> Results { get; set; } = new();
    }
}
