using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;

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

    // GET api/proofoffunds?familySize=4
    [HttpGet]
    public ActionResult<ProofOfFundsResult> Get([FromQuery] int familySize)
    {
        if (familySize < 1)
        {
            return BadRequest("familySize deve ser no mínimo 1.");
        }

        var result = _proofOfFundsService.Calculate(familySize);
        return Ok(result);
    }
}
