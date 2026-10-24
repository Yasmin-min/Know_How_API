using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class PlanosRepository : BaseRepository, IPlanosRepository
    {
        public PlanosRepository(Context context) : base(context)
        {
        }

        public async Task<ConfiguracaoPrecos?> GetConfiguracaoPrecos(int professorId)
        {
            return await _context.ConfiguracoesPrecos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ProfessorId == professorId);
        }

        // Grava a configuração (insert ou update) e o valor da hora do perfil na mesma
        // transação, para os dois nunca ficarem dessincronizados.
        public async Task SalvarConfiguracaoPrecos(ConfiguracaoPrecos configuracao, decimal valorHora)
        {
            await using var transacao = await _context.Database.BeginTransactionAsync();

            var atual = await _context.ConfiguracoesPrecos
                .FirstOrDefaultAsync(c => c.ProfessorId == configuracao.ProfessorId);

            if (atual == null)
            {
                _context.ConfiguracoesPrecos.Add(configuracao);
            }
            else
            {
                atual.PrecosPorDuracao = configuracao.PrecosPorDuracao;
                atual.AulaExperimentalTipo = configuracao.AulaExperimentalTipo;
                atual.AulaExperimentalPercentualDesconto = configuracao.AulaExperimentalPercentualDesconto;
                atual.AulaExperimentalDuracaoMinutos = configuracao.AulaExperimentalDuracaoMinutos;
                atual.AtualizadaEm = configuracao.AtualizadaEm;
            }

            await _context.SaveChangesAsync();

            await _context.PerfilProfessores
                .Where(p => p.UsuarioId == configuracao.ProfessorId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.ValorHora, valorHora));

            await transacao.CommitAsync();
        }

        // Pacotes

        public async Task<List<Pacote>> ListarPacotes(int professorId, bool somenteAtivos)
        {
            var query = _context.Pacotes.AsNoTracking().Where(p => p.ProfessorId == professorId);
            if (somenteAtivos)
                query = query.Where(p => p.Ativo);

            return await query.OrderBy(p => p.CriadoEm).ToListAsync();
        }

        public async Task<Pacote?> GetPacote(Guid id, int professorId)
        {
            return await _context.Pacotes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.ProfessorId == professorId);
        }

        public async Task<Pacote> CriarPacote(Pacote pacote)
        {
            _context.Pacotes.Add(pacote);
            await _context.SaveChangesAsync();
            return pacote;
        }

        public async Task<Pacote> AtualizarPacote(Pacote pacote)
        {
            _context.Pacotes.Update(pacote);
            await _context.SaveChangesAsync();
            return pacote;
        }

        public async Task<bool> ExcluirPacote(Guid id, int professorId)
        {
            var removidos = await _context.Pacotes
                .Where(p => p.Id == id && p.ProfessorId == professorId)
                .ExecuteDeleteAsync();
            return removidos > 0;
        }

        // Planos mensais

        public async Task<List<PlanoMensal>> ListarPlanosMensais(int professorId, bool somenteAtivos)
        {
            var query = _context.PlanosMensais.AsNoTracking().Where(p => p.ProfessorId == professorId);
            if (somenteAtivos)
                query = query.Where(p => p.Ativo);

            return await query.OrderBy(p => p.CriadoEm).ToListAsync();
        }

        public async Task<PlanoMensal?> GetPlanoMensal(Guid id, int professorId)
        {
            return await _context.PlanosMensais
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.ProfessorId == professorId);
        }

        public async Task<PlanoMensal> CriarPlanoMensal(PlanoMensal plano)
        {
            _context.PlanosMensais.Add(plano);
            await _context.SaveChangesAsync();
            return plano;
        }

        public async Task<PlanoMensal> AtualizarPlanoMensal(PlanoMensal plano)
        {
            _context.PlanosMensais.Update(plano);
            await _context.SaveChangesAsync();
            return plano;
        }

        public async Task<bool> ExcluirPlanoMensal(Guid id, int professorId)
        {
            var removidos = await _context.PlanosMensais
                .Where(p => p.Id == id && p.ProfessorId == professorId)
                .ExecuteDeleteAsync();
            return removidos > 0;
        }

        // Contratações

        // Projeta só Id/Nome do aluno para não carregar a foto de perfil (bytea).
        private static IQueryable<Contratacao> ComNomeDoAluno(IQueryable<Contratacao> query)
        {
            return query.Select(c => new Contratacao
            {
                Id = c.Id,
                ProfessorId = c.ProfessorId,
                AlunoId = c.AlunoId,
                Aluno = new Usuario { Id = c.Aluno.Id, Nome = c.Aluno.Nome },
                Tipo = c.Tipo,
                OfertaId = c.OfertaId,
                Descricao = c.Descricao,
                Valor = c.Valor,
                TotalAulas = c.TotalAulas,
                AulasUsadas = c.AulasUsadas,
                Inicio = c.Inicio,
                Fim = c.Fim,
                StatusPagamento = c.StatusPagamento,
                PagoEm = c.PagoEm,
                CriadaEm = c.CriadaEm
            });
        }

        public async Task<List<Contratacao>> ListarContratacoes(int professorId)
        {
            var query = _context.Contratacoes
                .AsNoTracking()
                .Where(c => c.ProfessorId == professorId)
                .OrderByDescending(c => c.CriadaEm);

            return await ComNomeDoAluno(query).ToListAsync();
        }

        public async Task<Contratacao?> GetContratacao(Guid id, int professorId)
        {
            var query = _context.Contratacoes
                .AsNoTracking()
                .Where(c => c.Id == id && c.ProfessorId == professorId);

            return await ComNomeDoAluno(query).FirstOrDefaultAsync();
        }

        public async Task<Contratacao> CriarContratacao(Contratacao contratacao)
        {
            _context.Contratacoes.Add(contratacao);
            await _context.SaveChangesAsync();
            return contratacao;
        }

        // Atualiza só os campos que mudam depois da venda; a navegação Aluno (projetada)
        // não é anexada ao contexto.
        public async Task<Contratacao> AtualizarContratacao(Contratacao contratacao)
        {
            await _context.Contratacoes
                .Where(c => c.Id == contratacao.Id && c.ProfessorId == contratacao.ProfessorId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.AulasUsadas, contratacao.AulasUsadas)
                    .SetProperty(c => c.StatusPagamento, contratacao.StatusPagamento)
                    .SetProperty(c => c.PagoEm, contratacao.PagoEm));
            return contratacao;
        }

        public async Task<bool> ExcluirContratacao(Guid id, int professorId)
        {
            var removidos = await _context.Contratacoes
                .Where(c => c.Id == id && c.ProfessorId == professorId)
                .ExecuteDeleteAsync();
            return removidos > 0;
        }
    }
};
