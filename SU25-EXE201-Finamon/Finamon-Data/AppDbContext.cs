using Microsoft.EntityFrameworkCore;
using Finamon_Data.Entities;

namespace Finamon_Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        // DbSet properties for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetAlert> BudgetAlerts { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<UserMembership> UserMemberships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure One-to-Many: User -> Expenses
            modelBuilder.Entity<User>()
                .HasMany(u => u.Expenses)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure One-to-Many: Role -> Users
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure One-to-Many: Category -> Expenses
            modelBuilder.Entity<Category>()
                .HasMany<Expense>()
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure One-to-Many: Budget -> Expenses (optional relationship)
            modelBuilder.Entity<Budget>()
                .HasMany<Expense>()
                .WithOne(e => e.Budget)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
                
            // Configure Many-to-Many: User <-> Membership via UserMembership
            modelBuilder.Entity<UserMembership>()
                .HasOne<User>()
                .WithMany(u => u.UserMemberships)
                .HasForeignKey(um => um.UserId);
                
            modelBuilder.Entity<UserMembership>()
                .HasOne<Membership>()
                .WithMany()
                .HasForeignKey(um => um.MembershipId);
                
            // Configure One-to-Many: User -> ChatSessions
            modelBuilder.Entity<User>()
                .HasMany(u => u.ChatSessions)
                .WithOne()
                .HasForeignKey(cs => cs.UserId);
                
            // Configure One-to-Many: User -> Reports
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reports)
                .WithOne()
                .HasForeignKey(r => r.UserId);
                
            // Configure soft delete filter
            modelBuilder.Entity<User>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Expense>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Budget>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Category>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Report>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Blog>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<ChatSession>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Chat>().HasQueryFilter(m => !m.IsDelete);
        }
    }
}
