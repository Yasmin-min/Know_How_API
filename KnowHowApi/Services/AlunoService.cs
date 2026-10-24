using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services.Interfaces;
using KnowHowApi.Services.Utils;

namespace KnowHowApi.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAreaInteresseRepository _areaInteresseRepository;

        public AlunoService(IUsuarioRepository usuarioRepository, IAreaInteresseRepository areaInteresseRepository)
        {
            _usuarioRepository = usuarioRepository;
            _areaInteresseRepository = areaInteresseRepository;
        }

        public async Task<AlunoPerfilResponseDTO?> ObterPerfil(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(usuarioId);
            if (usuario == null)
                return null;

            return MapParaResponse(usuario, usuario.AreaInteresse?.Nome);
        }

        public async Task<AlunoPerfilResponseDTO?> AtualizarPerfil(int usuarioId, AlunoPerfilRequestDTO request)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(usuarioId);
            if (usuario == null)
                return null;

            if (request == null || string.IsNullOrWhiteSpace(request.Nome))
                throw new DomainValidationException("Informe o nome.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new DomainValidationException("Informe o e-mail.");

            if (request.DataNascimento == default || request.DataNascimento.Date >= DateTime.UtcNow.Date)
                throw new DomainValidationException("Informe uma data de nascimento válida.");

            if (string.IsNullOrWhiteSpace(request.AreaInteresse))
                throw new DomainValidationException("Informe a área de interesse.");

            var emailDuplicado = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (emailDuplicado != null && emailDuplicado.Id != usuarioId)
                throw new DomainValidationException("Já existe um usuário cadastrado com este e-mail.");

            var areaInteresse = await _areaInteresseRepository.GetAreaInteresseByNome(request.AreaInteresse);
            if (areaInteresse == null)
                throw new DomainValidationException("Área de interesse inválida.");

            var fotoPerfil = usuario.FotoPerfil;
            var fotoPerfilContentType = usuario.FotoPerfilContentType;
            if (!string.IsNullOrEmpty(request.FotoPerfilBase64))
                (fotoPerfil, fotoPerfilContentType) = FotoPerfilConverter.ParseBase64(request.FotoPerfilBase64);

            usuario.Nome = request.Nome.Trim();
            usuario.Email = request.Email.Trim();
            usuario.DataNascimento = request.DataNascimento;
            usuario.AreaInteresseId = areaInteresse.Id;
            usuario.FotoPerfil = fotoPerfil;
            usuario.FotoPerfilContentType = fotoPerfilContentType;
            await _usuarioRepository.Update(usuario);

            return MapParaResponse(usuario, areaInteresse.Nome);
        }

        private static AlunoPerfilResponseDTO MapParaResponse(Usuario usuario, string? areaInteresseNome)
        {
            return new AlunoPerfilResponseDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                DataNascimento = usuario.DataNascimento,
                AreaInteresse = areaInteresseNome,
                FotoPerfilBase64 = FotoPerfilConverter.ToDataUri(usuario.FotoPerfil, usuario.FotoPerfilContentType)
            };
        }
    }
}
