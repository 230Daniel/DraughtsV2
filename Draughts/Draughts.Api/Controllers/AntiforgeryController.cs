using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Draughts.Api.Controllers;

[Route("antiforgery")]
public class AntiforgeryController : Controller
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }
        
    [HttpGet]
    public IActionResult Get()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Json(tokens.RequestToken);
    }
}