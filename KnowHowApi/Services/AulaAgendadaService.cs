using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services
{
    public class AulaAgendadaService : IAulaAgendadaService
    {
        private const int TituloMaxLength = 100;
        private const int DescricaoMaxLength = 500;
        private const int DuracaoMinimaMinutos = 15;
        private const int DuracaoMaximaMinutos = 480;

        private readonly IAulaAgendadaRepository _aulaAgendadaRepository;
        private readonly IProfessorAlunoRepository _professorAlunoRepository;
        private readonly IAulaRepository _aulaRepository;

        public AulaAgendadaService(IAulaAgendadaRepository aulaAgendadaRepository, IProfessorAlunoRepository professorAlunoRepository, IAulaRepository aulaRepository)
        {
            _aulaAgendadaRepository = aulaAgendadaRepository;
            _professorAlunoRepository = professorAlunoRepository;
            _aulaRepository = aulaRepository;
        }

        public async Task<List<AulaAgendadaDTO>> ListarAulas(int professorId, DateTime? inicio, DateTime? fim)
        {
            var inicioUtc = inicio.HasValue ? ParaUtc(inicio.Value) : (DateTime?)null;
            var fimUtc = fim.HasValue ? ParaUtc(fim.Value) : (DateTime?)null;

            if (inicioUtc.HasValue && fimUtc.HasValue && fimUtc.Value <= inicioUtc.Value)
                throw new DomainValidationException("O fim do período deve ser posterior ao início.");

            var aulas = await _aulaAgendadaRepository.ListarDoProfessor(professorId, inicioUtc, fimUtc);
            return aulas.Select(MapParaDTO).ToList();
        }

        public async Task<List<AulaAgendadaAlunoDTO>> ListarAulasDoAluno(int alunoId, DateTime? inicio, DateTime? fim)
        {
            var inicioUtc = inicio.HasValue ? ParaUtc(inicio.Value) : (DateTime?)null;
            var fimUtc = fim.HasValue ? ParaUtc(fim.Value) : (DateTime?)null;

            if (inicioUtc.HasValue && fimUtc.HasValue && fimUtc.Value <= inicioUtc.Value)
                throw new DomainValidationException("O fim do período deve ser posterior ao início.");

            var aulas = await _aulaAgendadaRepository.ListarDoAluno(alunoId, inicioUtc, fimUtc);
            return aulas.Select(aula => new AulaAgendadaAlunoDTO
            {
                Id = aula.Id.ToString(),
                Titulo = aula.Titulo,
                Descricao = aula.Descricao ?? string.Empty,
                ProfessorId = aula.ProfessorId,
                ProfessorNome = aula.Professor?.Nome ?? string.Empty,
                Inicio = aula.Inicio,
                DuracaoMinutos = aula.DuracaoMinutos,
                Status = aula.Status
            }).ToList();
        }

        public async Task<AulaAgendadaDTO> CriarAula(int professorId, CriarAulaAgendadaRequestDTO request)
        {
            var titulo = (request?.Titulo ?? string.Empty).Trim();
            var descricao = (request?.Descricao ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(titulo))
                throw new DomainValidationException("O título da aula é obrigatório.");

            if (titulo.Length > TituloMaxLength)
                throw new DomainValidationException($"O título deve ter no máximo {TituloMaxLength} caracteres.");

            if (descricao.Length > DescricaoMaxLength)
                throw new DomainValidationException($"A descrição deve ter no máximo {DescricaoMaxLength} caracteres.");

            if (request!.AlunoId is not int alunoId)
                throw new DomainValidationException("Selecione o aluno.");

            if (request.Inicio is not DateTime inicioInformado)
                throw new DomainValidationException("Informe a data e o horário de início.");

            if (request.DuracaoMinutos is not int duracao || duracao < DuracaoMinimaMinutos || duracao > DuracaoMaximaMinutos)
                throw new DomainValidationException($"A duração deve ser entre {DuracaoMinimaMinutos} e {DuracaoMaximaMinutos} minutos.");

            var agora = DateTime.UtcNow;
            var inicio = ParaUtc(inicioInformado);
            if (inicio < agora)
                throw new DomainValidationException("Não é possível agendar uma aula no passado.");

            var fim = inicio.AddMinutes(duracao);

            var aluno = await _professorAlunoRepository.GetAlunoDoProfessor(professorId, alunoId);
            if (aluno == null)
                throw new DomainValidationException("Aluno não encontrado entre os seus alunos.");

            if (request.AulaId is int aulaId && await _aulaRepository.GetAulaDoProfessor(aulaId, professorId) == null)
                throw new DomainValidationException("Aula não encontrada no seu catálogo.");

            var conflito = await _aulaAgendadaRepository.GetConflitoProfessor(professorId, inicio, fim);
            if (conflito != null)
                throw new ConflictException($"Já existe uma aula neste horário: \"{conflito.Titulo}\" com {conflito.Aluno?.Nome}.");

            if (await _aulaAgendadaRepository.ExisteConflitoAluno(alunoId, inicio, fim))
                throw new ConflictException($"{aluno.Nome} já tem uma aula agendada neste horário.");

            var aula = new AulaAgendada
            {
                Id = Guid.NewGuid(),
                ProfessorId = professorId,
                AlunoId = alunoId,
                AulaId = request.AulaId,
                Titulo = titulo,
                Descricao = descricao,
                Inicio = inicio,
                Fim = fim,
                DuracaoMinutos = duracao,
                Status = AulaAgendada.StatusAgendada,
                CriadaEm = agora
            };

            aula = await _aulaAgendadaRepository.Criar(aula);
            aula.Aluno = aluno;

            return MapParaDTO(aula);
        }

        public async Task<AulaAgendadaDTO> EditarAula(int professorId, Guid aulaId, EditarAulaAgendadaRequestDTO request)
        {
            var aula = await _aulaAgendadaRepository.GetByIdDoProfessor(aulaId, professorId);
            if (aula == null)
                throw new NotFoundException("Aula não encontrada.");

            var agora = DateTime.UtcNow;
            if (aula.Status == AulaAgendada.StatusCancelada)
                throw new DomainValidationException("Uma aula cancelada não pode ser editada.");

            if (aula.Fim <= agora)
                throw new DomainValidationException("Não é possível editar uma aula que já aconteceu.");

            var titulo = (request?.Titulo ?? string.Empty).Trim();
            var descricao = (request?.Descricao ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(titulo))
                throw new DomainValidationException("O título da aula é obrigatório.");

            if (titulo.Length > TituloMaxLength)
                throw new DomainValidationException($"O título deve ter no máximo {TituloMaxLength} caracteres.");

            if (descricao.Length > DescricaoMaxLength)
                throw new DomainValidationException($"A descrição deve ter no máximo {DescricaoMaxLength} caracteres.");

            if (request!.Inicio is not DateTime inicioInformado)
                throw new DomainValidationException("Informe a data e o horário de início.");

            if (request.DuracaoMinutos is not int duracao || duracao < DuracaoMinimaMinutos || duracao > DuracaoMaximaMinutos)
                throw new DomainValidationException($"A duração deve ser entre {DuracaoMinimaMinutos} e {DuracaoMaximaMinutos} minutos.");

            var inicio = ParaUtc(inicioInformado);
            // Mudar só o título ou a descrição de uma aula em andamento é permitido; remarcar para o passado não.
            if (inicio != aula.Inicio && inicio < agora)
                throw new DomainValidationException("Não é possível remarcar uma aula para o passado.");

            var fim = inicio.AddMinutes(duracao);

            if (request.AulaId is int catalogoId && await _aulaRepository.GetAulaDoProfessor(catalogoId, professorId) == null)
                throw new DomainValidationException("Aula não encontrada no seu catálogo.");

            var conflito = await _aulaAgendadaRepository.GetConflitoProfessor(professorId, inicio, fim, aula.Id);
            if (conflito != null)
                throw new ConflictException($"Já existe uma aula neste horário: \"{conflito.Titulo}\" com {conflito.Aluno?.Nome}.");

            if (await _aulaAgendadaRepository.ExisteConflitoAluno(aula.AlunoId, inicio, fim, aula.Id))
                throw new ConflictException($"{aula.Aluno?.Nome} já tem uma aula agendada neste horário.");

            await _aulaAgendadaRepository.Atualizar(aula.Id, request.AulaId, titulo, descricao, inicio, fim, duracao);

            aula.AulaId = request.AulaId;
            aula.Titulo = titulo;
            aula.Descricao = descricao;
            aula.Inicio = inicio;
            aula.Fim = fim;
            aula.DuracaoMinutos = duracao;
            return MapParaDTO(aula);
        }

        public async Task<AulaAgendadaDTO> CancelarAula(int professorId, Guid aulaId)
        {
            var aula = await _aulaAgendadaRepository.GetByIdDoProfessor(aulaId, professorId);
            if (aula == null)
                throw new NotFoundException("Aula não encontrada.");

            // Idempotente: cancelar uma aula já cancelada não muda nada.
            if (aula.Status == AulaAgendada.StatusCancelada)
                return MapParaDTO(aula);

            var agora = DateTime.UtcNow;
            if (aula.Fim <= agora)
                throw new DomainValidationException("Não é possível cancelar uma aula que já aconteceu.");

            await _aulaAgendadaRepository.Cancelar(aula.Id, agora);

            aula.Status = AulaAgendada.StatusCancelada;
            aula.CanceladaEm = agora;
            return MapParaDTO(aula);
        }

        // Datas de query string chegam com Kind=Local (o model binding converte o "Z" para o
        // fuso do servidor); datas sem fuso são tratadas como UTC.
        private static DateTime ParaUtc(DateTime data)
        {
            return data.Kind switch
            {
                DateTimeKind.Utc => data,
                DateTimeKind.Local => data.ToUniversalTime(),
                _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
            };
        }

        private static AulaAgendadaDTO MapParaDTO(AulaAgendada aula)
        {
            return new AulaAgendadaDTO
            {
                Id = aula.Id.ToString(),
                Titulo = aula.Titulo,
                Descricao = aula.Descricao ?? string.Empty,
                AlunoId = aula.AlunoId,
                AlunoNome = aula.Aluno?.Nome ?? string.Empty,
                AulaId = aula.AulaId,
                Inicio = aula.Inicio,
                DuracaoMinutos = aula.DuracaoMinutos,
                Status = aula.Status,
                CriadaEm = aula.CriadaEm
            };
        }
    }
}
