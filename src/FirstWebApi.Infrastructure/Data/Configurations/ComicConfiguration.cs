using FirstWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstWebApi.Infrastructure.Data.Configurations;

public class ComicConfiguration : IEntityTypeConfiguration<Comic>
{
    public void Configure(EntityTypeBuilder<Comic> builder)
    {
        builder.ToTable("Comics");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Titulo)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.WebUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(c => c.Observacao)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ComicType)
            .WithMany()
            .HasForeignKey(c => c.ComicTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.UserId);
    }
}
