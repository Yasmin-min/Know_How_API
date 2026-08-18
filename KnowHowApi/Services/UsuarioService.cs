using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using KnowHowApi.Domain.Configurations;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace KnowHowApi.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAreaInteresseRepository _areaInteresseRepository;
        private readonly IProfessorRepository _professorRepository;
        private readonly ICryptography _cryptography;
        private readonly JWTSettings _jwtSettings;

        public UsuarioService(IUsuarioRepository usuarioRepository, IAreaInteresseRepository areaInteresseRepository, IProfessorRepository professorRepository, ICryptography cryptography, JWTSettings jwtSettings)
        {
            _usuarioRepository = usuarioRepository;
            _areaInteresseRepository = areaInteresseRepository;
            _professorRepository = professorRepository;
            _cryptography = cryptography;
            _jwtSettings = jwtSettings;
        }

        public async Task<Usuario> Registrar(RegisterUsuarioDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Senha))
                throw new BadHttpRequestException("Informe nome, e-mail e senha");

            if (request.DataNascimento == default || request.DataNascimento.Date >= DateTime.UtcNow.Date)
                throw new BadHttpRequestException("Informe uma data de nascimento válida.");

            string? cpf = null;
            AreaInteresse? areaEspecialidade = null;
            byte[]? fotoPerfil = null;
            string? fotoPerfilContentType = null;
            if (request.TipoUsuario == TipoUsuario.Professor)
            {
                if (string.IsNullOrEmpty(request.Cpf) || request.Cpf.Length != 11 || !request.Cpf.All(char.IsDigit))
                    throw new BadHttpRequestException("Informe um CPF válido, contendo somente os 11 dígitos.");

                cpf = request.Cpf;

                var cpfExistente = await _usuarioRepository.GetUsuarioByCpf(cpf);
                if (cpfExistente != null)
                    throw new BadHttpRequestException("Já existe um usuário cadastrado com este CPF.");

                if (string.IsNullOrWhiteSpace(request.AreaEspecialidade))
                    throw new BadHttpRequestException("Informe a área de especialidade.");

                areaEspecialidade = await _areaInteresseRepository.GetAreaInteresseByNome(request.AreaEspecialidade);
                if (areaEspecialidade == null)
                    throw new BadHttpRequestException("Área de especialidade inválida.");

                if (string.IsNullOrWhiteSpace(request.AnosExperiencia))
                    throw new BadHttpRequestException("Informe os anos de experiência.");

                if (string.IsNullOrWhiteSpace(request.MiniBiografia))
                    throw new BadHttpRequestException("Informe a mini biografia.");

                if (!string.IsNullOrEmpty(request.FotoPerfilBase64))
                    (fotoPerfil, fotoPerfilContentType) = ParseFotoPerfil(request.FotoPerfilBase64);
            }

            int? areaInteresseId = null;
            if (request.TipoUsuario == TipoUsuario.Aluno)
            {
                if (string.IsNullOrWhiteSpace(request.AreaInteresse))
                    throw new BadHttpRequestException("Informe a área de interesse.");

                var areaInteresse = await _areaInteresseRepository.GetAreaInteresseByNome(request.AreaInteresse);
                if (areaInteresse == null)
                    throw new BadHttpRequestException("Área de interesse inválida.");

                areaInteresseId = areaInteresse.Id;
            }

            var existente = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (existente != null)
                throw new BadHttpRequestException("Já existe um usuário cadastrado com este e-mail.");

            var usuario = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = _cryptography.Crypt(request.Senha),
                TipoUsuario = request.TipoUsuario,
                DataNascimento = request.DataNascimento,
                Cpf = cpf,
                AreaInteresseId = areaInteresseId
            };

            usuario = await _usuarioRepository.CriarUsuario(usuario);

            if (request.TipoUsuario == TipoUsuario.Professor)
            {
                var perfil = new PerfilProfessor
                {
                    UsuarioId = usuario.Id,
                    Materia = areaEspecialidade!.Nome,
                    AreaEspecialidadeId = areaEspecialidade.Id,
                    AnosExperiencia = request.AnosExperiencia,
                    Descricao = request.MiniBiografia!,
                    FotoPerfil = fotoPerfil,
                    FotoPerfilContentType = fotoPerfilContentType,
                    ValorHora = 0,
                    Avaliacao = 0,
                    TotalAvaliacoes = 0,
                    Disponivel = false,
                    AvatarVariante = string.Empty
                };

                await _professorRepository.CriarPerfilProfessor(perfil);
            }

            return usuario;
        }

        private static (byte[] bytes, string? contentType) ParseFotoPerfil(string fotoPerfilBase64)
        {
            string? contentType = null;
            var base64 = fotoPerfilBase64;

            if (fotoPerfilBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var separatorIndex = fotoPerfilBase64.IndexOf(',');
                if (separatorIndex < 0)
                    throw new BadHttpRequestException("Foto de perfil inválida.");

                var header = fotoPerfilBase64[5..separatorIndex];
                var contentTypeEnd = header.IndexOf(';');
                contentType = contentTypeEnd >= 0 ? header[..contentTypeEnd] : header;
                base64 = fotoPerfilBase64[(separatorIndex + 1)..];
            }

            try
            {
                return (Convert.FromBase64String(base64), contentType);
            }
            catch (FormatException)
            {
                throw new BadHttpRequestException("Foto de perfil inválida.");
            }
        }

        public async Task<LoginResponseDTO> ValidateLogin(LoginRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Senha))
                throw new BadHttpRequestException("Informe e-mail e senha");

            var usuario = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (usuario == null || !_cryptography.Verify(request.Senha, usuario.SenhaHash))
                throw new BadHttpRequestException("E-mail ou senha inválidos.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDTO(usuario, tokenString);
        }

        public async Task<List<Usuario>> ListarTodosOsUsuarios()
        {
            return await _usuarioRepository.GetAllUsers();
        }

        public async Task<Usuario> EditarUsuario(int usuarioId, UsuarioDTO userEditDto)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(usuarioId);
            if (usuario == null)
                throw new BadHttpRequestException("Usuário não encontrado.");

            usuario.Nome = userEditDto.Nome;
            usuario.Email = userEditDto.Email;
            usuario.TipoUsuario = userEditDto.TipoUsuario;
            usuario.DataNascimento = userEditDto.DataNascimento;
            usuario.Cpf = userEditDto.Cpf;

            return await _usuarioRepository.Update(usuario);
        }
    }
}
