using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsPoc.Application.Dtos;
using MdsPoc.Application.Services;
using MdsPoc.Domain.Entities;
using Xunit;

namespace MdsPoc.Tests
{
    public class DecisionEvaluationServiceTests
    {
        [Fact]
        public void Evaluate_Should_Return_Best_Alternative()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Context = new DecisionContext
                {
                    Functionality = "Authentication",
                    Environment = "Cloud"
                },
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" },
                    new() { Name = "Buy", Type = "Buy" },
                    new() { Name = "Free", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Time", Category = "Operational" },
                    new() { Name = "Cost", Category = "Economic" },
                    new() { Name = "Maintainability", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Time", Value = 0.4 },
                    new() { CriterionName = "Cost", Value = 0.3 },
                    new() { CriterionName = "Maintainability", Value = 0.3 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Build", CriterionName = "Time", Score = 0.4, Uncertainty = 0.1 },
                    new() { AlternativeName = "Build", CriterionName = "Cost", Score = 0.7, Uncertainty = 0.1 },
                    new() { AlternativeName = "Build", CriterionName = "Maintainability", Score = 0.9, Uncertainty = 0.2 },

                    new() { AlternativeName = "Buy", CriterionName = "Time", Score = 0.9, Uncertainty = 0.1 },
                    new() { AlternativeName = "Buy", CriterionName = "Cost", Score = 0.5, Uncertainty = 0.1 },
                    new() { AlternativeName = "Buy", CriterionName = "Maintainability", Score = 0.7, Uncertainty = 0.1 },

                    new() { AlternativeName = "Free", CriterionName = "Time", Score = 0.8, Uncertainty = 0.2 },
                    new() { AlternativeName = "Free", CriterionName = "Cost", Score = 0.9, Uncertainty = 0.2 },
                    new() { AlternativeName = "Free", CriterionName = "Maintainability", Score = 0.6, Uncertainty = 0.2 }
                }
            };

            var response = service.Evaluate(request);

            Assert.Empty(response.ValidationErrors);
            Assert.NotEmpty(response.Results);
            Assert.False(string.IsNullOrWhiteSpace(response.SelectedAlternative));
        }

        [Fact]
        public void Evaluate_Should_Return_Validation_Error_When_Weights_Do_Not_Sum_To_One()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" },
                    new() { Name = "Buy", Type = "Buy" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Time", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Time", Value = 0.5 }
                }
            };

            var response = service.Evaluate(request);

            Assert.NotEmpty(response.ValidationErrors);
        }
    }
}
