using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailNotifierConfig
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; }
    public string SenderPassword { get; set; }
    public string RecipientEmail { get; set; }
}

public class EmailNotifier
{
    private readonly EmailNotifierConfig _config;

    public EmailNotifier(EmailNotifierConfig config)
    {
        _config = config;
    }

    public async Task SendUpdateNotificationAsync(string subject, string updateDetails)
    {
        try
        {
            using (var smtpClient = new SmtpClient(_config.SmtpServer, _config.SmtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_config.SenderEmail, _config.SenderPassword);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage(_config.SenderEmail, _config.RecipientEmail)
                {
                    Subject = $"[Canada Immigration Tracker] {subject} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    Body = GenerateEmailBody(updateDetails),
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("Email enviado com sucesso!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar email: {ex.Message}");
        }
    }

    private string GenerateEmailBody(string details)
    {
        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Nova Atualização Detectada</h2>
                    <p><strong>Horário:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                    <p><strong>Detalhes:</strong></p>
                    <pre style='background-color: #f4f4f4; padding: 10px; border-radius: 5px;'>{details}</pre>
                </body>
            </html>";
    }
}