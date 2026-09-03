namespace CanadaImmigration.Api.Models;

// Envelope genérico de paginação, usado pelo Controller pra devolver
// 20 draws por página, como definido no escopo do projeto.
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
