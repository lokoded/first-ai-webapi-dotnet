using FirstWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstWebApi.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.CpfCiphertext)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(u => u.CpfIv)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(u => u.CpfTag)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(u => u.CpfEncryptedDataKey)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(u => u.RgCiphertext)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(u => u.RgIv)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(u => u.RgTag)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(u => u.RgEncryptedDataKey)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);
    }
}
