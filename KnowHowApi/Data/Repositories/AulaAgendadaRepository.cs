using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class AulaAgendadaRepository : BaseRepository, IAulaAgendadaRepository
    {
        public AulaAgendadaRepository(Context context) : base(context)
        {
        }

        // Projeta só Id/Nome do aluno para não carregar a foto de perfil (bytea).
        private static IQueryable<AulaAgendada> ComNomeDoAluno(IQueryable<AulaAgendada> query)
        {
            return query.Select(a => new AulaAgendada
            {
                Id = a.Id,
                ProfessorId = a.ProfessorId,
                AlunoId = a.AlunoId,
                Aluno = new Usuario { Id = a.Aluno.Id, Nome = a.Aluno.Nome },
                AulaId = a.AulaId,
                Titulo = a.Titulo,
                Descricao = a.Descricao,
                Inicio = a.Inicio,
                Fim = a.Fim,
                DuracaoMinutos = a.DuracaoMinutos,
                Status = a.Status,
                CriadaEm = a.CriadaEm,
                CanceladaEm = a.CanceladaEm
            });
        }

        public async Task<List<AulaAgendada>> ListarDoProfessor(int professorId, DateTime? inicio, DateTime? fim)
        {
            var query = _context.AulasAgendadas.AsNoTracking().Where(a => a.ProfessorId == professorId);

            if (fim.HasValue)
                query = query.Where(a => a.Inicio < fim.Value);

            if (inicio.HasValue)
                query = query.Where(a => a.Fim > inicio.Value);

            return await ComNomeDoAluno(query.OrderBy(a => a.Inicio)).ToListAsync();
        }

        public async Task<List<AulaAgendada>> ListarDoAluno(int alunoId, DateTime? inicio, DateTime? fim)
        {
            var query = _context.AulasAgendadas.AsNoTracking().Where(a => a.AlunoId == alunoId);

            if (fim.HasValue)
                query = query.Where(a => a.Inicio < fim.Value);

            if (inicio.HasValue)
                query = query.Where(a => a.Fim > inicio.Value);

            return await query
                .OrderBy(a => a.Inicio)
                .Select(a => new AulaAgendada
                {
                    Id = a.Id,
                    ProfessorId = a.ProfessorId,
                    Professor = new Usuario { Id = a.Professor.Id, Nome = a.Professor.Nome },
                    AlunoId = a.AlunoId,
                    Titulo = a.Titulo,
                    Descricao = a.Descricao,
                    Inicio = a.Inicio,
                    Fim = a.Fim,
                    DuracaoMinutos = a.DuracaoMinutos,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<AulaAgendada?> GetConflitoProfessor(int professorId, DateTime inicio, DateTime fim, Guid? ignorarAulaId = null)
        {
            var query = _context.AulasAgendadas
                .AsNoTracking()
                .Where(a => a.ProfessorId == professorId
                    && a.Status == AulaAgendada.StatusAgendada
                    && a.Id != ignorarAulaId
                    && a.Inicio < fim
                    && inicio < a.Fim)
                .OrderBy(a => a.Inicio);

            return await ComNomeDoAluno(query).FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteConflitoAluno(int alunoId, DateTime inicio, DateTime fim, Guid? ignorarAulaId = null)
        {
            return await _context.AulasAgendadas
                .AsNoTracking()
                .AnyAsync(a => a.AlunoId == alunoId
                    && a.Status == AulaAgendada.StatusAgendada
                    && a.Id != ignorarAulaId
                    && a.Inicio < fim
                    && inicio < a.Fim);
        }

        public async Task<AulaAgendada?> GetByIdDoProfessor(Guid id, int professorId)
        {
            var query = _context.AulasAgendadas
                .AsNoTracking()
                .Where(a => a.Id == id && a.ProfessorId == professorId);

            return await ComNomeDoAluno(query).FirstOrDefaultAsync();
        }

        public async Task<AulaAgendada?> GetByIdComParticipantes(Guid id)
        {
            return await _context.AulasAgendadas
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AulaAgendada
                {
                    Id = a.Id,
                    ProfessorId = a.ProfessorId,
                    Professor = new Usuario { Id = a.Professor.Id, Nome = a.Professor.Nome },
                    AlunoId = a.AlunoId,
                    Aluno = new Usuario { Id = a.Aluno.Id, Nome = a.Aluno.Nome },
                    Titulo = a.Titulo,
                    Inicio = a.Inicio,
                    Fim = a.Fim,
                    DuracaoMinutos = a.DuracaoMinutos,
                    Status = a.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AulaAgendada> Criar(AulaAgendada aula)
        {
            _context.AulasAgendadas.Add(aula);
            await _context.SaveChangesAsync();
            return aula;
        }

        public async Task Atualizar(Guid id, int? aulaId, string titulo, string descricao, DateTime inicio, DateTime fim, int duracaoMinutos)
        {
            await _context.AulasAgendadas
                .Where(a => a.Id == id && a.Status == AulaAgendada.StatusAgendada)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.AulaId, aulaId)
                    .SetProperty(a => a.Titulo, titulo)
                    .SetProperty(a => a.Descricao, descricao)
                    .SetProperty(a => a.Inicio, inicio)
                    .SetProperty(a => a.Fim, fim)
                    .SetProperty(a => a.DuracaoMinutos, duracaoMinutos));
        }

        public async Task Cancelar(Guid id, DateTime canceladaEm)
        {
            await _context.AulasAgendadas
                .Where(a => a.Id == id && a.Status == AulaAgendada.StatusAgendada)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Status, AulaAgendada.StatusCancelada)
                    .SetProperty(a => a.CanceladaEm, canceladaEm));
        }

        // Alunos distintos com pelo menos uma aula não cancelada que já terminou até `ate`.
        public async Task<int> ContarAlunosAtendidos(int professorId, DateTime ate)
        {
            return await _context.AulasAgendadas
                .AsNoTracking()
                .Where(a => a.ProfessorId == professorId && a.Status == AulaAgendada.StatusAgendada && a.Fim <= ate)
                .Select(a => a.AlunoId)
                .Distinct()
                .CountAsync();
        }
    }
};
