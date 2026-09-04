using CanadaImmigration.Api.Controllers;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CanadaImmigration.Api.Tests.Controllers;

public class ProofOfFundsControllerTests
{
    private readonly ProofOfFundsController _sut;

    public ProofOfFundsControllerTests()
    {
        _sut = new ProofOfFundsController(new ProofOfFundsService());
    }

    [Fact]
    public void Get_WithValidFamilySize_ReturnsOkWithResult()
    {
        var actionResult = _sut.Get(4);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var value = Assert.IsType<Models.ProofOfFundsResult>(okResult.Value);
        Assert.Equal(28362m, value.RequiredFundsCad);
    }

    [Fact]
    public void Get_WithZeroFamilySize_ReturnsBadRequest()
    {
        var actionResult = _sut.Get(0);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }
}