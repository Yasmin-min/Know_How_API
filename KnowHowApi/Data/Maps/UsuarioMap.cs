using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class UsuarioMap : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Nome).IsRequired();
            builder.Property(u => u.Email).IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.SenhaHash).IsRequired();
            builder.Property(u => u.TipoUsuario).HasConversion<int>();
            builder.Property(u => u.DataNascimento).HasColumnType("date").IsRequired();
            builder.Property(u => u.Cpf).HasMaxLength(11);
            builder.HasIndex(u => u.Cpf).IsUnique().HasFilter("\"Cpf\" IS NOT NULL");

            builder.HasOne(u => u.AreaInteresse)
                .WithMany()
                .HasForeignKey(u => u.AreaInteresseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.RecuperacaoSenhaTentativas).IsRequired().HasDefaultValue(0);
        }
    }
}
