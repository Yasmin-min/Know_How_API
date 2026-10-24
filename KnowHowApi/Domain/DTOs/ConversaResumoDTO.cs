namespace KnowHowApi.Domain.DTOs
{
    public class ConversaResumoDTO
    {
        public string Id { get; set; } = string.Empty;
        public int ParticipanteId { get; set; }
        public string ParticipanteNome { get; set; } = string.Empty;
        public string? ParticipanteFotoUrl { get; set; }
        public string UltimaMensagem { get; set; } = string.Empty;
        public DateTime UltimaMensagemEm { get; set; }
        public int NaoLidas { get; set; }
    }
}
