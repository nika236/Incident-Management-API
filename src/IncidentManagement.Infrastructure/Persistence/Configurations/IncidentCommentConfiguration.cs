using IncidentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentManagement.Infrastructure.Persistence.Configurations;

public class IncidentCommentConfiguration : IEntityTypeConfiguration<IncidentComment>
{
    public void Configure(EntityTypeBuilder<IncidentComment> builder)
    {
        builder.ToTable("incident_comments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Body).HasMaxLength(4000).IsRequired();
        builder.HasOne(c => c.Incident).WithMany(i => i.Comments).HasForeignKey(c => c.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Author).WithMany(u => u.Comments).HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
