using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartKasir.Core.Entities;

namespace SmartKasir.Infrastructure.Persistence.Configurations;

public class TransactionItemConfiguration : IEntityTypeConfiguration<TransactionItem>
{
    public void Configure(EntityTypeBuilder<TransactionItem> builder)
    {
        builder.ToTable("TransactionItems");

        builder.HasKey(ti => ti.Id);

        builder.Property(ti => ti.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ti => ti.Quantity)
            .IsRequired();

        builder.Property(ti => ti.PriceAtMoment)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ti => ti.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(ti => ti.Transaction)
            .WithMany(t => t.Items)
            .HasForeignKey(ti => ti.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ti => ti.Product)
            .WithMany()
            .HasForeignKey(ti => ti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
