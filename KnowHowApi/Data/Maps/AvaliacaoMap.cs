using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class AvaliacaoMap : IEntityTypeConfiguration<Avaliacao>
    {
        public void Configure(EntityTypeBuilder<Avaliacao> builder)
        {
            builder.ToTable("Avaliacoes", t => t.HasCheckConstraint("CK_Avaliacoes_Nota", "\"Nota\" BETWEEN 1 AND 5"));
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Nota).IsRequired();
            builder.Property(a => a.Comentario).IsRequired().HasMaxLength(500);
            builder.Property(a => a.CriadaEm).IsRequired();

            builder.HasIndex(a => new { a.ProfessorId, a.AlunoId }).IsUnique();

            builder.HasOne(a => a.Professor)
                .WithMany()
                .HasForeignKey(a => a.ProfessorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Aluno)
                .WithMany()
                .HasForeignKey(a => a.AlunoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
