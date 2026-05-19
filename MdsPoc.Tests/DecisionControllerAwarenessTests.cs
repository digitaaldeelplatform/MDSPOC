using Microsoft.AspNetCore.Mvc;
using MdsPoc.Api.Controllers;
using MdsPoc.Application.Dtos;
using MdsPoc.Application.Services;
using MdsPoc.Domain.Entities;
using Xunit;

namespace MdsPoc.Tests
{
    public class DecisionControllerAwarenessTests
    {
        [Fact]
        public void Awareness_Should_Only_Return_Non_Selected_Alternatives_That_Score_Higher()
        {
            var controller = CreateController();

            var request = CreateCatalogRequestWithWeakSelectedAlternatives();

            var actionResult = controller.EvaluateFromCatalog(request);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<EvaluateDecisionResponse>(okResult.Value);

            var selectedScore = response.Results
                .Single(r => r.AlternativeName == response.SelectedAlternative)
                .FinalScore;

            Assert.NotEmpty(response.NonSelectedBetterAlternatives);
            Assert.All(response.NonSelectedBetterAlternatives,
                alternative => Assert.True(alternative.FinalScore > selectedScore));
        }

        [Fact]
        public void Awareness_Should_Not_Override_Selected_Alternative()
        {
            var controller = CreateController();

            var request = CreateCatalogRequestWithWeakSelectedAlternatives();

            var actionResult = controller.EvaluateFromCatalog(request);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<EvaluateDecisionResponse>(okResult.Value);

            var selectedAlternativeNames = response.Results
                .Select(r => r.AlternativeName)
                .ToList();

            Assert.Contains(response.SelectedAlternative, selectedAlternativeNames);
            Assert.DoesNotContain(response.SelectedAlternative,
                response.NonSelectedBetterAlternatives.Select(a => a.AlternativeName));
        }

        [Fact]
        public void Awareness_Should_Return_Empty_List_When_All_Catalog_Alternatives_Are_Selected()
        {
            var controller = CreateController();
            var catalog = new CatalogService();

            var request = new EvaluateFromCatalogRequest
            {
                Context = new DecisionContext
                {
                    Functionality = "Authentication",
                    Environment = "Cloud"
                },
                SelectedAlternativeIds = catalog.GetAlternatives()
                    .Select(a => a.Id)
                    .ToList(),
                SelectedCriterionNames = catalog.GetCriteria()
                    .Select(c => c.Name)
                    .ToList(),
                Weights = catalog.GetCriteria()
                    .Select(c => new CriterionWeight
                    {
                        CriterionName = c.Name,
                        Value = c.DefaultWeight
                    })
                    .ToList()
            };

            var actionResult = controller.EvaluateFromCatalog(request);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<EvaluateDecisionResponse>(okResult.Value);

            Assert.Empty(response.NonSelectedBetterAlternatives);
        }

        private static DecisionController CreateController()
        {
            return new DecisionController(
                new DecisionEvaluationService(),
                new CatalogService());
        }

        private static EvaluateFromCatalogRequest CreateCatalogRequestWithWeakSelectedAlternatives()
        {
            return new EvaluateFromCatalogRequest
            {
                Context = new DecisionContext
                {
                    Functionality = "Authentication",
                    Environment = "Cloud"
                },
                SelectedAlternativeIds = new List<string>
                {
                    "custom-auth-service",
                    "minimal-auth-service"
                },
                SelectedCriterionNames = new List<string>
                {
                    "Performance",
                    "Maintenance",
                    "Tijd"
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Performance", Value = 0.30 },
                    new() { CriterionName = "Maintenance", Value = 0.40 },
                    new() { CriterionName = "Tijd", Value = 0.30 }
                }
            };
        }
    }
}