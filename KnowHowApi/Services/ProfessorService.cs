using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services.Interfaces;
using KnowHowApi.Services.Utils;

namespace KnowHowApi.Services
{
    public class ProfessorService : IProfessorService
    {
        private const int TituloMaxLength = 100;
        private const int DescricaoMaxLength = 500;
        private const string StatusAtiva = "ativa";
        private const string StatusPausada = "pausada";
        private const string StatusRascunho = "rascunho";
        private static readonly HashSet<string> StatusPermitidosCriacao = new(StringComparer.Ordinal) { StatusAtiva, StatusRascunho };
        private static readonly HashSet<string> StatusPermitidosEdicao = new(StringComparer.Ordinal) { StatusAtiva, StatusPausada, StatusRascunho };
        private static readonly HashSet<(string De, string Para)> TransicoesDeStatusPermitidas = new()
        {
            (StatusAtiva, StatusPausada),
            (StatusPausada, StatusAtiva),
            (StatusRascunho, StatusAtiva)
        };
        private static readonly Dictionary<string, string> NomesStatus = new(StringComparer.Ordinal)
        {
            [StatusAtiva] = "ativa",
            [StatusPausada] = "inativa",
            [StatusRascunho] = "rascunho"
        };
        private static readonly HashSet<int> DuracoesPermitidas = new() { 30, 45, 60, 90, 120 };
        private static readonly HashSet<string> NiveisPermitidos = new(StringComparer.Ordinal)
        {
            "Iniciante", "Intermediário", "Avançado", "Todos os níveis"
        };

        private readonly IProfessorRepository _professorRepository;
        private readonly IAreaInteresseRepository _areaInteresseRepository;
        private readonly IAulaRepository _aulaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAulaAgendadaRepository _aulaAgendadaRepository;
        private readonly IAvaliacaoRepository _avaliacaoRepository;

        private const int DashboardMaxItens = 5;
        private const int DashboardMaxAulas = 3;

        public ProfessorService(IProfessorRepository professorRepository, IAreaInteresseRepository areaInteresseRepository, IAulaRepository aulaRepository, IUsuarioRepository usuarioRepository, IAulaAgendadaRepository aulaAgendadaRepository, IAvaliacaoRepository avaliacaoRepository)
        {
            _professorRepository = professorRepository;
            _areaInteresseRepository = areaInteresseRepository;
            _aulaRepository = aulaRepository;
            _usuarioRepository = usuarioRepository;
            _aulaAgendadaRepository = aulaAgendadaRepository;
            _avaliacaoRepository = avaliacaoRepository;
        }

        public async Task<List<ProfessorDTO>> ListarProfessores()
        {
            var perfis = await _professorRepository.ListarProfessores();
            if (perfis.Count == 0)
                return new List<ProfessorDTO>();

            // Já vêm ordenadas por CriadaEm decrescente; o GroupBy preserva a ordem.
            var aulasPorProfessor = (await _aulaRepository.ListarAtivasDosProfessores(perfis.Select(p => p.UsuarioId)))
                .GroupBy(a => a.ProfessorId)
                .ToDictionary(g => g.Key, g => g.Select(MapAulaPublicada).ToList());

            return perfis.Select(p =>
            {
                var dto = new ProfessorDTO(p);
                if (aulasPorProfessor.TryGetValue(p.UsuarioId, out var aulas))
                    dto.Aulas = aulas;
                return dto;
            }).ToList();
        }

        public async Task<ProfessorDTO?> ObterProfessorPublico(int professorId)
        {
            var perfil = await _professorRepository.GetPerfilPublico(professorId);
            if (perfil == null)
                return null;

            var dto = new ProfessorDTO(perfil);
            var aulas = await _aulaRepository.ListarAtivasDosProfessores(new[] { professorId });
            dto.Aulas = aulas.Select(MapAulaPublicada).ToList();
            return dto;
        }

