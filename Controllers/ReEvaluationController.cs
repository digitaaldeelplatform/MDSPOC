using Microsoft.AspNetCore.Mvc;
using MdsPoc.Application.Dtos;
using MdsPoc.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MDSPOC.Controllers
{
    /*
        ReEvaluationController ontvangt feedback vanuit de buitenwereld.

        Belangrijk:
        Deze controller bevat geen beslislogica.
        De controller geeft de request door aan de Application-laag.

        Route:
        POST /api/ReEvaluation/reevaluate
    */
    [ApiController]
    [Route("api/[controller]")]
    public class ReEvaluationController : ControllerBase
    {
        private readonly IReEvaluationService _reEvaluationService;

        public ReEvaluationController(IReEvaluationService reEvaluationService)
        {
            _reEvaluationService = reEvaluationService;
        }

        [HttpPost("reevaluate")]
        public ActionResult<ReEvaluationResponse> ReEvaluate(
            [FromBody] ReEvaluationRequest request)
        {
            var result = _reEvaluationService.ReEvaluate(request);

            return Ok(result);
        }
    }
}