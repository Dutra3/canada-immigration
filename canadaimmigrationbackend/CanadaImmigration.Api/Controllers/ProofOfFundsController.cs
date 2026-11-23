using System.ComponentModel.DataAnnotations;
using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CanadaImmigration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProofOfFundsController : ControllerBase
{
    private readonly IProofOfFundsService _proofOfFundsService;

    public ProofOfFundsController(IProofOfFundsService proofOfFundsService)
    {
        _proofOfFundsService = proofOfFundsService;
    }

    [HttpGet]
    [EnableRateLimiting("compute")]
    public ActionResult<ProofOfFundsResult> Get([FromQuery, Range(1, 50)] int familySize)
    {
        if (familySize < 1)
        {
            return BadRequest("familySize deve ser no mínimo 1.");
        }

        var result = _proofOfFundsService.Calculate(familySize);
        return Ok(result);
    }
}
