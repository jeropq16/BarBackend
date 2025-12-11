using Barber.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barber.Infrastructure.Config;

public class HairCutConfig : IEntityTypeConfiguration<HairCut>
{
    public void Configure(EntityTypeBuilder<HairCut> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Price)
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.DurationMinutes)
            .IsRequired();
    }
}
