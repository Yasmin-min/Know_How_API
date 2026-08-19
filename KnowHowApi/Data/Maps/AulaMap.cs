using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class AulaMap : IEntityTypeConfiguration<Aula>
    {
        public void Configure(EntityTypeBuilder<Aula> builder)
        {
            builder.ToTable("Aulas");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Titulo).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Descricao).IsRequired().HasMaxLength(500);
            builder.Property(a => a.Nivel).HasMaxLength(30);
            builder.Property(a => a.Status).IsRequired().HasMaxLength(20);
            builder.Property(a => a.Valor).HasColumnType("decimal(18,2)");
            builder.Property(a => a.CriadaEm).IsRequired();

            builder.HasOne(a => a.Professor)
                .WithMany()
                .HasForeignKey(a => a.ProfessorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.AreaInteresse)
                .WithMany()
                .HasForeignKey(a => a.AreaInteresseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.ProfessorId, a.Status });
            builder.HasIndex(a => new { a.ProfessorId, a.Titulo });
        }
    }
}
