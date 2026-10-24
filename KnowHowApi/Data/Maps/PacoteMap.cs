using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class PacoteMap : IEntityTypeConfiguration<Pacote>
    {
        public void Configure(EntityTypeBuilder<Pacote> builder)
        {
            builder.ToTable("Pacotes");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nome).IsRequired().HasMaxLength(60);
            builder.Property(p => p.Descricao).IsRequired().HasMaxLength(200).HasDefaultValue(string.Empty);
            builder.Property(p => p.QuantidadeAulas).IsRequired();
            builder.Property(p => p.DuracaoMinutos).IsRequired();
            builder.Property(p => p.ValorTotal).IsRequired();
            builder.Property(p => p.ValidadeDias).IsRequired();
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