        public async Task<ProfessorDashboardResponseDTO?> ObterDashboard(int usuarioId)
        {
            var perfil = await _professorRepository.GetPerfilProfessorByUsuarioId(usuarioId);
            if (perfil == null)
                return null;

            var agora = DateTime.UtcNow;
            // Semana de domingo a sábado, como na agenda do frontend.
            var inicioSemana = agora.Date.AddDays(-(int)agora.DayOfWeek);
            var fimSemana = inicioSemana.AddDays(7);

            var aulas = await _aulaRepository.ListarDoProfessor(usuarioId) ?? new List<Aula>();
            var totalAlunos = await ContarAlunosAtendidos(usuarioId);
            var agendadas = (await _aulaAgendadaRepository.ListarDoProfessor(usuarioId, inicioSemana, null) ?? new List<AulaAgendada>())
                .Where(a => a.Status == AulaAgendada.StatusAgendada)
                .ToList();
            var avaliacoes = await _avaliacaoRepository.ListarPorProfessor(usuarioId) ?? new List<Avaliacao>();

            return new ProfessorDashboardResponseDTO
            {
                Especialidade = perfil.Materia,
                Indicadores = new ProfessorDashboardIndicadoresDTO
                {
                    AulasAtivas = aulas.Count(a => a.Status == StatusAtiva),
                    Alunos = totalAlunos,
                    AvaliacaoMedia = perfil.Avaliacao,
                    AulasSemana = agendadas.Count(a => a.Inicio < fimSemana)
                },
                ProximasAulas = agendadas
                    .Where(a => a.Fim > agora)
                    .OrderBy(a => a.Inicio)
                    .Take(DashboardMaxItens)
                    .Select(a => new ProfessorDashboardProximaAulaDTO
                    {
                        Id = a.Id.ToString(),
                        Titulo = a.Titulo,
                        DataHoraInicio = a.Inicio,
                        DataHoraFim = a.Fim,
                        TotalAlunos = 1,
                        AlunoId = a.AlunoId,
                        AlunoNome = a.Aluno?.Nome ?? string.Empty,
                        Status = a.Status
                    })
                    .ToList(),
                AtividadesRecentes = avaliacoes
                    .Take(DashboardMaxItens)
                    .Select(a => new ProfessorDashboardAtividadeDTO
                    {
                        Id = $"avaliacao-{a.Id}",
                        Tipo = "avaliacao",
                        NomePessoa = a.Aluno?.Nome ?? string.Empty,
                        Titulo = "Nova avaliação",
                        Detalhe = string.IsNullOrWhiteSpace(a.Comentario) ? "Avaliou suas aulas." : a.Comentario,
                        DataHora = a.AtualizadaEm ?? a.CriadaEm,
                        AvaliacaoEstrelas = a.Nota
                    })
                    .ToList(),
                // Publicadas primeiro, depois as mais recentes.
                MinhasAulas = aulas
                    .OrderBy(a => a.Status == StatusAtiva ? 0 : 1)
                    .ThenByDescending(a => a.CriadaEm)
                    .Take(DashboardMaxAulas)
                    .Select(a => MapAulaParaResponse(a, totalAlunos))
                    .ToList()
            };
        }

        public async Task<ProfessorAulaResponseDTO> CriarAula(int professorId, CriarAulaRequestDTO request)
        {
            var dados = await ValidarDadosAula(professorId, request, StatusPermitidosCriacao, aulaIgnorada: null);

            var aula = new Aula
            {
                ProfessorId = professorId,
                Titulo = dados.Titulo,
                Descricao = dados.Descricao,
                AreaInteresseId = dados.AreaInteresse?.Id,
                Valor = request.Valor,
                DuracaoMinutos = request.DuracaoMinutos,
                Nivel = dados.Nivel,
                Status = request.Status,
                CriadaEm = DateTime.UtcNow
            };

            aula = await _aulaRepository.CriarAula(aula);
            aula.AreaInteresse = dados.AreaInteresse;

            return MapAulaParaResponse(aula, totalAlunos: 0);
        }

        public async Task<List<ProfessorAulaResponseDTO>> ListarAulas(int professorId)
        {
            var aulas = await _aulaRepository.ListarDoProfessor(professorId);
            if (aulas.Count == 0)
                return new List<ProfessorAulaResponseDTO>();

            var totalAlunos = await ContarAlunosAtendidos(professorId);
            return aulas.Select(a => MapAulaParaResponse(a, totalAlunos)).ToList();
        }

        public async Task<ProfessorAulaResponseDTO> ObterAula(int professorId, int aulaId)
        {
            var aula = await GetAulaDoProfessorOuFalhar(professorId, aulaId);
            return MapAulaParaResponse(aula, await ContarAlunosAtendidos(professorId));
        }

        public async Task<ProfessorAulaResponseDTO> AtualizarAula(int professorId, int aulaId, CriarAulaRequestDTO request)
        {
            var aula = await GetAulaDoProfessorOuFalhar(professorId, aulaId);
            var dados = await ValidarDadosAula(professorId, request, StatusPermitidosEdicao, aulaIgnorada: aula.Id);

            aula.Titulo = dados.Titulo;
            aula.Descricao = dados.Descricao;
            aula.AreaInteresseId = dados.AreaInteresse?.Id;
            aula.AreaInteresse = dados.AreaInteresse;
            aula.Valor = request.Valor;
            aula.DuracaoMinutos = request.DuracaoMinutos;
            aula.Nivel = dados.Nivel;
            aula.Status = request.Status;

            aula = await _aulaRepository.AtualizarAula(aula);
            return MapAulaParaResponse(aula, await ContarAlunosAtendidos(professorId));
        }

