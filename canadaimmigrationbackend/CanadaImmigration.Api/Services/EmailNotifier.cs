using CanadaImmigration.Api.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CanadaImmigration.Api.Services;

public class EmailNotifierConfig
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = "Canada Immigration Tracker";

    /// <summary>URL pública do site (frontend), usada nos links dos e-mails.</summary>
    public string SiteBaseUrl { get; set; } = string.Empty;
}

public class EmailNotifier
{
    private readonly EmailNotifierConfig _config;
    private readonly ILogger<EmailNotifier> _logger;

    public EmailNotifier(IOptions<EmailNotifierConfig> config, ILogger<EmailNotifier> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task SendNewDrawNotificationAsync(string recipientEmail, string unsubscribeToken, ExpressEntryDraw draw, CancellationToken cancellationToken = default)
    {
        var subject = $"Nova rodada do Express Entry: {draw.Category} (CRS mínimo {draw.MinimumCrs})";
        var body = BuildBody(draw, unsubscribeToken);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config.SenderDisplayName, _config.SenderEmail));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

        var secureSocketOptions = _config.SmtpPort == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(_config.SmtpServer, _config.SmtpPort, secureSocketOptions, cancellationToken);
        await smtpClient.AuthenticateAsync(_config.SenderEmail, _config.SenderPassword, cancellationToken);
        await smtpClient.SendAsync(message, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("E-mail de novo draw enviado para {Email} (draw #{DrawNumber})", recipientEmail, draw.DrawNumber);
    }

    private string BuildBody(ExpressEntryDraw draw, string unsubscribeToken)
    {
        var unsubscribeUrl = string.IsNullOrWhiteSpace(_config.SiteBaseUrl)
            ? null
            : $"{_config.SiteBaseUrl.TrimEnd('/')}/unsubscribe?token={unsubscribeToken}";

        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Nova rodada do Express Entry detectada!</h2>
                    <table style='border-collapse: collapse;'>
                        <tr><td style='padding: 4px 8px;'><strong>Draw:</strong></td><td style='padding: 4px 8px;'>#{draw.DrawNumber}</td></tr>
                        <tr><td style='padding: 4px 8px;'><strong>Data:</strong></td><td style='padding: 4px 8px;'>{draw.Date:dd/MM/yyyy}</td></tr>
                        <tr><td style='padding: 4px 8px;'><strong>Categoria:</strong></td><td style='padding: 4px 8px;'>{draw.Category}</td></tr>
                        <tr><td style='padding: 4px 8px;'><strong>Convites emitidos:</strong></td><td style='padding: 4px 8px;'>{draw.InvitationsIssued}</td></tr>
                        <tr><td style='padding: 4px 8px;'><strong>CRS mínimo:</strong></td><td style='padding: 4px 8px;'>{draw.MinimumCrs}</td></tr>
                    </table>
                    {(unsubscribeUrl is null ? string.Empty : $"<p style='margin-top: 24px; font-size: 12px; color: #666;'>Não quer mais receber esses e-mails? <a href='{unsubscribeUrl}'>Cancelar inscrição</a>.</p>")}
                </body>
            </html>";
    }
}
