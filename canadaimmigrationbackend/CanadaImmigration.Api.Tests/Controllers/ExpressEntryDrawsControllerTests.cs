using CanadaImmigration.Api.Controllers;
using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CanadaImmigration.Api.Tests.Controllers;

public class ExpressEntryDrawsControllerTests
{
    private static List<ExpressEntryDraw> BuildSampleDraws(int count)
    {
        var draws = new List<ExpressEntryDraw>();
        for (var i = 1; i <= count; i++)
        {
            draws.Add(new ExpressEntryDraw
            {
                DrawNumber = i,
                Date = new DateTime(2026, 1, 1).AddDays(i),
                InvitationsIssued = 1000 + i,
                MinimumCrs = 500 - i,
                Category = i % 2 == 0 ? "CEC" : "PNP",
                Year = "2026"
            });
        }
        return draws;
    }

    [Fact]
    public async Task GetDraws_ReturnsFirstPageOrderedByMostRecentDate()
    {
        var fakeService = new FakeExpressEntryDrawService
        {
            DrawsToReturn = BuildSampleDraws(25)
        };
        var sut = new ExpressEntryDrawsController(fakeService);

        var actionResult = await sut.GetDraws(page: 1, pageSize: 20);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var paged = Assert.IsType<PagedResult<ExpressEntryDraw>>(okResult.Value);

        Assert.Equal(20, paged.Items.Count);
        Assert.Equal(25, paged.TotalCount);
        Assert.Equal(2, paged.TotalPages);
        Assert.Equal(25, paged.Items[0].DrawNumber);
    }

    [Fact]
    public async Task GetDraws_SecondPage_ReturnsRemainingItems()
    {
        var fakeService = new FakeExpressEntryDrawService
        {
            DrawsToReturn = BuildSampleDraws(25)
        };
        var sut = new ExpressEntryDrawsController(fakeService);

        var actionResult = await sut.GetDraws(page: 2, pageSize: 20);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var paged = Assert.IsType<PagedResult<ExpressEntryDraw>>(okResult.Value);

        Assert.Equal(5, paged.Items.Count);
    }

    [Fact]
    public async Task GetDraws_PageBeyondAvailableData_ReturnsEmptyItems()
    {
        var fakeService = new FakeExpressEntryDrawService
        {
            DrawsToReturn = BuildSampleDraws(10)
        };
        var sut = new ExpressEntryDrawsController(fakeService);

        var actionResult = await sut.GetDraws(page: 5, pageSize: 20);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var paged = Assert.IsType<PagedResult<ExpressEntryDraw>>(okResult.Value);

        Assert.Empty(paged.Items);
        Assert.Equal(10, paged.TotalCount);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(-1, 20)]
    public async Task GetDraws_WithInvalidPageOrPageSize_ReturnsBadRequest(int page, int pageSize)
    {
        var fakeService = new FakeExpressEntryDrawService();
        var sut = new ExpressEntryDrawsController(fakeService);

        var actionResult = await sut.GetDraws(page: page, pageSize: pageSize);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetLatest_WhenServiceReturnsDraw_ReturnsOk()
    {
        var fakeService = new FakeExpressEntryDrawService
        {
            LatestToReturn = BuildSampleDraws(1)[0]
        };
        var sut = new ExpressEntryDrawsController(fakeService);

        var actionResult = await sut.GetLatest(CancellationToken.None);

        Assert.IsType<OkObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetLatest_WhenServiceReturnsNull_ReturnsNotFound()
    {
        var fakeService = new FakeExpressEntryDrawService
        {
            LatestToReturn = null
        };
        var sut = new ExpressEntryDrawsController(fakeService);

        var actionResult = await sut.GetLatest(CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult.Result);
    }
}