using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class ConfiguracaoPrecosMap : IEntityTypeConfiguration<ConfiguracaoPrecos>
    {
        public void Configure(EntityTypeBuilder<ConfiguracaoPrecos> builder)
        {
            builder.ToTable("ConfiguracoesPrecos");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.AulaExperimentalTipo).IsRequired().HasMaxLength(20);
            builder.Property(c => c.AulaExperimentalDuracaoMinutos).IsRequired();
            builder.Property(c => c.AtualizadaEm).IsRequired();

            builder.OwnsMany(c => c.PrecosPorDuracao, precos => precos.ToJson());

            builder.HasIndex(c => c.ProfessorId).IsUnique();

            builder.HasOne(c => c.Professor)
                .WithMany()
                .HasForeignKey(c => c.ProfessorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
