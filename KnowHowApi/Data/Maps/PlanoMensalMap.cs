using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class PlanoMensalMap : IEntityTypeConfiguration<PlanoMensal>
    {
        public void Configure(EntityTypeBuilder<PlanoMensal> builder)
        {
            builder.ToTable("PlanosMensais");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nome).IsRequired().HasMaxLength(60);
            builder.Property(p => p.Descricao).IsRequired().HasMaxLength(200).HasDefaultValue(string.Empty);
            builder.Property(p => p.AulasPorSemana).IsRequired();
            builder.Property(p => p.DuracaoMinutos).IsRequired();
            builder.Property(p => p.ValorMensal).IsRequired();
            builder.Property(p => p.Ativo).IsRequired();
            builder.Property(p => p.CriadoEm).IsRequired();

            builder.HasOne(p => p.Professor)
                .WithMany()
                .HasForeignKey(p => p.ProfessorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.ProfessorId);
        }
    }
}
