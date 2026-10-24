namespace KnowHowApi.Domain.Models
{
    public class Conversa
    {
        public int Id { get; set; }

        public int AlunoId { get; set; }
        public Usuario Aluno { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public DateTime CriadaEm { get; set; }
    }
}
