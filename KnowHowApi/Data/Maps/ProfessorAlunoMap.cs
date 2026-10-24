using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class ProfessorAlunoMap : IEntityTypeConfiguration<ProfessorAluno>
    {
        public void Configure(EntityTypeBuilder<ProfessorAluno> builder)
        {
            builder.ToTable("ProfessorAlunos");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.CriadoEm).IsRequired();

            builder.HasIndex(p => new { p.ProfessorId, p.AlunoId }).IsUnique();

            builder.HasOne(p => p.Professor)
                .WithMany()
                .HasForeignKey(p => p.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Aluno)
                .WithMany()
                .HasForeignKey(p => p.AlunoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
