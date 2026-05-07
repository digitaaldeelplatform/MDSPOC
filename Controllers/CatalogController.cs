/*
* CatalogController.cs
 *
 * This file defines the CatalogController class, 
 * which is an API controller responsible for handling HTTP requests 
 * related to the catalog of alternatives and criteria in the MdsPoc 
 * application. It uses the CatalogService to retrieve data and return 
 * it in the response.
 * 
 * Dit maakt de frontend afhankelijk van de backendcatalogus in plaats van 
 * hardcoded frontenddata.
 *
*/

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
