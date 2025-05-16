using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Configurations;

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        // Primary key là composite key từ LoginProvider và ProviderKey
        builder.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
        
        // Chỉ định tên bảng trong database
        builder.ToTable("UserLogins");
        
        builder.Property(ul => ul.LoginProvider)
            .HasMaxLength(128)
            .IsRequired();
            
        builder.Property(ul => ul.ProviderKey)
            .HasMaxLength(256)
            .IsRequired();
            
        builder.Property(ul => ul.ProviderDisplayName)
            .HasMaxLength(128);
            
        // Tạo index để tìm kiếm logins theo UserId
        builder.HasIndex(ul => ul.UserId);
        
        // Relationship với User đã được cấu hình bên UserConfiguration
    }
}
