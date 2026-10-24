namespace KnowHowApi.Domain.Models
{
    // Avaliação de um aluno sobre um professor. Uma por par aluno/professor; editar substitui a anterior.
    public class Avaliacao
    {
        public int Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public int AlunoId { get; set; }
        public Usuario Aluno { get; set; }

        // 1 a 5 estrelas.
        public int Nota { get; set; }
        public string Comentario { get; set; } = string.Empty;

        public DateTime CriadaEm { get; set; }
        public DateTime? AtualizadaEm { get; set; }
    }
}
