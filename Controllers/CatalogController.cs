// Deze file is onderdeel van een ASP.NET Core Web API project.
// De CatalogController biedt endpoints om alternatieven en criteria op te halen,
// die worden gebruikt in de Microservice Decision Support (MDS) context.
// De CatalogService wordt geïnjecteerd via dependency injection en
// bevat de logica om de benodigde gegevens op te halen.

using Microsoft.AspNetCore.Mvc;
using MdsPoc.Application.Services;

namespace MdsPoc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogService _catalogService;

        public CatalogController(CatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet("alternatives")]
        public IActionResult GetAlternatives()
        {
            return Ok(_catalogService.GetAlternatives());
        }

        [HttpGet("criteria")]
        public IActionResult GetCriteria()
        {
            return Ok(_catalogService.GetCriteria());
        }
    }
}
