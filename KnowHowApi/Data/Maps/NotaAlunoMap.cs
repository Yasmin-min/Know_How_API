using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class NotaAlunoMap : IEntityTypeConfiguration<NotaAluno>
    {
        public void Configure(EntityTypeBuilder<NotaAluno> builder)
        {
            builder.ToTable("NotasAluno");
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Texto).IsRequired().HasMaxLength(2000);
            builder.Property(n => n.AtualizadaEm).IsRequired();

            builder.HasIndex(n => new { n.ProfessorId, n.AlunoId }).IsUnique();

            builder.HasOne(n => n.Professor)
                .WithMany()
                .HasForeignKey(n => n.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.Aluno)
                .WithMany()
                .HasForeignKey(n => n.AlunoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
