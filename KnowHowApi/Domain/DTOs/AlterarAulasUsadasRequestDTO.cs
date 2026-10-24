namespace KnowHowApi.Domain.DTOs
{
    // Body de PUT /Professor/contratacoes/{id}/aulas-usadas. Valor absoluto, não incremento.
    public class AlterarAulasUsadasRequestDTO
    {
        public int? AulasUsadas { get; set; }
    }
}
