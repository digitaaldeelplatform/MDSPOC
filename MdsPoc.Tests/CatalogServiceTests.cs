using MdsPoc.Application.Services;
using Xunit;

namespace MdsPoc.Tests
{
    public class CatalogServiceTests
    {
        [Fact]
        public void GetAlternatives_Should_Return_Full_Catalog()
        {
            var service = new CatalogService();

            var alternatives = service.GetAlternatives();

            Assert.Equal(15, alternatives.Count);
            Assert.Contains(alternatives, a => a.Name == "Auth0");
            Assert.Contains(alternatives, a => a.Name == "Keycloak");
            Assert.Contains(alternatives, a => a.Name == "Custom Auth Service");
        }

        [Fact]
        public void GetCriteria_Should_Return_Six_Default_Criteria()
        {
            var service = new CatalogService();

            var criteria = service.GetCriteria();

            Assert.Equal(6, criteria.Count);
            Assert.Contains(criteria, c => c.Name == "Performance");
            Assert.Contains(criteria, c => c.Name == "Kosten");
            Assert.Contains(criteria, c => c.Name == "Tijd");
        }
    }
}