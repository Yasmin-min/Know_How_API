using System.Text.Json.Serialization;

namespace KnowHowApi.Domain.DTOs
{
    // GET /Chamada/{aulaId}: estado da sala de vídeo de uma aula agendada para quem participa dela.
    public class ChamadaAcessoDTO
    {
        public const string SituacaoAguardando = "aguardando";
        public const string SituacaoAberta = "aberta";
        public const string SituacaoEncerrada = "encerrada";
        public const string SituacaoCancelada = "cancelada";

        public string AulaId { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        // Janela em que a sala aceita participantes.
        public DateTime AbreEm { get; set; }
        public DateTime FechaEm { get; set; }
        public string Situacao { get; set; } = SituacaoAguardando;
        public bool SouProfessor { get; set; }
        // A outra pessoa da aula (o aluno, para o professor; o professor, para o aluno).
        public ParticipanteChamadaDTO Participante { get; set; } = new();
        // Servidores STUN/TURN no formato RTCIceServer. Só vem preenchido com a sala aberta.
        public List<IceServerDTO> IceServers { get; set; } = new();
    }

    public class ParticipanteChamadaDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class IceServerDTO
    {
        public List<string> Urls { get; set; } = new();
        // STUN não tem credenciais: os campos ficam fora do JSON (o navegador recusa null).
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Username { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Credential { get; set; }
    }

    // Retorno do método EntrarNaChamada do hub.
    public class EntradaChamadaDTO
    {
        public bool OutroPresente { get; set; }
    }
}
