namespace KnowHowApi.Domain.Models
{
    // Aluno adicionado manualmente (por e-mail) à lista do professor.
    public class ProfessorAluno
    {
        public int Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public int AlunoId { get; set; }
        public Usuario Aluno { get; set; }

        public DateTime CriadoEm { get; set; }
    }
}
