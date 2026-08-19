using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services
{
    public class ProfessorService : IProfessorService
    {
        private const int TituloMaxLength = 100;
        private const int DescricaoMaxLength = 500;
        private static readonly HashSet<string> StatusPermitidosCriacao = new(StringComparer.Ordinal) { "ativa", "rascunho" };
        private static readonly HashSet<int> DuracoesPermitidas = new() { 30, 45, 60, 90, 120 };
        private static readonly HashSet<string> NiveisPermitidos = new(StringComparer.Ordinal)
        {
            "Iniciante", "Intermediário", "Avançado", "Todos os níveis"
        };

        private readonly IProfessorRepository _professorRepository;
        private readonly IAreaInteresseRepository _areaInteresseRepository;
        private readonly IAulaRepository _aulaRepository;

        public ProfessorService(IProfessorRepository professorRepository, IAreaInteresseRepository areaInteresseRepository, IAulaRepository aulaRepository)
        {
            _professorRepository = professorRepository;
            _areaInteresseRepository = areaInteresseRepository;
            _aulaRepository = aulaRepository;
        }

        public async Task<List<ProfessorDTO>> ListarProfessores()
        {
            var perfis = await _professorRepository.ListarProfessores();
            return perfis.Select(p => new ProfessorDTO(p)).ToList();
        }

        public async Task<ProfessorDashboardResponseDTO?> ObterDashboard(int usuarioId)
        {
            var perfil = await _professorRepository.GetPerfilProfessorByUsuarioId(usuarioId);
            if (perfil == null)
                return null;

            return new ProfessorDashboardResponseDTO
            {
                Especialidade = perfil.Materia,
                Indicadores = new ProfessorDashboardIndicadoresDTO
                {
                    AulasAtivas = 0,
                    Alunos = 0,
                    AvaliacaoMedia = perfil.Avaliacao,
                    AulasSemana = 0
                },
                ProximasAulas = new List<ProfessorDashboardProximaAulaDTO>(),
                AtividadesRecentes = new List<ProfessorDashboardAtividadeDTO>(),
                MinhasAulas = new List<ProfessorAulaResponseDTO>()
            };
        }

        public async Task<ProfessorAulaResponseDTO> CriarAula(int professorId, CriarAulaRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status) || !StatusPermitidosCriacao.Contains(request.Status))
                throw new DomainValidationException("Status inválido.");

            var publicando = request.Status == "ativa";

            var titulo = (request.Titulo ?? string.Empty).Trim();
            var descricao = (request.Descricao ?? string.Empty).Trim();
            var materia = request.Materia?.Trim();
            var nivel = request.Nivel?.Trim();

            if (titulo.Length > TituloMaxLength)
                throw new DomainValidationException($"O título deve ter no máximo {TituloMaxLength} caracteres.");

            if (descricao.Length > DescricaoMaxLength)
                throw new DomainValidationException($"A descrição deve ter no máximo {DescricaoMaxLength} caracteres.");

            if (request.Valor.HasValue && request.Valor.Value <= 0)
                throw new DomainValidationException("O valor deve ser maior que zero.");

            if (request.DuracaoMinutos.HasValue && !DuracoesPermitidas.Contains(request.DuracaoMinutos.Value))
                throw new DomainValidationException("Duração inválida.");

            if (!string.IsNullOrEmpty(nivel) && !NiveisPermitidos.Contains(nivel))
                throw new DomainValidationException("Nível inválido.");

            if (publicando)
            {
                if (string.IsNullOrEmpty(titulo))
                    throw new DomainValidationException("O título da aula é obrigatório.");

                if (string.IsNullOrEmpty(descricao))
                    throw new DomainValidationException("A descrição da aula é obrigatória.");

                if (string.IsNullOrEmpty(materia))
                    throw new DomainValidationException("A matéria da aula é obrigatória.");

                if (!request.Valor.HasValue)
                    throw new DomainValidationException("O valor da aula é obrigatório.");
            }

            AreaInteresse? areaInteresse = null;
            if (!string.IsNullOrEmpty(materia))
            {
                areaInteresse = await _areaInteresseRepository.GetAreaInteresseByNome(materia);
                if (areaInteresse == null)
                    throw new DomainValidationException("Área de interesse inválida.");
            }

            if (!string.IsNullOrEmpty(titulo))
            {
                var tituloDuplicado = await _aulaRepository.ExisteAulaComTitulo(professorId, titulo);
                if (tituloDuplicado)
                    throw new DomainValidationException("Já existe uma aula com esse título.");
            }

            var aula = new Aula
            {
                ProfessorId = professorId,
                Titulo = titulo,
                Descricao = descricao,
                AreaInteresseId = areaInteresse?.Id,
                Valor = request.Valor,
                DuracaoMinutos = request.DuracaoMinutos,
                Nivel = nivel,
                Status = request.Status,
                CriadaEm = DateTime.UtcNow
            };

            aula = await _aulaRepository.CriarAula(aula);

            return new ProfessorAulaResponseDTO
            {
                Id = aula.Id,
                Titulo = aula.Titulo,
                Descricao = aula.Descricao,
                Materia = areaInteresse?.Nome,
                Valor = aula.Valor,
                DuracaoMinutos = aula.DuracaoMinutos,
                Nivel = aula.Nivel,
                Status = aula.Status,
                TotalAlunos = 0,
                CriadaEm = aula.CriadaEm
            };
        }
    }
}
