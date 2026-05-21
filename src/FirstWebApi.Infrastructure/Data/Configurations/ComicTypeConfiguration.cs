using FirstWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstWebApi.Infrastructure.Data.Configurations;

public class ComicTypeConfiguration : IEntityTypeConfiguration<ComicType>
{
    public void Configure(EntityTypeBuilder<ComicType> builder)
    {
        builder.ToTable("ComicTypes");

        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Nome)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(ct => ct.Nome)
            .IsUnique();
    }
}
