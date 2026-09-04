using CanadaImmigration.Api.Services;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class ProofOfFundsServiceTests
{
    private readonly ProofOfFundsService _sut;
       
    public ProofOfFundsServiceTests()
    {
        _sut = new ProofOfFundsService();
    }

    [Theory]
    [InlineData(1, 15263)]
    [InlineData(2, 19001)]
    [InlineData(3, 23360)]
    [InlineData(4, 28362)]
    [InlineData(5, 32168)]
    [InlineData(6, 36280)]
    [InlineData(7, 40392)]
    public void Calculate_WithFamilySizeUpToSeven_ReturnsTableValue(int familySize, decimal expected)
    {
        var result = _sut.Calculate(familySize);

        Assert.Equal(familySize, result.FamilySize);
        Assert.Equal(expected, result.RequiredFundsCad);
    }

    [Fact]
    public void Calculate_WithEightMembers_AddsOneExtraIncrement()
    {
        var result = _sut.Calculate(8);

        Assert.Equal(44504m, result.RequiredFundsCad);
    }

    [Fact]
    public void Calculate_WithNineMembers_AddsTwoExtraIncrements()
    {
        var result = _sut.Calculate(9);

        Assert.Equal(48616m, result.RequiredFundsCad);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Calculate_WithInvalidFamilySize_ThrowsArgumentOutOfRangeException(int familySize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Calculate(familySize));
    }
}