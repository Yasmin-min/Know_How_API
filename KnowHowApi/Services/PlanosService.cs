using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services
{
    // Preços, pacotes, planos mensais e contratações (vendas) do professor.
    public class PlanosService : IPlanosService
    {
        private const int NomeMaxLength = 60;
        private const int DescricaoOfertaMaxLength = 200;
        private const int DescricaoContratacaoMaxLength = 200;
        private const int PacoteMinAulas = 2;
        private const int PacoteMaxAulas = 50;
        private const int ValidadeMaxDias = 365;
        private const int AulasPorSemanaMax = 7;
        private const int ContratacaoMaxAulas = 100;
        private const int PercentualDescontoMin = 1;
        private const int PercentualDescontoMax = 99;
        private static readonly HashSet<int> DuracoesPermitidas = new() { 30, 45, 60, 90, 120 };
        private static readonly HashSet<int> DuracoesExperimentalPermitidas = new() { 30, 45, 60 };
        private static readonly HashSet<string> TiposAulaExperimental = new(StringComparer.Ordinal)
        {
            ConfiguracaoPrecos.ExperimentalNenhuma, ConfiguracaoPrecos.ExperimentalGratis, ConfiguracaoPrecos.ExperimentalDesconto
        };
        private static readonly HashSet<string> TiposContratacao = new(StringComparer.Ordinal)
        {
            Contratacao.TipoAvulsa, Contratacao.TipoPacote, Contratacao.TipoMensal
        };

        private readonly IPlanosRepository _planosRepository;
        private readonly IProfessorRepository _professorRepository;
        private readonly IProfessorAlunoRepository _professorAlunoRepository;

        public PlanosService(IPlanosRepository planosRepository, IProfessorRepository professorRepository, IProfessorAlunoRepository professorAlunoRepository)
        {
            _planosRepository = planosRepository;
            _professorRepository = professorRepository;
            _professorAlunoRepository = professorAlunoRepository;
        }

        // Preços

        public async Task<ConfiguracaoPrecosDTO> ObterPrecos(int professorId)
        {
            var perfil = await GetPerfilOuFalhar(professorId);
            var configuracao = await _planosRepository.GetConfiguracaoPrecos(professorId);
            return MapPrecos(perfil.ValorHora, configuracao);
        }

        public async Task<ConfiguracaoPrecosDTO> AtualizarPrecos(int professorId, AtualizarPrecosRequestDTO request)
        {
            if (request?.ValorHora is not decimal valorHora || valorHora <= 0)
                throw new DomainValidationException("Informe o valor da sua hora-aula.");

            var precosPorDuracao = new List<PrecoPorDuracao>();
            foreach (var item in request.PrecosPorDuracao ?? new List<PrecoPorDuracaoDTO>())
            {
                if (item?.DuracaoMinutos is not int duracao || !DuracoesPermitidas.Contains(duracao))
                    throw new DomainValidationException("Duração inválida nos preços personalizados.");

                if (item.Valor is not decimal valor || valor <= 0)
                    throw new DomainValidationException("Os preços personalizados devem ser maiores que zero.");

                if (precosPorDuracao.Any(p => p.DuracaoMinutos == duracao))
                    throw new DomainValidationException("Informe só um preço personalizado por duração.");

                precosPorDuracao.Add(new PrecoPorDuracao { DuracaoMinutos = duracao, Valor = valor });
            }

            var experimental = request.AulaExperimental;
            var tipoExperimental = experimental?.Tipo?.Trim() ?? string.Empty;
            if (!TiposAulaExperimental.Contains(tipoExperimental))
                throw new DomainValidationException("Tipo de aula experimental inválido.");

            if (experimental!.DuracaoMinutos is not int duracaoExperimental || !DuracoesExperimentalPermitidas.Contains(duracaoExperimental))
                throw new DomainValidationException("Duração da aula experimental inválida.");

            int? percentualDesconto = null;
            if (tipoExperimental == ConfiguracaoPrecos.ExperimentalDesconto)
            {
                if (experimental.PercentualDesconto is not int percentual || percentual < PercentualDescontoMin || percentual > PercentualDescontoMax)
                    throw new DomainValidationException($"Informe um desconto entre {PercentualDescontoMin}% e {PercentualDescontoMax}%.");

                percentualDesconto = percentual;
            }

            await GetPerfilOuFalhar(professorId);

            var configuracao = new ConfiguracaoPrecos
            {
                ProfessorId = professorId,
                PrecosPorDuracao = precosPorDuracao.OrderBy(p => p.DuracaoMinutos).ToList(),
                AulaExperimentalTipo = tipoExperimental,
                AulaExperimentalPercentualDesconto = percentualDesconto,
                AulaExperimentalDuracaoMinutos = duracaoExperimental,
                AtualizadaEm = DateTime.UtcNow
            };

            await _planosRepository.SalvarConfiguracaoPrecos(configuracao, valorHora);
            return MapPrecos(valorHora, configuracao);
        }

        // Pacotes

        public async Task<List<PacoteDTO>> ListarPacotes(int professorId)
        {
            var pacotes = await _planosRepository.ListarPacotes(professorId, somenteAtivos: false);
            return pacotes.Select(MapPacote).ToList();
        }

        public async Task<PacoteDTO> CriarPacote(int professorId, SalvarPacoteRequestDTO request)
        {
            var pacote = new Pacote { Id = Guid.NewGuid(), ProfessorId = professorId, CriadoEm = DateTime.UtcNow };
            AplicarDadosPacote(pacote, request);

            pacote = await _planosRepository.CriarPacote(pacote);
            return MapPacote(pacote);
        }

        public async Task<PacoteDTO> AtualizarPacote(int professorId, Guid pacoteId, SalvarPacoteRequestDTO request)
        {
            var pacote = await _planosRepository.GetPacote(pacoteId, professorId)
                ?? throw new NotFoundException("Pacote não encontrado.");
            AplicarDadosPacote(pacote, request);

            pacote = await _planosRepository.AtualizarPacote(pacote);
            return MapPacote(pacote);
        }

        public async Task ExcluirPacote(int professorId, Guid pacoteId)
        {
            if (!await _planosRepository.ExcluirPacote(pacoteId, professorId))
                throw new NotFoundException("Pacote não encontrado.");
        }

        private static void AplicarDadosPacote(Pacote pacote, SalvarPacoteRequestDTO request)
        {
            var (nome, descricao) = ValidarNomeEDescricao(request?.Nome, request?.Descricao);

            if (request!.QuantidadeAulas is not int quantidade || quantidade < PacoteMinAulas || quantidade > PacoteMaxAulas)
                throw new DomainValidationException($"O pacote deve ter de {PacoteMinAulas} a {PacoteMaxAulas} aulas.");

            var duracao = ValidarDuracao(request.DuracaoMinutos);

            if (request.ValorTotal is not decimal valor || valor <= 0)
                throw new DomainValidationException("Informe um valor válido.");

            if (request.ValidadeDias is not int validade || validade < 1 || validade > ValidadeMaxDias)
                throw new DomainValidationException($"A validade do pacote deve ser de 1 a {ValidadeMaxDias} dias.");

            pacote.Nome = nome;
            pacote.Descricao = descricao;
            pacote.QuantidadeAulas = quantidade;
            pacote.DuracaoMinutos = duracao;
            pacote.ValorTotal = valor;
            pacote.ValidadeDias = validade;
            pacote.Ativo = request.Ativo ?? true;
        }

        // Planos mensais

        public async Task<List<PlanoMensalDTO>> ListarPlanosMensais(int professorId)
        {
            var planos = await _planosRepository.ListarPlanosMensais(professorId, somenteAtivos: false);
            return planos.Select(MapPlanoMensal).ToList();
        }

        public async Task<PlanoMensalDTO> CriarPlanoMensal(int professorId, SalvarPlanoMensalRequestDTO request)
        {
            var plano = new PlanoMensal { Id = Guid.NewGuid(), ProfessorId = professorId, CriadoEm = DateTime.UtcNow };
            AplicarDadosPlanoMensal(plano, request);

            plano = await _planosRepository.CriarPlanoMensal(plano);
            return MapPlanoMensal(plano);
        }

        public async Task<PlanoMensalDTO> AtualizarPlanoMensal(int professorId, Guid planoId, SalvarPlanoMensalRequestDTO request)
        {
            var plano = await _planosRepository.GetPlanoMensal(planoId, professorId)
                ?? throw new NotFoundException("Plano mensal não encontrado.");
            AplicarDadosPlanoMensal(plano, request);

            plano = await _planosRepository.AtualizarPlanoMensal(plano);
            return MapPlanoMensal(plano);
        }

        public async Task ExcluirPlanoMensal(int professorId, Guid planoId)
        {
            if (!await _planosRepository.ExcluirPlanoMensal(planoId, professorId))
                throw new NotFoundException("Plano mensal não encontrado.");
        }

        private static void AplicarDadosPlanoMensal(PlanoMensal plano, SalvarPlanoMensalRequestDTO request)
        {
            var (nome, descricao) = ValidarNomeEDescricao(request?.Nome, request?.Descricao);

            if (request!.AulasPorSemana is not int aulasPorSemana || aulasPorSemana < 1 || aulasPorSemana > AulasPorSemanaMax)
                throw new DomainValidationException($"Escolha de 1 a {AulasPorSemanaMax} aulas por semana.");

            var duracao = ValidarDuracao(request.DuracaoMinutos);

            if (request.ValorMensal is not decimal valor || valor <= 0)
                throw new DomainValidationException("Informe um valor válido.");

            plano.Nome = nome;
            plano.Descricao = descricao;
            plano.AulasPorSemana = aulasPorSemana;
            plano.DuracaoMinutos = duracao;
            plano.ValorMensal = valor;
            plano.Ativo = request.Ativo ?? true;
        }

        private static (string Nome, string Descricao) ValidarNomeEDescricao(string? nomeInformado, string? descricaoInformada)
        {
            var nome = (nomeInformado ?? string.Empty).Trim();
            var descricao = (descricaoInformada ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(nome))
                throw new DomainValidationException("Dê um nome para identificar a oferta.");

            if (nome.Length > NomeMaxLength)
                throw new DomainValidationException($"O nome deve ter no máximo {NomeMaxLength} caracteres.");

            if (descricao.Length > DescricaoOfertaMaxLength)
                throw new DomainValidationException($"A descrição deve ter no máximo {DescricaoOfertaMaxLength} caracteres.");

            return (nome, descricao);
        }

        private static int ValidarDuracao(int? duracaoMinutos)
        {
            if (duracaoMinutos is not int duracao || !DuracoesPermitidas.Contains(duracao))
                throw new DomainValidationException("Duração inválida.");

            return duracao;
        }

        // Ofertas públicas

        public async Task<OfertasProfessorDTO> ObterOfertas(int professorId)
        {
            var perfil = await _professorRepository.GetPerfilProfessorByUsuarioId(professorId)
                ?? throw new NotFoundException("Professor não encontrado.");

            var configuracao = await _planosRepository.GetConfiguracaoPrecos(professorId);
            var pacotes = await _planosRepository.ListarPacotes(professorId, somenteAtivos: true);
            var planos = await _planosRepository.ListarPlanosMensais(professorId, somenteAtivos: true);

            return new OfertasProfessorDTO
            {
                Precos = MapPrecos(perfil.ValorHora, configuracao),
                Pacotes = pacotes.Select(MapPacote).ToList(),
                PlanosMensais = planos.Select(MapPlanoMensal).ToList()
            };
        }

        // Contratações

        public async Task<List<ContratacaoDTO>> ListarContratacoes(int professorId)
        {
            var contratacoes = await _planosRepository.ListarContratacoes(professorId);
            return contratacoes.Select(MapContratacao).ToList();
        }

        public async Task<ContratacaoDTO> CriarContratacao(int professorId, CriarContratacaoRequestDTO request)
        {
            if (request?.AlunoId is not int alunoId)
                throw new DomainValidationException("Selecione o aluno.");

            var tipo = request.Tipo?.Trim() ?? string.Empty;
            if (!TiposContratacao.Contains(tipo))
                throw new DomainValidationException("Tipo de contratação inválido.");

            var descricao = (request.Descricao ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(descricao))
                throw new DomainValidationException("Informe a descrição da contratação.");

            if (descricao.Length > DescricaoContratacaoMaxLength)
                throw new DomainValidationException($"A descrição deve ter no máximo {DescricaoContratacaoMaxLength} caracteres.");

            if (request.Valor is not decimal valor || valor <= 0)
                throw new DomainValidationException("Informe o valor cobrado.");

            if (request.TotalAulas is not int totalAulas || totalAulas < 1 || totalAulas > ContratacaoMaxAulas)
                throw new DomainValidationException($"O total de aulas deve ser de 1 a {ContratacaoMaxAulas}.");

            if (request.Inicio is not DateOnly inicio)
                throw new DomainValidationException("Informe a data de início.");

            Guid? ofertaId = null;
            if (tipo == Contratacao.TipoAvulsa)
            {
                if (!string.IsNullOrEmpty(request.OfertaId))
                    throw new DomainValidationException("Aula avulsa não tem pacote ou plano de origem.");

                if (request.Fim.HasValue)
                    throw new DomainValidationException("Aula avulsa não tem data de fim.");
            }
            else
            {
                if (!Guid.TryParse(request.OfertaId, out var ofertaInformada))
                    throw new DomainValidationException(tipo == Contratacao.TipoPacote ? "Selecione o pacote." : "Selecione o plano mensal.");

                var ofertaExiste = tipo == Contratacao.TipoPacote
                    ? await _planosRepository.GetPacote(ofertaInformada, professorId) != null
                    : await _planosRepository.GetPlanoMensal(ofertaInformada, professorId) != null;
                if (!ofertaExiste)
                    throw new DomainValidationException(tipo == Contratacao.TipoPacote ? "Pacote não encontrado." : "Plano mensal não encontrado.");

                if (request.Fim is not DateOnly fim)
                    throw new DomainValidationException("Informe a data de fim.");

                if (fim < inicio)
                    throw new DomainValidationException("A data de fim deve ser igual ou posterior à de início.");

                ofertaId = ofertaInformada;
            }

            var aluno = await _professorAlunoRepository.GetAlunoDoProfessor(professorId, alunoId)
                ?? throw new DomainValidationException("Aluno não encontrado entre os seus alunos.");

            var agora = DateTime.UtcNow;
            var contratacao = new Contratacao
            {
                Id = Guid.NewGuid(),
                ProfessorId = professorId,
                AlunoId = alunoId,
                Tipo = tipo,
                OfertaId = ofertaId,
                Descricao = descricao,
                Valor = valor,
                TotalAulas = totalAulas,
                AulasUsadas = 0,
                Inicio = inicio,
                Fim = tipo == Contratacao.TipoAvulsa ? null : request.Fim,
                StatusPagamento = request.Pago ? Contratacao.StatusPago : Contratacao.StatusPendente,
                PagoEm = request.Pago ? agora : null,
                CriadaEm = agora
            };

            contratacao = await _planosRepository.CriarContratacao(contratacao);
            contratacao.Aluno = aluno;

            return MapContratacao(contratacao);
        }

        public async Task<ContratacaoDTO> AlterarPagamento(int professorId, Guid contratacaoId, AlterarPagamentoContratacaoRequestDTO request)
        {
            if (request?.Pago is not bool pago)
                throw new DomainValidationException("Informe se a contratação foi paga.");

            var contratacao = await GetContratacaoOuFalhar(professorId, contratacaoId);

            // Idempotente: marcar como paga de novo mantém a data do primeiro pagamento.
            var novoStatus = pago ? Contratacao.StatusPago : Contratacao.StatusPendente;
            if (contratacao.StatusPagamento == novoStatus)
                return MapContratacao(contratacao);

            contratacao.StatusPagamento = novoStatus;
            contratacao.PagoEm = pago ? DateTime.UtcNow : null;
            contratacao = await _planosRepository.AtualizarContratacao(contratacao);
            return MapContratacao(contratacao);
        }

        public async Task<ContratacaoDTO> AlterarAulasUsadas(int professorId, Guid contratacaoId, AlterarAulasUsadasRequestDTO request)
        {
            if (request?.AulasUsadas is not int aulasUsadas)
                throw new DomainValidationException("Informe a quantidade de aulas usadas.");

            var contratacao = await GetContratacaoOuFalhar(professorId, contratacaoId);

            if (aulasUsadas < 0 || aulasUsadas > contratacao.TotalAulas)
                throw new DomainValidationException($"As aulas usadas devem ficar entre 0 e {contratacao.TotalAulas}.");

            if (contratacao.AulasUsadas == aulasUsadas)
                return MapContratacao(contratacao);

            contratacao.AulasUsadas = aulasUsadas;
            contratacao = await _planosRepository.AtualizarContratacao(contratacao);
            return MapContratacao(contratacao);
        }

        public async Task ExcluirContratacao(int professorId, Guid contratacaoId)
        {
            if (!await _planosRepository.ExcluirContratacao(contratacaoId, professorId))
                throw new NotFoundException("Contratação não encontrada.");
        }

        private async Task<Contratacao> GetContratacaoOuFalhar(int professorId, Guid contratacaoId)
        {
            return await _planosRepository.GetContratacao(contratacaoId, professorId)
                ?? throw new NotFoundException("Contratação não encontrada.");
        }

        private async Task<PerfilProfessor> GetPerfilOuFalhar(int professorId)
        {
            return await _professorRepository.GetPerfilProfessorByUsuarioId(professorId)
                ?? throw new NotFoundException("Perfil de professor não encontrado para o usuário autenticado.");
        }

        // Mapeamentos

        // ValorHora = 0 no perfil significa "ainda não definido" e sai como null.
        private static ConfiguracaoPrecosDTO MapPrecos(decimal valorHora, ConfiguracaoPrecos? configuracao)
        {
            configuracao ??= new ConfiguracaoPrecos();

            return new ConfiguracaoPrecosDTO
            {
                ValorHora = valorHora > 0 ? valorHora : null,
                PrecosPorDuracao = configuracao.PrecosPorDuracao
                    .OrderBy(p => p.DuracaoMinutos)
                    .Select(p => new PrecoPorDuracaoDTO { DuracaoMinutos = p.DuracaoMinutos, Valor = p.Valor })
                    .ToList(),
                AulaExperimental = new AulaExperimentalDTO
                {
                    Tipo = configuracao.AulaExperimentalTipo,
                    PercentualDesconto = configuracao.AulaExperimentalPercentualDesconto,
                    DuracaoMinutos = configuracao.AulaExperimentalDuracaoMinutos
                }
            };
        }

        private static PacoteDTO MapPacote(Pacote pacote)
        {
            return new PacoteDTO
            {
                Id = pacote.Id.ToString(),
                Nome = pacote.Nome,
                Descricao = pacote.Descricao ?? string.Empty,
                QuantidadeAulas = pacote.QuantidadeAulas,
                DuracaoMinutos = pacote.DuracaoMinutos,
                ValorTotal = pacote.ValorTotal,
                ValidadeDias = pacote.ValidadeDias,
                Ativo = pacote.Ativo,
                CriadoEm = pacote.CriadoEm
            };
        }

        private static PlanoMensalDTO MapPlanoMensal(PlanoMensal plano)
        {
            return new PlanoMensalDTO
            {
                Id = plano.Id.ToString(),
                Nome = plano.Nome,
                Descricao = plano.Descricao ?? string.Empty,
                AulasPorSemana = plano.AulasPorSemana,
                DuracaoMinutos = plano.DuracaoMinutos,
                ValorMensal = plano.ValorMensal,
                Ativo = plano.Ativo,
                CriadoEm = plano.CriadoEm
            };
        }

        private static ContratacaoDTO MapContratacao(Contratacao contratacao)
        {
            return new ContratacaoDTO
            {
                Id = contratacao.Id.ToString(),
                AlunoId = contratacao.AlunoId,
                AlunoNome = contratacao.Aluno?.Nome ?? string.Empty,
                Tipo = contratacao.Tipo,
                OfertaId = contratacao.OfertaId?.ToString(),
                Descricao = contratacao.Descricao,
                Valor = contratacao.Valor,
                TotalAulas = contratacao.TotalAulas,
                AulasUsadas = contratacao.AulasUsadas,
                Inicio = contratacao.Inicio,
                Fim = contratacao.Fim,
                StatusPagamento = contratacao.StatusPagamento,
                PagoEm = contratacao.PagoEm,
                CriadaEm = contratacao.CriadaEm
            };
        }
    }
}
