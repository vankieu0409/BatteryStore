using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Primary key là composite key từ UserId và RoleId
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        
        // Chỉ định tên bảng trong database
        builder.ToTable("UserRoles");
        
        // Tạo các indexes cho các lookup phổ biến
        builder.HasIndex(ur => ur.UserId);
        builder.HasIndex(ur => ur.RoleId);
        
        // Cấu hình relationship với User
        // Note: Relationship chính giữa User và Role đã được cấu hình trong UserConfiguration
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Cấu hình relationship với Role
        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
