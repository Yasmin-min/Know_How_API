namespace KnowHowApi.Domain.Models
{
    public class AulaAgendada
    {
        public const string StatusAgendada = "agendada";
        public const string StatusCancelada = "cancelada";

        public Guid Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public int AlunoId { get; set; }
        public Usuario Aluno { get; set; }

        // Aula do catálogo (Aulas) a que este agendamento se refere, quando informada.
        public int? AulaId { get; set; }
        public Aula? Aula { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;

        // Sempre em UTC. Fim = Inicio + DuracaoMinutos, gravado para simplificar as
        // consultas de sobreposição (aula.Inicio < fim && aula.Fim > inicio).
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public int DuracaoMinutos { get; set; }

        public string Status { get; set; } = StatusAgendada;
        public DateTime CriadaEm { get; set; }
        public DateTime? CanceladaEm { get; set; }
    }
}
