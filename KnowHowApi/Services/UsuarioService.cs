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
        private readonly ICryptography _cryptography;
        private readonly JWTSettings _jwtSettings;

        public UsuarioService(IUsuarioRepository usuarioRepository, ICryptography cryptography, JWTSettings jwtSettings)
        {
            _usuarioRepository = usuarioRepository;
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
            if (request.TipoUsuario == TipoUsuario.Professor)
            {
                if (string.IsNullOrEmpty(request.Cpf) || request.Cpf.Length != 11 || !request.Cpf.All(char.IsDigit))
                    throw new BadHttpRequestException("Informe um CPF válido, contendo somente os 11 dígitos.");

                cpf = request.Cpf;

                var cpfExistente = await _usuarioRepository.GetUsuarioByCpf(cpf);
                if (cpfExistente != null)
                    throw new BadHttpRequestException("Já existe um usuário cadastrado com este CPF.");
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
                Cpf = cpf
            };

            return await _usuarioRepository.CriarUsuario(usuario);
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
