namespace KnowHowApi.Domain.Models
{
    public class PerfilProfessor
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public string Materia { get; set; }
        public decimal ValorHora { get; set; }
        public decimal Avaliacao { get; set; }
        public int TotalAvaliacoes { get; set; }
        public string Descricao { get; set; }
        public bool Disponivel { get; set; }
        public string AvatarVariante { get; set; }
    }
}
