using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class AreaInteresseMap : IEntityTypeConfiguration<AreaInteresse>
    {
        public void Configure(EntityTypeBuilder<AreaInteresse> builder)
        {
            builder.ToTable("AreasInteresse");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Nome).IsRequired().HasMaxLength(100);
            builder.HasIndex(a => a.Nome).IsUnique();

            builder.HasData(
                new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" },
                new AreaInteresse { Id = 2, Nome = "Idiomas" },
                new AreaInteresse { Id = 3, Nome = "Música" },
                new AreaInteresse { Id = 4, Nome = "Design e Artes" },
                new AreaInteresse { Id = 5, Nome = "Negócios e Empreendedorismo" },
                new AreaInteresse { Id = 6, Nome = "Saúde e Bem-estar" },
                new AreaInteresse { Id = 7, Nome = "Reforço Escolar" },
                new AreaInteresse { Id = 8, Nome = "Outros" }
            );
        }
    }
}
