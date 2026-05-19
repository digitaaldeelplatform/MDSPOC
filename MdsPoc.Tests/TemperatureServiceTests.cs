using MdsPoc.Application.Services;
using MdsPoc.Domain.Entities;
using Xunit;

namespace MdsPoc.Tests
{
    public class TemperatureServiceTests
    {
        [Fact]
        public void CalculateTemperature_Should_Trigger_ReEvaluation_When_Change_Exceeds_Threshold()
        {
            var service = new TemperatureService();

            var feedback = new FeedbackSignal
            {
                AffectedCriterion = "Kosten",
                ChangeValue = 0.20
            };

            var weights = new List<CriterionWeight>
            {
                new() { CriterionName = "Kosten", Value = 0.40 }
            };

            var result = service.CalculateTemperature(feedback, weights);

            Assert.True(result.ShouldReEvaluate);
            Assert.True(result.Temperature >= 1.0);
        }

        [Fact]
        public void CalculateTemperature_Should_Not_Trigger_When_Criterion_Is_Unknown()
        {
            var service = new TemperatureService();

            var feedback = new FeedbackSignal
            {
                AffectedCriterion = "Unknown",
                ChangeValue = 0.50
            };

            var weights = new List<CriterionWeight>
            {
                new() { CriterionName = "Kosten", Value = 1.0 }
            };

            var result = service.CalculateTemperature(feedback, weights);

            Assert.False(result.ShouldReEvaluate);
            Assert.Equal(0, result.Temperature);
            Assert.Equal(1, result.TriggerThreshold);
        }
    }
}