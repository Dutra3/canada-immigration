using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CanadaImmigration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpressEntryDrawsController : ControllerBase
{
    private const int DefaultPageSize = 20;

    private readonly IExpressEntryDrawService _drawService;

    public ExpressEntryDrawsController(IExpressEntryDrawService drawService)
    {
        _drawService = drawService;
    }

    // GET api/expressentrydraws?page=1&pageSize=20&year=2026&category=PNP
    [HttpGet]
    public async Task<ActionResult<PagedResult<ExpressEntryDraw>>> GetDraws(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] int? year = null,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("page e pageSize devem ser maiores que zero.");
        }

        var allDraws = await _drawService.GetDrawsAsync(year, category, cancellationToken);

        // Ordena do mais recente pro mais antigo antes de paginar
        var ordered = allDraws.OrderByDescending(d => d.Date).ToList();

        var pageItems = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<ExpressEntryDraw>
        {
            Items = pageItems,
            Page = page,
            PageSize = pageSize,
            TotalCount = ordered.Count
        };

        return Ok(result);
    }

    // GET api/expressentrydraws/latest
    [HttpGet("latest")]
    public async Task<ActionResult<ExpressEntryDraw>> GetLatest(CancellationToken cancellationToken)
    {
        var latest = await _drawService.GetLatestDrawAsync(cancellationToken);

        if (latest is null)
        {
            return NotFound();
        }

        return Ok(latest);
    }

    // GET api/expressentrydraws/pool
    [HttpGet("pool")]
    public async Task<ActionResult<PoolDistribution>> GetPoolDistribution(CancellationToken cancellationToken)
    {
        var pool = await _drawService.GetPoolDistributionAsync(cancellationToken);

        if (pool is null)
        {
            return NotFound();
        }

        return Ok(pool);
    }
}
