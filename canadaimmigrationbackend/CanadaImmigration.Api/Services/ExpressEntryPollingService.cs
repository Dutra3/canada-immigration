using CanadaImmigration.Api.Data;
using CanadaImmigration.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CanadaImmigration.Api.Services;

// Fica de olho na API externa de draws e, quando aparece uma rodada nova,
// dispara e-mail para os assinantes interessados na categoria daquele draw.
//
// Só faz a checagem dentro do horário comercial de Brasília (6h-23h), pra evitar
// chamadas desnecessárias à API externa durante a madrugada.
public class ExpressEntryPollingService : BackgroundService
{
    private static readonly TimeZoneInfo BrasiliaTimeZone = ResolveBrasiliaTimeZone();

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpressEntryPollingService> _logger;
    private readonly TimeSpan _pollingInterval;

    public ExpressEntryPollingService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpressEntryPollingService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var minutes = configuration.GetValue<int?>("ExpressEntryPolling:IntervalMinutes") ?? 60;
        _pollingInterval = TimeSpan.FromMinutes(minutes);
    }

    // Extraído como método estático para poder ser testado sem precisar do BackgroundService inteiro.
    public static bool IsWithinSendingWindow(DateTimeOffset utcNow)
    {
        var brasiliaNow = TimeZoneInfo.ConvertTime(utcNow, BrasiliaTimeZone);
        return brasiliaNow.Hour >= 6 && brasiliaNow.Hour < 23;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_pollingInterval);

        do
        {
            try
            {
                if (IsWithinSendingWindow(DateTimeOffset.UtcNow))
                {
                    await CheckForNewDrawAsync(stoppingToken);
                }
                else
                {
                    _logger.LogInformation("Fora da janela de checagem (6h-23h Brasília). Pulando verificação.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar novos draws do Express Entry.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckForNewDrawAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var drawService = scope.ServiceProvider.GetRequiredService<IExpressEntryDrawService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var subscriberService = scope.ServiceProvider.GetRequiredService<ISubscriberService>();
        var emailNotifier = scope.ServiceProvider.GetRequiredService<EmailNotifier>();

        var latestDraw = await drawService.GetLatestDrawAsync(cancellationToken);
        if (latestDraw is null)
        {
            return;
        }

        var state = await db.DrawCheckStates.FirstOrDefaultAsync(cancellationToken);
        if (state is null)
        {
            // Primeira execução: só registra o estado atual, sem notificar (evita
            // disparar e-mail de "novo draw" pra rodadas que já existiam antes do sistema existir).
            db.DrawCheckStates.Add(new DrawCheckState
            {
                LastDrawNumber = latestDraw.DrawNumber,
                LastCheckedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        state.LastCheckedAtUtc = DateTime.UtcNow;

        if (latestDraw.DrawNumber <= state.LastDrawNumber)
        {
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Novo draw detectado: #{DrawNumber} ({Category})", latestDraw.DrawNumber, latestDraw.Category);

        state.LastDrawNumber = latestDraw.DrawNumber;
        await db.SaveChangesAsync(cancellationToken);

        var subscribers = await subscriberService.GetSubscribersForCategoryAsync(latestDraw.Category, cancellationToken);

        foreach (var subscriber in subscribers)
        {
            await emailNotifier.SendNewDrawNotificationAsync(subscriber.Email, subscriber.UnsubscribeToken, latestDraw, cancellationToken);
        }
    }

    private static TimeZoneInfo ResolveBrasiliaTimeZone()
    {
        // O ID do fuso muda entre Windows ("E. South America Standard Time") e IANA/Linux/macOS ("America/Sao_Paulo").
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }
}
