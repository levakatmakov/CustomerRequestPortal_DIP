using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CustomerRequestPortal.Models;

namespace CustomerRequestPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<CustomerRequest> CustomerRequests { get; set; }
        public DbSet<RequestItem> RequestItems { get; set; }
        public DbSet<RequestStatusHistory> RequestStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CustomerRequest>()
                .HasIndex(r => r.RequestNumber)
                .IsUnique();

            builder.Entity<CustomerRequest>()
                .HasOne(r => r.AssignedExecutor)
                .WithMany()
                .HasForeignKey(r => r.AssignedExecutorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<RequestStatusHistory>()
                .HasOne(h => h.Request)
                .WithMany(r => r.StatusHistory)
                .HasForeignKey(h => h.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RequestStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
