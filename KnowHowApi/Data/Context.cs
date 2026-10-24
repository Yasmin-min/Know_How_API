using System;
using KnowHowApi.Data.Maps;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KnowHowApi.Data
{
    public class Context : DbContext
    {
        public Context() { }

        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PerfilProfessor> PerfilProfessores { get; set; }
        public DbSet<AreaInteresse> AreasInteresse { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<Conversa> Conversas { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<AulaAgendada> AulasAgendadas { get; set; }
        public DbSet<ProfessorAluno> ProfessorAlunos { get; set; }
        public DbSet<NotaAluno> NotasAluno { get; set; }
        public DbSet<ConfiguracaoPrecos> ConfiguracoesPrecos { get; set; }
        public DbSet<Pacote> Pacotes { get; set; }
        public DbSet<PlanoMensal> PlanosMensais { get; set; }
        public DbSet<Contratacao> Contratacoes { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioMap());
            modelBuilder.ApplyConfiguration(new PerfilProfessorMap());
            modelBuilder.ApplyConfiguration(new AreaInteresseMap());
            modelBuilder.ApplyConfiguration(new AulaMap());
            modelBuilder.ApplyConfiguration(new ConversaMap());
            modelBuilder.ApplyConfiguration(new MensagemMap());
            modelBuilder.ApplyConfiguration(new AulaAgendadaMap());
            modelBuilder.ApplyConfiguration(new ProfessorAlunoMap());
            modelBuilder.ApplyConfiguration(new NotaAlunoMap());
            modelBuilder.ApplyConfiguration(new ConfiguracaoPrecosMap());
            modelBuilder.ApplyConfiguration(new PacoteMap());
            modelBuilder.ApplyConfiguration(new PlanoMensalMap());
            modelBuilder.ApplyConfiguration(new ContratacaoMap());
            modelBuilder.ApplyConfiguration(new AvaliacaoMap());
            base.OnModelCreating(modelBuilder);

            var utcConverter = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.IsKeyless)
                {
                    continue;
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(utcConverter);
                    }
                }
            }
        }
    }
}
