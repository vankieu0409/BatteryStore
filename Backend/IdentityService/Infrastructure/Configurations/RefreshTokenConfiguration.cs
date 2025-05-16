using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        
        // Chỉ định tên bảng trong database
        builder.ToTable("RefreshTokens");
        
        builder.Property(rt => rt.Token)
            .HasMaxLength(256)
            .IsRequired();
        
        // Tạo index cho trường Token để tìm kiếm nhanh
        builder.HasIndex(rt => rt.Token);
            
        builder.Property(rt => rt.DeviceInfo)
            .HasMaxLength(512);
            
        builder.Property(rt => rt.ExpiryDate)
            .IsRequired();
            
        builder.Property(rt => rt.CreatedDate)
            .IsRequired();
            
        // Tạo index cho UserId để tăng tốc độ truy vấn
        builder.HasIndex(rt => rt.UserId);
        
        // Cấu hình relationship với User (mặc dù đã được cấu hình bên UserConfiguration)
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
