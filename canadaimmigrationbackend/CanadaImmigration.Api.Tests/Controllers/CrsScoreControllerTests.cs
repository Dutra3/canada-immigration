using CanadaImmigration.Api.Controllers;
using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CanadaImmigration.Api.Tests.Controllers;

public class CrsScoreControllerTests
{
    private readonly CrsScoreController _sut = new(new CrsScoreService());

    private static CrsScoreRequest ValidRequest() => new()
    {
        Age = 30,
        Education = EducationLevel.BachelorsOrThreeYear,
        FirstLanguageIsFrench = false,
        FirstLanguage = new LanguageAbilities { Listening = 9, Reading = 9, Writing = 9, Speaking = 9 },
        CanadianWorkYears = 1,
        ForeignWorkYears = 0,
        CanadianEducation = CanadianEducationLevel.None
    };

    [Fact]
    public void Calculate_ValidRequest_ReturnsOkWithResult()
    {
        var actionResult = _sut.Calculate(ValidRequest());

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var result = Assert.IsType<CrsScoreResult>(ok.Value);
        Assert.True(result.TotalScore > 0);
        Assert.NotNull(result.Breakdown);
    }

    [Fact]
    public void Calculate_NegativeAge_ReturnsBadRequest()
    {
        var request = ValidRequest();
        request.Age = -1;

        var actionResult = _sut.Calculate(request);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public void Calculate_NegativeClb_ReturnsBadRequest()
    {
        var request = ValidRequest();
        request.FirstLanguage.Listening = -1;

        var actionResult = _sut.Calculate(request);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public void Calculate_NegativeWorkYears_ReturnsBadRequest()
    {
        var request = ValidRequest();
        request.ForeignWorkYears = -2;

        var actionResult = _sut.Calculate(request);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }
}
