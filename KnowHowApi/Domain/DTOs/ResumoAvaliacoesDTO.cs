namespace KnowHowApi.Domain.DTOs
{
    public class ResumoAvaliacoesDTO
    {
        public decimal Media { get; set; }
        public int Total { get; set; }
        // Quantidade por nota: índice 0 = 1 estrela ... índice 4 = 5 estrelas.
        public int[] Distribuicao { get; set; } = new int[5];
    }
}
