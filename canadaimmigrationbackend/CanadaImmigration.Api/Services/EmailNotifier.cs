using System.Net;
using System.Net.Mail;
using CanadaImmigration.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CanadaImmigration.Api.Services;

public class EmailNotifierConfig
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = "Canada Immigration Tracker";

    // URL base do site, usada para montar o link de descadastro nos e-mails.
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

        try
        {
            using var smtpClient = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
            {
                Credentials = new NetworkCredential(_config.SenderEmail, _config.SenderPassword),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_config.SenderEmail, _config.SenderDisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(recipientEmail);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            // Uma falha de envio para um assinante não deve derrubar o processamento dos demais.
            _logger.LogError(ex, "Falha ao enviar e-mail de notificação para {Email}", recipientEmail);
        }
    }

    private string BuildBody(ExpressEntryDraw draw, string unsubscribeToken)
    {
        var unsubscribeUrl = string.IsNullOrWhiteSpace(_config.SiteBaseUrl)
            ? null
            : $"{_config.SiteBaseUrl.TrimEnd('/')}/api/subscriptions/unsubscribe?token={unsubscribeToken}";

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
