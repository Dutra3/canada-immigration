using CanadaImmigration.Api.Services;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class ExpressEntryPollingServiceTests
{
    // Brasília é UTC-3 (sem horário de verão desde 2019), então basta somar 3h ao UTC.
    [Theory]
    [InlineData(9, true)]   // 06:00 em Brasília (início da janela, inclusivo)
    [InlineData(12, true)]  // 09:00 em Brasília
    [InlineData(1, true)]   // 22:00 em Brasília (ainda dentro da janela)
    [InlineData(2, false)]  // 23:00 em Brasília (fim da janela, exclusivo)
    [InlineData(8, false)]  // 05:00 em Brasília
    [InlineData(3, false)]  // 00:00 em Brasília
    public void IsWithinSendingWindow_RespectsBrasiliaSixAmToElevenPmWindow(int utcHour, bool expected)
    {
        var utcNow = new DateTimeOffset(2026, 1, 15, utcHour, 0, 0, TimeSpan.Zero);

        var result = ExpressEntryPollingService.IsWithinSendingWindow(utcNow);

        Assert.Equal(expected, result);
    }
}
