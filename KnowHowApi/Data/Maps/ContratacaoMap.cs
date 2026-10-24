using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class ContratacaoMap : IEntityTypeConfiguration<Contratacao>
    {
        public void Configure(EntityTypeBuilder<Contratacao> builder)
        {
            builder.ToTable("Contratacoes");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Tipo).IsRequired().HasMaxLength(20);
            builder.Property(c => c.Descricao).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Valor).IsRequired();
            builder.Property(c => c.TotalAulas).IsRequired();
            builder.Property(c => c.AulasUsadas).IsRequired();
            builder.Property(c => c.Inicio).IsRequired();
            builder.Property(c => c.StatusPagamento).IsRequired().HasMaxLength(20);
            builder.Property(c => c.CriadaEm).IsRequired();

            builder.HasOne(c => c.Professor)
                .WithMany()
                .HasForeignKey(c => c.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Aluno)
                .WithMany()
                .HasForeignKey(c => c.AlunoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => new { c.ProfessorId, c.CriadaEm });
            builder.HasIndex(c => c.AlunoId);
        }
    }
}
