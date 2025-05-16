using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Configurations;

public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.HasKey(uc => uc.Id);
        
        // Chỉ định tên bảng trong database
        builder.ToTable("UserClaims");
        
        builder.Property(uc => uc.ClaimType)
            .HasMaxLength(256)
            .IsRequired();
            
        builder.Property(uc => uc.ClaimValue)
            .HasMaxLength(2048);
            
        // Tạo index để tìm kiếm claims theo UserId
        builder.HasIndex(uc => uc.UserId);
        
        // Relationship với User đã được cấu hình bên UserConfiguration
    }
}
