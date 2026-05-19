using MdsPoc.Application.Dtos;
using MdsPoc.Application.Services;
using MdsPoc.Domain.Entities;
using Xunit;

namespace MdsPoc.Tests
{
    public class ReEvaluationServiceTests
    {
        [Fact]
        public void ReEvaluate_Should_Not_Run_When_Temperature_Is_Below_Threshold()
        {
            var service = CreateService();

            var request = new ReEvaluationRequest
            {
                DecisionRecord = CreateDecisionRecord(),
                FeedbackSignal = new FeedbackSignal
                {
                    Type = "COST_INCREASE",
                    AffectedAlternative = "Buy",
                    AffectedCriterion = "Kosten",
                    ChangeValue = 0.01
                }
            };

            var response = service.ReEvaluate(request);

            Assert.False(response.ReEvaluationTriggered);
            Assert.False(response.DecisionChanged);
            Assert.Null(response.NewEvaluation);
            Assert.Empty(response.NonSelectedBetterAlternatives);
        }

        [Fact]
        public void ReEvaluate_Should_Run_When_Temperature_Exceeds_Threshold()
        {
            var service = CreateService();

            var request = new ReEvaluationRequest
            {
                DecisionRecord = CreateDecisionRecord(),
                FeedbackSignal = new FeedbackSignal
                {
                    Type = "COST_INCREASE",
                    AffectedAlternative = "Buy",
                    AffectedCriterion = "Kosten",
                    ChangeValue = 0.50
                }
            };

            var response = service.ReEvaluate(request);

            Assert.True(response.ReEvaluationTriggered);
            Assert.NotNull(response.NewEvaluation);
            Assert.True(response.Temperature.ShouldReEvaluate);
        }

        [Fact]
        public void ReEvaluate_Should_Include_Non_Selected_Better_Alternatives()
        {
            var service = CreateService();

            var request = new ReEvaluationRequest
            {
                DecisionRecord = CreateDecisionRecord(),
                FeedbackSignal = new FeedbackSignal
                {
                    Type = "COST_INCREASE",
                    AffectedAlternative = "Buy",
                    AffectedCriterion = "Kosten",
                    ChangeValue = 0.50
                }
            };

            var response = service.ReEvaluate(request);

            Assert.True(response.ReEvaluationTriggered);
            Assert.NotNull(response.NewEvaluation);
            Assert.Equal(
                response.NonSelectedBetterAlternatives.Count,
                response.NewEvaluation!.NonSelectedBetterAlternatives.Count);
        }

        [Fact]
        public void ReEvaluate_Should_Not_Automatically_Override_With_Awareness_Alternative()
        {
            var service = CreateService();

            var request = new ReEvaluationRequest
            {
                DecisionRecord = CreateDecisionRecord(),
                FeedbackSignal = new FeedbackSignal
                {
                    Type = "COST_INCREASE",
                    AffectedAlternative = "Buy",
                    AffectedCriterion = "Kosten",
                    ChangeValue = 0.50
                }
            };

            var response = service.ReEvaluate(request);

            Assert.True(response.ReEvaluationTriggered);

            Assert.DoesNotContain(
                response.NewSelectedAlternative,
                response.NonSelectedBetterAlternatives.Select(a => a.AlternativeName));
        }

        private static ReEvaluationService CreateService()
        {
            return new ReEvaluationService(
                new DecisionEvaluationService(),
                new TemperatureService(),
                new CatalogService());
        }

        private static DecisionRecord CreateDecisionRecord()
        {
            return new DecisionRecord
            {
                Context = new DecisionContext
                {
                    Functionality = "Authentication",
                    Environment = "Cloud"
                },
                SelectedAlternative = "Buy",
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" },
                    new() { Name = "Buy", Type = "Buy" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Kosten", Category = "Economic" },
                    new() { Name = "Maintenance", Category = "Technical" },
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Kosten", Value = 0.40 },
                    new() { CriterionName = "Maintenance", Value = 0.30 },
                    new() { CriterionName = "Tijd", Value = 0.30 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Build", CriterionName = "Kosten", Score = 0.40, Uncertainty = 0.10 },
                    new() { AlternativeName = "Build", CriterionName = "Maintenance", Score = 0.75, Uncertainty = 0.10 },
                    new() { AlternativeName = "Build", CriterionName = "Tijd", Score = 0.30, Uncertainty = 0.10 },

                    new() { AlternativeName = "Buy", CriterionName = "Kosten", Score = 0.80, Uncertainty = 0.10 },
                    new() { AlternativeName = "Buy", CriterionName = "Maintenance", Score = 0.85, Uncertainty = 0.10 },
                    new() { AlternativeName = "Buy", CriterionName = "Tijd", Score = 0.90, Uncertainty = 0.10 }
                }
            };
        }
    }
}