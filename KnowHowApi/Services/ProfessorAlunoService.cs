using System.Net.Mail;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services
{
    public class ProfessorAlunoService : IProfessorAlunoService
    {
        private const int NotaMaxLength = 2000;

        private readonly IProfessorAlunoRepository _professorAlunoRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public ProfessorAlunoService(IProfessorAlunoRepository professorAlunoRepository, IUsuarioRepository usuarioRepository)
        {
            _professorAlunoRepository = professorAlunoRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<List<AlunoResumoDTO>> ListarAlunos(int professorId)
        {
            var alunos = await _professorAlunoRepository.ListarAlunosDoProfessor(professorId);
            return alunos.Select(a => new AlunoResumoDTO { Id = a.Id, Nome = a.Nome }).ToList();
        }

        public async Task<AlunoResumoDTO> AdicionarAlunoPorEmail(int professorId, AdicionarAlunoRequestDTO request)
        {
            var email = (request?.Email ?? string.Empty).Trim().ToLowerInvariant();
            if (!EmailValido(email))
                throw new DomainValidationException("Informe um e-mail válido.");

            var usuario = await _usuarioRepository.GetUsuarioByEmailIgnorandoCaixa(email);
            if (usuario == null)
                throw new NotFoundException("Nenhum aluno cadastrado com esse e-mail.");

            if (usuario.TipoUsuario != TipoUsuario.Aluno)
                throw new DomainValidationException("Esse e-mail não pertence a um aluno.");

            // Idempotente: se o vínculo já existe, não faz nada.
            await _professorAlunoRepository.AdicionarAluno(professorId, usuario.Id);

            return new AlunoResumoDTO { Id = usuario.Id, Nome = usuario.Nome };
        }

        public async Task<NotaAlunoDTO> ObterNota(int professorId, int alunoId)
        {
            await GarantirAlunoDoProfessor(professorId, alunoId);

            var nota = await _professorAlunoRepository.GetNota(professorId, alunoId);
            return nota == null
                ? new NotaAlunoDTO()
                : new NotaAlunoDTO { Texto = nota.Texto, AtualizadaEm = nota.AtualizadaEm };
        }

        public async Task<NotaAlunoDTO> SalvarNota(int professorId, int alunoId, SalvarNotaAlunoRequestDTO request)
        {
            await GarantirAlunoDoProfessor(professorId, alunoId);

            var texto = (request?.Texto ?? string.Empty).Trim();
            if (texto.Length > NotaMaxLength)
                throw new DomainValidationException($"A anotação deve ter no máximo {NotaMaxLength} caracteres.");

            if (texto.Length == 0)
            {
                await _professorAlunoRepository.RemoverNota(professorId, alunoId);
                return new NotaAlunoDTO();
            }

            var nota = await _professorAlunoRepository.SalvarNota(professorId, alunoId, texto, DateTime.UtcNow);
            return new NotaAlunoDTO { Texto = nota.Texto, AtualizadaEm = nota.AtualizadaEm };
        }

        private async Task GarantirAlunoDoProfessor(int professorId, int alunoId)
        {
            var aluno = await _professorAlunoRepository.GetAlunoDoProfessor(professorId, alunoId);
            if (aluno == null)
                throw new NotFoundException("Aluno não encontrado.");
        }

        private static bool EmailValido(string email)
        {
            return email.Length > 0
                && MailAddress.TryCreate(email, out var endereco)
                && endereco.Address == email
                && endereco.Host.Contains('.');
        }
    }
}
