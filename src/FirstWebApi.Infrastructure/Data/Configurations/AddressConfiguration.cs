using FirstWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstWebApi.Infrastructure.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Ciphertext)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(a => a.Iv)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(a => a.Tag)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(a => a.EncryptedDataKey)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.HasOne(a => a.User)
            .WithOne()
            .HasForeignKey<Address>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId)
            .IsUnique();
    }
}
