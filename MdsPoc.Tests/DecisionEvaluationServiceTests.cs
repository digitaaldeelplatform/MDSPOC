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

            var request = CreateBaseRequest();

            var response = service.Evaluate(request);

            Assert.Empty(response.ValidationErrors);
            Assert.NotEmpty(response.Results);
            Assert.Equal("Buy", response.SelectedAlternative);
        }

        [Fact]
        public void Evaluate_Should_Apply_Uncertainty_Correction()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Certain", Type = "Build" },
                    new() { Name = "Uncertain", Type = "Build" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Performance", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Performance", Value = 1.0 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Certain", CriterionName = "Performance", Score = 0.8, Uncertainty = 0.0 },
                    new() { AlternativeName = "Uncertain", CriterionName = "Performance", Score = 0.8, Uncertainty = 0.5 }
                }
            };

            var response = service.Evaluate(request);

            var certain = response.Results.Single(r => r.AlternativeName == "Certain");
            var uncertain = response.Results.Single(r => r.AlternativeName == "Uncertain");

            Assert.Equal(0.8, certain.FinalScore);
            Assert.Equal(0.4, uncertain.FinalScore);
            Assert.Equal("Certain", response.SelectedAlternative);
        }

        [Fact]
        public void Evaluate_Should_Calculate_Coverage_When_Criterion_Is_Missing()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Complete", Type = "Build" },
                    new() { Name = "Incomplete", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Maintenance", Category = "Technical" },
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Maintenance", Value = 0.2 },
                    new() { CriterionName = "Tijd", Value = 0.8 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Complete", CriterionName = "Maintenance", Score = 0.75, Uncertainty = 0.2 },
                    new() { AlternativeName = "Complete", CriterionName = "Tijd", Score = 0.30, Uncertainty = 0.2 },

                    new() { AlternativeName = "Incomplete", CriterionName = "Maintenance", Score = 0.70, Uncertainty = 0.2 }
                }
            };

            var response = service.Evaluate(request);

            var incomplete = response.Results.Single(r => r.AlternativeName == "Incomplete");

            Assert.Equal(0.2, incomplete.Coverage);
            Assert.Equal(0.8, incomplete.MissingWeight);
            Assert.Contains("Tijd", incomplete.MissingCriteria);
        }

        [Fact]
        public void Evaluate_Should_Apply_Non_Linear_Missing_Criteria_Correction()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Complete", Type = "Build" },
                    new() { Name = "MissingImportantCriterion", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Maintenance", Category = "Technical" },
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Maintenance", Value = 0.2 },
                    new() { CriterionName = "Tijd", Value = 0.8 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Complete", CriterionName = "Maintenance", Score = 0.75, Uncertainty = 0.2 },
                    new() { AlternativeName = "Complete", CriterionName = "Tijd", Score = 0.30, Uncertainty = 0.2 },

                    new() { AlternativeName = "MissingImportantCriterion", CriterionName = "Maintenance", Score = 0.70, Uncertainty = 0.2 }
                }
            };

            var response = service.Evaluate(request);

            var result = response.Results.Single(r => r.AlternativeName == "MissingImportantCriterion");

            Assert.Equal(0.56, result.NormalizedScore);
            Assert.Equal(0.36, result.CorrectionFactor);
            Assert.Equal(0.2016, result.FinalScore);
        }

        [Fact]
        public void Evaluate_Should_Break_Tie_Using_Highest_Weighted_Criterion()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "AlternativeA", Type = "Build" },
                    new() { Name = "AlternativeB", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Kosten", Category = "Economic" },
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Kosten", Value = 0.2 },
                    new() { CriterionName = "Tijd", Value = 0.8 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "AlternativeA", CriterionName = "Kosten", Score = 0.9, Uncertainty = 0.0 },
                    new() { AlternativeName = "AlternativeA", CriterionName = "Tijd", Score = 0.5, Uncertainty = 0.0 },

                    new() { AlternativeName = "AlternativeB", CriterionName = "Kosten", Score = 0.5, Uncertainty = 0.0 },
                    new() { AlternativeName = "AlternativeB", CriterionName = "Tijd", Score = 0.6, Uncertainty = 0.0 }
                }
            };

            // Forceer gelijke eindscore:
            // A = 0.2*0.9 + 0.8*0.5 = 0.58
            // B = 0.2*0.5 + 0.8*0.6 = 0.58
            // Tie-break kijkt naar zwaarste criterium: Tijd.
            // B scoort daar hoger op.

            var response = service.Evaluate(request);

            Assert.Equal("AlternativeB", response.SelectedAlternative);
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
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Tijd", Value = 0.5 }
                }
            };

            var response = service.Evaluate(request);

            Assert.NotEmpty(response.ValidationErrors);
            Assert.Contains("The sum of all criterion weights must be 1.0.", response.ValidationErrors);
        }

        [Fact]
        public void Evaluate_Should_Return_Validation_Error_When_Less_Than_Two_Alternatives()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Tijd", Value = 1.0 }
                }
            };

            var response = service.Evaluate(request);

            Assert.NotEmpty(response.ValidationErrors);
            Assert.Contains("At least two alternatives are required.", response.ValidationErrors);
        }

        [Fact]
        public void Evaluate_Should_Return_Validation_Error_When_Criterion_Has_No_Weight()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" },
                    new() { Name = "Free", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Performance", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>()
            };

            var response = service.Evaluate(request);

            Assert.NotEmpty(response.ValidationErrors);
            Assert.Contains("Missing weight for criterion 'Performance'.", response.ValidationErrors);
        }

        [Fact]
        public void Evaluate_Should_Return_Zero_When_All_Criteria_Are_Missing_For_Alternative()
        {
            var service = new DecisionEvaluationService();

            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Known", Type = "Build" },
                    new() { Name = "Unknown", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Performance", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Performance", Value = 1.0 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Known", CriterionName = "Performance", Score = 0.8, Uncertainty = 0.0 }
                }
            };

            var response = service.Evaluate(request);

            var unknown = response.Results.Single(r => r.AlternativeName == "Unknown");

            Assert.Equal(0, unknown.FinalScore);
            Assert.Equal(0, unknown.Coverage);
            Assert.Equal(1, unknown.MissingWeight);
            Assert.Equal(0, unknown.CorrectionFactor);
            Assert.Contains("Performance", unknown.MissingCriteria);
        }

        private static EvaluateDecisionRequest CreateBaseRequest()
        {
            return new EvaluateDecisionRequest
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
                    new() { Name = "Tijd", Category = "Operational" },
                    new() { Name = "Kosten", Category = "Economic" },
                    new() { Name = "Maintenance", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Tijd", Value = 0.4 },
                    new() { CriterionName = "Kosten", Value = 0.3 },
                    new() { CriterionName = "Maintenance", Value = 0.3 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Build", CriterionName = "Tijd", Score = 0.4, Uncertainty = 0.1 },
                    new() { AlternativeName = "Build", CriterionName = "Kosten", Score = 0.7, Uncertainty = 0.1 },
                    new() { AlternativeName = "Build", CriterionName = "Maintenance", Score = 0.9, Uncertainty = 0.2 },

                    new() { AlternativeName = "Buy", CriterionName = "Tijd", Score = 0.9, Uncertainty = 0.1 },
                    new() { AlternativeName = "Buy", CriterionName = "Kosten", Score = 0.5, Uncertainty = 0.1 },
                    new() { AlternativeName = "Buy", CriterionName = "Maintenance", Score = 0.7, Uncertainty = 0.1 },

                    new() { AlternativeName = "Free", CriterionName = "Tijd", Score = 0.8, Uncertainty = 0.2 },
                    new() { AlternativeName = "Free", CriterionName = "Kosten", Score = 0.9, Uncertainty = 0.2 },
                    new() { AlternativeName = "Free", CriterionName = "Maintenance", Score = 0.6, Uncertainty = 0.2 }
                }
            };
        }
    }
}