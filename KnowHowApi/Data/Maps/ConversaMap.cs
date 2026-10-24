using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class ConversaMap : IEntityTypeConfiguration<Conversa>
    {
        public void Configure(EntityTypeBuilder<Conversa> builder)
        {
            builder.ToTable("Conversas");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.CriadaEm).IsRequired();

            builder.HasIndex(c => new { c.AlunoId, c.ProfessorId }).IsUnique();

            builder.HasOne(c => c.Aluno)
                .WithMany()
                .HasForeignKey(c => c.AlunoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Professor)
                .WithMany()
                .HasForeignKey(c => c.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
