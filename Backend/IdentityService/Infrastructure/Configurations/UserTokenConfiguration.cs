using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Configurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        // Primary key là composite key từ UserId, LoginProvider và Name
        builder.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
        
        // Chỉ định tên bảng trong database
        builder.ToTable("UserTokens");
        
        builder.Property(ut => ut.LoginProvider)
            .HasMaxLength(128)
            .IsRequired();
            
        builder.Property(ut => ut.Name)
            .HasMaxLength(128)
            .IsRequired();
            
        builder.Property(ut => ut.Value)
            .HasMaxLength(4096); // Token values có thể dài
        
        // Relationship với User đã được cấu hình bên UserConfiguration
        // Nhưng cũng có thể định nghĩa lại ở đây nếu cần thiết
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
