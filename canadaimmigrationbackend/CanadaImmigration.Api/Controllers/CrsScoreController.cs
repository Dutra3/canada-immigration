using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CanadaImmigration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrsScoreController : ControllerBase
{
    private readonly ICrsScoreService _crsScoreService;

    public CrsScoreController(ICrsScoreService crsScoreService)
    {
        _crsScoreService = crsScoreService;
    }

    [HttpPost]
    public ActionResult<CrsScoreResult> Calculate([FromBody] CrsScoreRequest request)
    {
        if (request.Age < 0
            || request.CanadianWorkYears < 0
            || request.ForeignWorkYears < 0
            || HasNegativeAbility(request.FirstLanguage)
            || HasNegativeAbility(request.SecondLanguage))
        {
            return BadRequest("Valores não podem ser negativos.");
        }

        return Ok(_crsScoreService.Calculate(request));
    }

    private static bool HasNegativeAbility(LanguageAbilities? language)
    {
        return language is not null && language.All.Any(clb => clb < 0);
    }
}
