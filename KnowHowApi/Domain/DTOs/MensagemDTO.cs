namespace KnowHowApi.Domain.DTOs
{
    public class MensagemDTO
    {
        public string Id { get; set; } = string.Empty;
        public int RemetenteId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public DateTime EnviadaEm { get; set; }
        public bool Lida { get; set; }
    }
}