        public async Task<ProfessorAulaResponseDTO> AlterarStatusAula(int professorId, int aulaId, AlterarStatusAulaRequestDTO request)
        {
            var novoStatus = request?.Status?.Trim() ?? string.Empty;
            if (!StatusPermitidosEdicao.Contains(novoStatus))
                throw new DomainValidationException("Status inválido.");

            var aula = await GetAulaDoProfessorOuFalhar(professorId, aulaId);

            if (aula.Status != novoStatus)
            {
                if (!TransicoesDeStatusPermitidas.Contains((aula.Status, novoStatus)))
                    throw new DomainValidationException($"Não é possível mudar uma aula de {NomeStatus(aula.Status)} para {NomeStatus(novoStatus)}.");

                if (novoStatus == StatusAtiva && !TemCamposObrigatorios(aula))
                    throw new DomainValidationException("Preencha título, descrição, matéria e valor antes de publicar.");

                aula.Status = novoStatus;
                aula = await _aulaRepository.AtualizarAula(aula);
            }

            return MapAulaParaResponse(aula, await ContarAlunosAtendidos(professorId));
        }

        private async Task<Aula> GetAulaDoProfessorOuFalhar(int professorId, int aulaId)
        {
            var aula = await _aulaRepository.GetAulaDoProfessor(aulaId, professorId);
            if (aula == null)
                throw new NotFoundException("Aula não encontrada.");

            return aula;
        }

        private Task<int> ContarAlunosAtendidos(int professorId)
        {
            return _aulaAgendadaRepository.ContarAlunosAtendidos(professorId, DateTime.UtcNow);
        }

        private static bool TemCamposObrigatorios(Aula aula)
        {
            return !string.IsNullOrWhiteSpace(aula.Titulo)
                && !string.IsNullOrWhiteSpace(aula.Descricao)
                && aula.AreaInteresseId.HasValue
                && aula.Valor.HasValue;
        }

        private static string NomeStatus(string status)
        {
            return NomesStatus.TryGetValue(status, out var nome) ? nome : status;
        }

        private sealed record DadosAulaValidados(string Titulo, string Descricao, AreaInteresse? AreaInteresse, string? Nivel);

