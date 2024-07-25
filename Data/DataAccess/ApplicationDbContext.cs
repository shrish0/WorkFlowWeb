using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using WorkFlowWeb.Models;

namespace WorkFlowWeb.Data.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<LastUserId> LastUserIds { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Define relationships and default values
            builder.Entity<Category>()
            .HasIndex(c => c.Code)
            .IsUnique();

            builder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(sc => sc.Category)
                .HasForeignKey(sc => sc.CategoryId);

            builder.Entity<Category>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Category>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.Entity<SubCategory>()
                .Property(sc => sc.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<SubCategory>()
                .Property(sc => sc.IsActive)
                .HasDefaultValue(true);
        }
    }

    public class LastUserId
    {
        public int Id { get; set; }
        public int LastId { get; set; }
    }
}
