using Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense.Infrastructure.Persistence.Configurations
{
    public class ApprovalConfiguration : IEntityTypeConfiguration<Approval>
    {
        public void Configure(EntityTypeBuilder<Approval> builder)
        {
            builder.ToTable("Approvals");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Note)
                .HasMaxLength(250);
        }
    }
}