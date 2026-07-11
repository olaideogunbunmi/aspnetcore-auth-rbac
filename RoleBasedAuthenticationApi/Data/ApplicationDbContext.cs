using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RoleBasedAuthenticationApi.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
        {
            
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            //1. Create a DB sequence starting at 100000
            builder.HasSequence<int>("UserPublicIdSequence") //is the Sequence object name
                   .StartsAt(100000)
                   .IncrementsBy(1);

            //2. Automatically apply it to the PublicId field 
            builder.Entity<ApplicationUser>()
                   .Property(u => u.PublicId)
                   .HasDefaultValueSql("NEXT VALUE FOR UserPublicIdSequence")
                   .IsRequired();

            //3. Add unique index using rhe
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.PublicId)
                .IsUnique();

            //4 dates timespan
            builder.Entity<ApplicationUser>()
                .Property(u => u.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

        }
    }
}
