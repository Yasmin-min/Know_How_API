using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.DTOs
{
    public class ProfessorDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Materia { get; set; }
        public decimal ValorHora { get; set; }
        public decimal Avaliacao { get; set; }
        public int TotalAvaliacoes { get; set; }
        public string Descricao { get; set; }
        public bool Disponivel { get; set; }
        public string AvatarVariante { get; set; }

        public ProfessorDTO()
        {
        }

        public ProfessorDTO(PerfilProfessor perfil)
        {
            Id = perfil.UsuarioId;
            Nome = perfil.Usuario.Nome;
            Materia = perfil.Materia;
            ValorHora = perfil.ValorHora;
            Avaliacao = perfil.Avaliacao;
            TotalAvaliacoes = perfil.TotalAvaliacoes;
            Descricao = perfil.Descricao;
            Disponivel = perfil.Disponivel;
            AvatarVariante = perfil.AvatarVariante;
        }
    }
}
