using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(r => r.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.HasIndex(r => r.NormalizedName)
            .IsUnique();
    }
}
