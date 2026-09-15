using CanadaImmigration.Api.Data;
using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class SubscriberServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task SubscribeAsync_WithValidEmailAndCategories_CreatesSubscriber()
    {
        using var db = CreateDbContext();
        var sut = new SubscriberService(db);

        var result = await sut.SubscribeAsync("user@example.com", new List<string> { "CEC", "PNP" });

        Assert.Equal(SubscribeResult.Created, result);
        var subscriber = await db.Subscribers.Include(s => s.Categories).SingleAsync();
        Assert.Equal("user@example.com", subscriber.Email);
        Assert.Equal(2, subscriber.Categories.Count);
    }

    [Fact]
    public async Task SubscribeAsync_WithInvalidEmail_ReturnsInvalidEmail()
    {
        using var db = CreateDbContext();
        var sut = new SubscriberService(db);

        var result = await sut.SubscribeAsync("not-an-email", new List<string> { "CEC" });

        Assert.Equal(SubscribeResult.InvalidEmail, result);
        Assert.Empty(db.Subscribers);
    }

    [Fact]
    public async Task SubscribeAsync_WithNoCategories_ReturnsNoCategories()
    {
        using var db = CreateDbContext();
        var sut = new SubscriberService(db);

        var result = await sut.SubscribeAsync("user@example.com", new List<string>());

        Assert.Equal(SubscribeResult.NoCategories, result);
    }

    [Fact]
    public async Task SubscribeAsync_WithAllCategorySelected_CollapsesToSingleAllEntry()
    {
        using var db = CreateDbContext();
        var sut = new SubscriberService(db);

        await sut.SubscribeAsync("user@example.com", new List<string> { "CEC", "ALL", "PNP" });

        var subscriber = await db.Subscribers.Include(s => s.Categories).SingleAsync();
        var category = Assert.Single(subscriber.Categories);
        Assert.Equal(Subscriber.AllCategoriesValue, category.Category);
    }

    [Fact]
    public async Task SubscribeAsync_CalledTwiceForSameEmail_UpdatesCategoriesInsteadOfDuplicating()
    {
        using var db = CreateDbContext();
        var sut = new SubscriberService(db);

        await sut.SubscribeAsync("user@example.com", new List<string> { "CEC" });
        var secondResult = await sut.SubscribeAsync("user@example.com", new List<string> { "PNP" });

        Assert.Equal(SubscribeResult.Updated, secondResult);
        Assert.Equal(1, await db.Subscribers.CountAsync());
        var subscriber = await db.Subscribers.Include(s => s.Categories).SingleAsync();
        var category = Assert.Single(subscriber.Categories);
        Assert.Equal("PNP", category.Category);
    }

    [Fact]
    public async Task GetSubscribersForCategoryAsync_ReturnsExactMatchAndAllSubscribers()
    {
        using var db = CreateDbContext();
        var sut = new SubscriberService(db);

        await sut.SubscribeAsync("cec-only@example.com", new List<string> { "CEC" });
        await sut.SubscribeAsync("pnp-only@example.com", new List<string> { "PNP" });
        await sut.SubscribeAsync("all@example.com", new List<string> { Subscriber.AllCategoriesValue });

        var matches = await sut.GetSubscribersForCategoryAsync("CEC");

        Assert.Equal(2, matches.Count);
        Assert.Contains(matches, s => s.Email == "cec-only@example.com");
        Assert.Contains(matches, s => s.Email == "all@example.com");
        Assert.DoesNotContain(matches, s => s.Email == "pnp-only@example.com");
    }

    [Fact]
    public async Task UnsubscribeAsync_WithValidToken_RemovesSubscriber()
    {
        using var db = CreateDbContext();
        var sut = new SubscriberService(db);
        await sut.SubscribeAsync("user@example.com", new List<string> { "CEC" });
        var token = (await db.Subscribers.SingleAsync()).UnsubscribeToken;

        var removed = await sut.UnsubscribeAsync(token);

        Assert.True(removed);
        Assert.Empty(db.Subscribers);
    }

    [Fact]
    public async Task UnsubscribeAsync_WithUnknownToken_ReturnsFalse()
    {
        using var db = CreateDbContext();
        var sut = new SubscriberService(db);

        var removed = await sut.UnsubscribeAsync("does-not-exist");

        Assert.False(removed);
    }
}
