namespace KnowHowApi.Domain.DTOs
{
    // GET/PUT /Professor/alunos/{alunoId}/notas. Sem anotação: Texto = "" e AtualizadaEm = null.
    public class NotaAlunoDTO
    {
        public string Texto { get; set; } = string.Empty;
        public DateTime? AtualizadaEm { get; set; }
    }
}
