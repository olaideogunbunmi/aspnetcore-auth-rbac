using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoleBasedAuthenticationApi.Models;

namespace RoleBasedAuthenticationApi.Configuration
{
    public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);
            
            //to auto generate GUID in DB
            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(512);

            builder.HasIndex(x => x.TokenHash)
                .IsUnique();

            builder.HasIndex(x => x.Id)
                .IsUnique();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(x => x.ExpiredAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .HasConstraintName("FK_RefreshToken_AspNetUsers")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ReplaceByToken)
                .WithMany()
                .HasForeignKey(x => x.ReplaceByTokenId)
                .OnDelete(DeleteBehavior.Restrict);           
        }
    }
}
