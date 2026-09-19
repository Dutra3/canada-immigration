using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CanadaImmigration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrsScoreController : ControllerBase
{
    private readonly ICrsScoreService _crsScoreService;
    private readonly IInvitationService _invitationService;

    public CrsScoreController(ICrsScoreService crsScoreService, IInvitationService invitationService)
    {
        _crsScoreService = crsScoreService;
        _invitationService = invitationService;
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

    [HttpGet("invitation")]
    public async Task<ActionResult<InvitationAnalysis>> Invitation(
        [FromQuery] int score,
        [FromQuery] int canadianWorkYears,
        [FromQuery] bool hasFrenchProficiency,
        [FromQuery] bool hasProvincialNomination,
        CancellationToken cancellationToken)
    {
        if (score < 0 || score > 1200 || canadianWorkYears < 0)
        {
            return BadRequest("Score deve estar entre 0 e 1200 e experiência não pode ser negativa.");
        }

        var analysis = await _invitationService.AnalyzeAsync(
            score, canadianWorkYears, hasFrenchProficiency, hasProvincialNomination, cancellationToken);

        return Ok(analysis);
    }

    private static bool HasNegativeAbility(LanguageAbilities? language)
    {
        return language is not null && language.All.Any(clb => clb < 0);
    }
}
