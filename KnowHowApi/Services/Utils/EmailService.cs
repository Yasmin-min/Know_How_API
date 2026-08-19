using System.Net;
using System.Net.Mail;
using KnowHowApi.Domain.Configurations;
using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services.Utils
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(SmtpSettings smtpSettings, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings;
            _logger = logger;
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            if (string.IsNullOrWhiteSpace(_smtpSettings.Host))
            {
                _logger.LogWarning(
                    "Smtp não configurado. E-mail não enviado. Destinatario: {Destinatario}, Assunto: {Assunto}, Corpo: {Corpo}",
                    destinatario, assunto, corpo);
                return;
            }

            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            using var message = new MailMessage(_smtpSettings.From, destinatario, assunto, corpo);
            await client.SendMailAsync(message);
        }
    }
}
