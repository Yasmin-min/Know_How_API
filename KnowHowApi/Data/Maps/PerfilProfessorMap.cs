using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class PerfilProfessorMap : IEntityTypeConfiguration<PerfilProfessor>
    {
        public void Configure(EntityTypeBuilder<PerfilProfessor> builder)
        {
            builder.ToTable("PerfilProfessores");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Materia).IsRequired();
            builder.Property(p => p.Descricao).IsRequired();
            builder.Property(p => p.AvatarVariante).IsRequired();
            builder.Property(p => p.ValorHora).HasColumnType("decimal(10,2)");
            builder.Property(p => p.Avaliacao).HasColumnType("decimal(3,2)");
            builder.Property(p => p.AnosExperiencia).HasMaxLength(50);
            builder.Property(p => p.FotoPerfilContentType).HasMaxLength(100);

            builder.HasIndex(p => p.UsuarioId).IsUnique();
            builder.HasOne(p => p.Usuario)
                .WithOne()
                .HasForeignKey<PerfilProfessor>(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.AreaEspecialidade)
                .WithMany()
                .HasForeignKey(p => p.AreaEspecialidadeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
