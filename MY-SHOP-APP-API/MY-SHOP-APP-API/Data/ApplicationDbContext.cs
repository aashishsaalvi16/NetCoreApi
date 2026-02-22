using Microsoft.EntityFrameworkCore;
using MY_SHOP_APP_API.Models;

namespace MY_SHOP_APP_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserMaster> UserMasters { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserMaster>(entity =>
            {
                entity.ToTable("tblUserMaster");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).HasColumnName("UserId");

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(10);
                entity.Property(e => e.Email).HasMaxLength(50);
                entity.Property(e => e.Address1).HasMaxLength(100);
                entity.Property(e => e.Address2).HasMaxLength(100);
                entity.Property(e => e.CreatedOn).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.ModifiedOn);
                entity.Property(e => e.ModifiedBy).HasMaxLength(50);
                entity.Property(e => e.IsActive).IsRequired();
            });
        }
    }
}
