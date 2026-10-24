namespace KnowHowApi.Domain.DTOs
{
    // Body de PUT /Professor/alunos/{alunoId}/notas. Texto vazio apaga a anotação.
    public class SalvarNotaAlunoRequestDTO
    {
        public string? Texto { get; set; }
    }
}
