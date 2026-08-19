namespace KnowHowApi.Services.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string destinatario, string assunto, string corpo);
    }
}
