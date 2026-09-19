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

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories(CancellationToken cancellationToken)
    {
        var draws = await _drawService.GetDrawsAsync(cancellationToken: cancellationToken);

        var categories = draws
            .Select(d => d.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        return Ok(categories);
    }

    [HttpGet("years")]
    public async Task<ActionResult<IReadOnlyList<int>>> GetYears(CancellationToken cancellationToken)
    {
        var draws = await _drawService.GetDrawsAsync(cancellationToken: cancellationToken);

        var years = draws
            .Select(d => d.Date.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToList();

        return Ok(years);
    }
}
