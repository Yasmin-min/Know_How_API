namespace KnowHowApi.Domain.DTOs
{
    // Item de GET /Professor/alunos. Id é o id do usuário aluno (mesmo participanteId do chat).
    public class AlunoResumoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
}
