namespace KnowHowApi.Domain.Models
{
    // Anotação privada do professor sobre um aluno. Só o professor que escreveu pode ler.
    public class NotaAluno
    {
        public int Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public int AlunoId { get; set; }
        public Usuario Aluno { get; set; }

        public string Texto { get; set; } = string.Empty;
        public DateTime AtualizadaEm { get; set; }
    }
}
