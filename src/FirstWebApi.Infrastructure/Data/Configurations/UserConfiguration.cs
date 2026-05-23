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

        builder.Property(u => u.CpfDados)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(u => u.RgDados)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);
    }
}
