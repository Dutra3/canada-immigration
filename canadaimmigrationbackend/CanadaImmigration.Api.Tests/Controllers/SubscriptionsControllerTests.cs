using CanadaImmigration.Api.Controllers;
using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CanadaImmigration.Api.Tests.Controllers;

public class SubscriptionsControllerTests
{
    private class FakeSubscriberService : ISubscriberService
    {
        public SubscribeResult ResultToReturn { get; set; } = SubscribeResult.Created;
        public bool UnsubscribeResultToReturn { get; set; } = true;

        public Task<SubscribeResult> SubscribeAsync(string email, IReadOnlyList<string> categories, CancellationToken cancellationToken = default)
            => Task.FromResult(ResultToReturn);

        public Task<bool> UnsubscribeAsync(string token, CancellationToken cancellationToken = default)
            => Task.FromResult(UnsubscribeResultToReturn);

        public Task<IReadOnlyList<Subscriber>> GetSubscribersForCategoryAsync(string category, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<Subscriber>)new List<Subscriber>());
    }

    [Theory]
    [InlineData(SubscribeResult.Created, typeof(OkObjectResult))]
    [InlineData(SubscribeResult.Updated, typeof(OkObjectResult))]
    [InlineData(SubscribeResult.InvalidEmail, typeof(BadRequestObjectResult))]
    [InlineData(SubscribeResult.NoCategories, typeof(BadRequestObjectResult))]
    public async Task Subscribe_MapsServiceResultToExpectedHttpStatus(SubscribeResult serviceResult, Type expectedResultType)
    {
        var fakeService = new FakeSubscriberService { ResultToReturn = serviceResult };
        var sut = new SubscriptionsController(fakeService);

        var result = await sut.Subscribe(new SubscriptionRequest { Email = "user@example.com", Categories = new List<string> { "CEC" } }, CancellationToken.None);

        Assert.IsType(expectedResultType, result);
    }

    [Fact]
    public async Task Unsubscribe_WithValidToken_ReturnsOk()
    {
        var fakeService = new FakeSubscriberService { UnsubscribeResultToReturn = true };
        var sut = new SubscriptionsController(fakeService);

        var result = await sut.Unsubscribe("some-token", CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Unsubscribe_WithUnknownToken_ReturnsNotFound()
    {
        var fakeService = new FakeSubscriberService { UnsubscribeResultToReturn = false };
        var sut = new SubscriptionsController(fakeService);

        var result = await sut.Unsubscribe("unknown", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