        // Validações compartilhadas entre criação e edição. Os campos obrigatórios valem para
        // "ativa" e "pausada" (uma aula inativa pode ser reativada a qualquer momento).
        private async Task<DadosAulaValidados> ValidarDadosAula(int professorId, CriarAulaRequestDTO request, HashSet<string> statusPermitidos, int? aulaIgnorada)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status) || !statusPermitidos.Contains(request.Status))
                throw new DomainValidationException("Status inválido.");

            var publicando = request.Status != StatusRascunho;

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
                var tituloDuplicado = await _aulaRepository.ExisteAulaComTitulo(professorId, titulo, aulaIgnorada);
                if (tituloDuplicado)
                    throw new DomainValidationException("Já existe uma aula com esse título.");
            }

            return new DadosAulaValidados(titulo, descricao, areaInteresse, string.IsNullOrEmpty(nivel) ? null : nivel);
        }

        private static ProfessorAulaResponseDTO MapAulaParaResponse(Aula aula, int totalAlunos)
        {
            return new ProfessorAulaResponseDTO
            {
                Id = aula.Id,
                Titulo = aula.Titulo,
                Descricao = aula.Descricao,
                Materia = aula.AreaInteresse?.Nome,
                Valor = aula.Valor,
                DuracaoMinutos = aula.DuracaoMinutos,
                Nivel = aula.Nivel,
                Status = aula.Status,
                TotalAlunos = totalAlunos,
                CriadaEm = aula.CriadaEm
            };
        }

        public async Task<ProfessorPerfilResponseDTO?> ObterPerfil(int usuarioId)
        {
            var perfil = await _professorRepository.GetPerfilProfessorByUsuarioId(usuarioId);
            if (perfil?.Usuario == null)
                return null;

            return MapPerfilParaResponse(perfil, perfil.Usuario);
        }

        public async Task<ProfessorPerfilResponseDTO?> AtualizarPerfil(int usuarioId, ProfessorPerfilRequestDTO request)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(usuarioId);
            var perfil = await _professorRepository.GetPerfilProfessorByUsuarioId(usuarioId);
            if (usuario == null || perfil == null)
                return null;

            if (request == null || string.IsNullOrWhiteSpace(request.Nome))
                throw new DomainValidationException("Informe o nome.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new DomainValidationException("Informe o e-mail.");

            if (string.IsNullOrWhiteSpace(request.Cpf) || request.Cpf.Length != 11 || !request.Cpf.All(char.IsDigit))
                throw new DomainValidationException("Informe um CPF válido, contendo somente os 11 dígitos.");

            if (request.DataNascimento == default || request.DataNascimento.Date >= DateTime.UtcNow.Date)
                throw new DomainValidationException("Informe uma data de nascimento válida.");

            if (string.IsNullOrWhiteSpace(request.AreaEspecialidade))
                throw new DomainValidationException("Informe a área de especialidade.");

            if (string.IsNullOrWhiteSpace(request.AnosExperiencia))
                throw new DomainValidationException("Informe os anos de experiência.");

            if (string.IsNullOrWhiteSpace(request.MiniBiografia))
                throw new DomainValidationException("Informe a mini biografia.");

            if (request.ValorHora < 0)
                throw new DomainValidationException("O valor da hora não pode ser negativo.");

            var emailDuplicado = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (emailDuplicado != null && emailDuplicado.Id != usuarioId)
                throw new DomainValidationException("Já existe um usuário cadastrado com este e-mail.");

            var cpfDuplicado = await _usuarioRepository.GetUsuarioByCpf(request.Cpf);
            if (cpfDuplicado != null && cpfDuplicado.Id != usuarioId)
                throw new DomainValidationException("Já existe um usuário cadastrado com este CPF.");

            var areaEspecialidade = await _areaInteresseRepository.GetAreaInteresseByNome(request.AreaEspecialidade);
            if (areaEspecialidade == null)
                throw new DomainValidationException("Área de especialidade inválida.");

            var fotoPerfil = perfil.FotoPerfil;
            var fotoPerfilContentType = perfil.FotoPerfilContentType;
            if (!string.IsNullOrEmpty(request.FotoPerfilBase64))
                (fotoPerfil, fotoPerfilContentType) = FotoPerfilConverter.ParseBase64(request.FotoPerfilBase64);

            usuario.Nome = request.Nome.Trim();
            usuario.Email = request.Email.Trim();
            usuario.Cpf = request.Cpf;
            usuario.DataNascimento = request.DataNascimento;
            await _usuarioRepository.Update(usuario);

            // Evita conflito no change tracker do EF: perfil.Usuario veio de uma consulta
            // AsNoTracking separada do `usuario` já rastreado acima.
            perfil.Usuario = null!;
            perfil.Materia = areaEspecialidade.Nome;
            perfil.AreaEspecialidadeId = areaEspecialidade.Id;
            perfil.AnosExperiencia = request.AnosExperiencia.Trim();
            perfil.Descricao = request.MiniBiografia.Trim();
            perfil.ValorHora = request.ValorHora;
            perfil.Disponivel = request.Disponivel;
            perfil.FotoPerfil = fotoPerfil;
            perfil.FotoPerfilContentType = fotoPerfilContentType;
            await _professorRepository.AtualizarPerfilProfessor(perfil);

            return MapPerfilParaResponse(perfil, usuario);
        }

        private static ProfessorAulaPublicadaDTO MapAulaPublicada(Aula aula)
        {
            return new ProfessorAulaPublicadaDTO
            {
                Id = aula.Id,
                Titulo = aula.Titulo,
                Descricao = aula.Descricao,
                Materia = aula.AreaInteresse?.Nome,
                Valor = aula.Valor,
                DuracaoMinutos = aula.DuracaoMinutos,
                Nivel = aula.Nivel
            };
        }

        private static ProfessorPerfilResponseDTO MapPerfilParaResponse(PerfilProfessor perfil, Usuario usuario)
        {
            return new ProfessorPerfilResponseDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Cpf = usuario.Cpf,
                DataNascimento = usuario.DataNascimento,
                AreaEspecialidade = perfil.Materia,
                AnosExperiencia = perfil.AnosExperiencia,
                MiniBiografia = perfil.Descricao,
                ValorHora = perfil.ValorHora,
                Disponivel = perfil.Disponivel,
                FotoPerfilBase64 = FotoPerfilConverter.ToDataUri(perfil.FotoPerfil, perfil.FotoPerfilContentType)
            };
        }
    }
}
