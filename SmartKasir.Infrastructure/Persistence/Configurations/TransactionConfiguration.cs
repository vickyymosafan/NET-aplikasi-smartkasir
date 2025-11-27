using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartKasir.Core.Entities;

namespace SmartKasir.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(t => t.InvoiceNumber)
            .IsUnique();

        builder.Property(t => t.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasOne(t => t.Cashier)
            .WithMany()
            .HasForeignKey(t => t.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Items)
            .WithOne(i => i.Transaction)
            .HasForeignKey(i => i.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
