using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Token)
            .HasMaxLength(256)
            .IsRequired();
            
        builder.Property(rt => rt.DeviceInfo)
            .HasMaxLength(512);
            
        builder.Property(rt => rt.ExpiryDate)
            .IsRequired();
            
        builder.Property(rt => rt.CreatedDate)
            .IsRequired();
    }
}
