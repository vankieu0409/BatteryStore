using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.UserName)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();
        });
        
        builder.Property(u => u.PasswordHash)
            .IsRequired();
            
        // Cấu hình relationship với Role thông qua UserRole
        builder.HasMany<Role>()
            .WithMany()
            .UsingEntity<UserRole>(
                j => j.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId),
                j => j.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(ur => ur.UserId),
                j => j.HasKey(ur => new { ur.UserId, ur.RoleId })
            );
            
        // Cấu hình relationship với RefreshToken
        builder.HasMany<RefreshToken>()
            .WithOne()
            .HasForeignKey(rt => rt.UserId);
            
        // Cấu hình relationship với UserClaim
        builder.HasMany<UserClaim>()
            .WithOne()
            .HasForeignKey(uc => uc.UserId);
            
        // Cấu hình relationship với UserLogin
        builder.HasMany<UserLogin>()
            .WithOne()
            .HasForeignKey(ul => ul.UserId);
            
        // Cấu hình relationship với UserToken
        builder.HasMany<UserToken>()
            .WithOne()
            .HasForeignKey(ut => ut.UserId);
            
        // Bỏ qua các domain events khi lưu vào database
        builder.Ignore(e => e.DomainEvents);
    }
}
