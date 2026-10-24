namespace KnowHowApi.Domain.Models
{
    public class Mensagem
    {
        public int Id { get; set; }

        public int ConversaId { get; set; }
        public Conversa Conversa { get; set; }

        public int RemetenteId { get; set; }
        public Usuario Remetente { get; set; }

        public string Texto { get; set; }
        public DateTime EnviadaEm { get; set; }
        public bool Lida { get; set; }
    }
}
