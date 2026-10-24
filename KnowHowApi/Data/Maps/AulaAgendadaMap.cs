using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class AulaAgendadaMap : IEntityTypeConfiguration<AulaAgendada>
    {
        public void Configure(EntityTypeBuilder<AulaAgendada> builder)
        {
            builder.ToTable("AulasAgendadas");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Titulo).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Descricao).IsRequired().HasMaxLength(500).HasDefaultValue(string.Empty);
            builder.Property(a => a.Status).IsRequired().HasMaxLength(20);
            builder.Property(a => a.Inicio).IsRequired();
            builder.Property(a => a.Fim).IsRequired();
            builder.Property(a => a.DuracaoMinutos).IsRequired();
            builder.Property(a => a.CriadaEm).IsRequired();

            builder.HasOne(a => a.Professor)
                .WithMany()
                .HasForeignKey(a => a.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Aluno)
                .WithMany()
                .HasForeignKey(a => a.AlunoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Aula)
                .WithMany()
                .HasForeignKey(a => a.AulaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(a => new { a.ProfessorId, a.Inicio });
            builder.HasIndex(a => new { a.AlunoId, a.Inicio });
        }
    }
}
