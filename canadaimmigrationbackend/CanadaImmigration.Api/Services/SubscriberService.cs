using System.Net.Mail;
using CanadaImmigration.Api.Data;
using CanadaImmigration.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CanadaImmigration.Api.Services;

public class SubscriberService : ISubscriberService
{
    private readonly AppDbContext _db;

    public SubscriberService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SubscribeResult> SubscribeAsync(string email, IReadOnlyList<string> categories, CancellationToken cancellationToken = default)
    {
        email = email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (!IsValidEmail(email))
        {
            return SubscribeResult.InvalidEmail;
        }

        var normalizedCategories = NormalizeCategories(categories);
        if (normalizedCategories.Count == 0)
        {
            return SubscribeResult.NoCategories;
        }

        var existing = await _db.Subscribers
            .Include(s => s.Categories)
            .FirstOrDefaultAsync(s => s.Email == email, cancellationToken);

        if (existing is null)
        {
            var subscriber = new Subscriber
            {
                Email = email,
                Categories = normalizedCategories.Select(c => new SubscriberCategory { Category = c }).ToList()
            };
            _db.Subscribers.Add(subscriber);
            await _db.SaveChangesAsync(cancellationToken);
            return SubscribeResult.Created;
        }

        _db.SubscriberCategories.RemoveRange(existing.Categories);
        existing.Categories = normalizedCategories.Select(c => new SubscriberCategory { Category = c, SubscriberId = existing.Id }).ToList();
        await _db.SaveChangesAsync(cancellationToken);
        return SubscribeResult.Updated;
    }

    public async Task<bool> UnsubscribeAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var subscriber = await _db.Subscribers.FirstOrDefaultAsync(s => s.UnsubscribeToken == token, cancellationToken);
        if (subscriber is null)
        {
            return false;
        }

        _db.Subscribers.Remove(subscriber);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<Subscriber>> GetSubscribersForCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _db.Subscribers
            .Include(s => s.Categories)
            .Where(s => s.Categories.Any(c => c.Category == category || c.Category == Subscriber.AllCategoriesValue))
            .ToListAsync(cancellationToken);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static List<string> NormalizeCategories(IReadOnlyList<string>? categories)
    {
        if (categories is null || categories.Count == 0)
        {
            return new List<string>();
        }

        var cleaned = categories
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cleaned.Any(c => string.Equals(c, Subscriber.AllCategoriesValue, StringComparison.OrdinalIgnoreCase)))
        {
            return new List<string> { Subscriber.AllCategoriesValue };
        }

        return cleaned;
    }
}
