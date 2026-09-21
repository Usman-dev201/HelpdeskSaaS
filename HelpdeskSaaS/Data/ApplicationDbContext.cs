using HelpdeskSaaS.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSaaS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TicketStatusHistory> TicketStatusHistories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =====================================================
            // TENANT
            // =====================================================

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.TenantId);

                entity.Property(t => t.TenantName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.CreatedAt)
                    .IsRequired();
            });


            // =====================================================
            // USER
            // =====================================================

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.UserName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .IsRequired();


                // Tenant 1 ----> Many Users
                entity.HasOne(u => u.Tenant)
                    .WithMany(t => t.Users)
                    .HasForeignKey(u => u.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // TICKET
            // =====================================================

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.TicketId);

                entity.Property(t => t.TicketTitle)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Description)
                    .IsRequired();

                entity.Property(t => t.Status)
                    .IsRequired();

                entity.Property(t => t.Priority)
                    .IsRequired();

                entity.Property(t => t.Category)
                    .IsRequired()
                    .HasMaxLength(100);


                // User 1 ----> Many Created Tickets
                entity.HasOne(t => t.CreatedByUser)
                    .WithMany(u => u.CreatedTickets)
                    .HasForeignKey(t => t.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Cascade);


                // User 1 ----> Many Assigned Tickets
              
                entity.HasOne(t => t.AssignedAgent)
                    .WithMany(u => u.AssignedTickets)
                    .HasForeignKey(t => t.AssignedAgentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Tenant)
                    .WithMany(t => t.Tickets)
                    .HasForeignKey(t => t.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

            });


            // =====================================================
            // COMMENT
            // =====================================================

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.CommentId);

                entity.Property(c => c.Message)
                    .IsRequired();

                entity.Property(c => c.CreatedAt)
                    .IsRequired();


                // Ticket 1 ----> Many Comments
                entity.HasOne(c => c.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                     .OnDelete(DeleteBehavior.Restrict);
                     });


            // =====================================================
            // TICKET STATUS HISTORY
            // =====================================================

            modelBuilder.Entity<TicketStatusHistory>(entity =>
            {
                entity.HasKey(h => h.HistoryId);

                entity.Property(h => h.OldStatus)
                    .IsRequired();

                entity.Property(h => h.NewStatus)
                    .IsRequired();

                entity.Property(h => h.ChangedAt)
                    .IsRequired();


                // Ticket 1 ----> Many Status History Records
                entity.HasOne(h => h.Ticket)
                    .WithMany(t => t.StatusHistories)
                    .HasForeignKey(h => h.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);


                // User 1 ----> Many Status History Records
                entity.HasOne(h => h.ChangedByUser)
                    .WithMany(u => u.StatusChanges)
                    .HasForeignKey(h => h.ChangedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
