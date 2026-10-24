using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowHowApi.Data.Maps
{
    public class MensagemMap : IEntityTypeConfiguration<Mensagem>
    {
        public void Configure(EntityTypeBuilder<Mensagem> builder)
        {
            builder.ToTable("Mensagens");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Texto).IsRequired().HasMaxLength(2000);
            builder.Property(m => m.Lida).IsRequired().HasDefaultValue(false);
            builder.Property(m => m.EnviadaEm).IsRequired();

            builder.HasIndex(m => new { m.ConversaId, m.EnviadaEm });

            builder.HasOne(m => m.Conversa)
                .WithMany()
                .HasForeignKey(m => m.ConversaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Remetente)
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
