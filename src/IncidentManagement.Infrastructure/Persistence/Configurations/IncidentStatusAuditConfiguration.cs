using IncidentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentManagement.Infrastructure.Persistence.Configurations;

public class IncidentStatusAuditConfiguration : IEntityTypeConfiguration<IncidentStatusAudit>
{
    public void Configure(EntityTypeBuilder<IncidentStatusAudit> builder)
    {
        builder.ToTable("incident_status_audits");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FromStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.ToStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.Note).HasMaxLength(2000);
        builder.HasOne(a => a.Incident).WithMany(i => i.StatusAudits).HasForeignKey(a => a.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.ChangedByUser).WithMany().HasForeignKey(a => a.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
