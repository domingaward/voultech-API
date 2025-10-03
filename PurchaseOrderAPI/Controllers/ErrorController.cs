using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PurchaseOrderAPI.DTOs;

namespace PurchaseOrderAPI.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            return StatusCode(500, ApiResponse.ErrorResult(
                "Error interno del servidor",
                new { details = exception?.Message }
            ));
        }
    }
}