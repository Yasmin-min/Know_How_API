namespace KnowHowApi.Domain.DTOs
{
    // Aula publicada (status "ativa") exibida ao aluno em GET /Professor/listar.
    // Materia, Valor, DuracaoMinutos e Nivel são opcionais no formulário e podem vir null.
    public class ProfessorAulaPublicadaDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string? Materia { get; set; }
        public decimal? Valor { get; set; }
        public int? DuracaoMinutos { get; set; }
        public string? Nivel { get; set; }
    }
}
