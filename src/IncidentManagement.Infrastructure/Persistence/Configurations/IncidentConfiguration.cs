using IncidentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentManagement.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Title).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(8000).IsRequired();
        builder.Property(i => i.Severity).HasConversion<string>().HasMaxLength(16);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.Severity);
        builder.HasIndex(i => i.CreatedAt);
        builder.HasOne(i => i.Assignee).WithMany(u => u.AssignedIncidents).HasForeignKey(i => i.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
