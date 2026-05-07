/*
De frontend hoeft niet meer alle scores mee te sturen, 
de backend haalt de scores uit de catalogus.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Domain.Entities;

namespace MdsPoc.Application.Dtos
{
    public class EvaluateFromCatalogRequest
    {
        public DecisionContext Context { get; set; } = new();

        public List<string> SelectedAlternativeIds { get; set; } = new();

        public List<string> SelectedCriterionNames { get; set; } = new();

        public List<CriterionWeight> Weights { get; set; } = new();

        public List<Assumption> Assumptions { get; set; } = new();
    }
}
