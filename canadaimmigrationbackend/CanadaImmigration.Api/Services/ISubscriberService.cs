using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

public enum SubscribeResult
{
    Created,
    Updated,
    InvalidEmail,
    NoCategories
}

public interface ISubscriberService
{
    Task<SubscribeResult> SubscribeAsync(string email, IReadOnlyList<string> categories, CancellationToken cancellationToken = default);

    Task<bool> UnsubscribeAsync(string token, CancellationToken cancellationToken = default);

    // Assinantes que devem ser avisados sobre um draw da categoria informada
    // (inclui quem escolheu "ALL").
    Task<IReadOnlyList<Subscriber>> GetSubscribersForCategoryAsync(string category, CancellationToken cancellationToken = default);
}
