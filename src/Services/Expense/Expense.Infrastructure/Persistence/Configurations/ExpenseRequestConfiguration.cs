using Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense.Infrastructure.Persistence.Configurations
{
    public class ExpenseRequestConfiguration : IEntityTypeConfiguration<ExpenseRequest>
    {
        public void Configure(EntityTypeBuilder<ExpenseRequest> builder)
        {
            builder.ToTable("ExpenseRequests");

            builder.HasKey(er => er.Id);

            builder.Property(er => er.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(er => er.Description)
                .IsRequired()
                .HasMaxLength(300);
            
            builder.Property(er => er.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.HasMany(er => er.Approvals)
                .WithOne(a => a.ExpenseRequest)
                .HasForeignKey(a => a.ExpenseRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}