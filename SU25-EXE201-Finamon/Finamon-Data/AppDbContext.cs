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
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetAlert> BudgetAlerts { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<CategoryAlert> CategoryAlerts { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<UserMembership> UserMemberships { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<SiteAnalytic> SiteAnalytics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Many-to-Many: User <-> Role via UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure One-to-Many: User -> Budgets
            modelBuilder.Entity<User>()
                .HasMany(u => u.Budgets)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-Many: Budget -> BudgetAlerts
            modelBuilder.Entity<Budget>()
                .HasMany(b => b.Alerts)
                .WithOne(ba => ba.Budget)
                .HasForeignKey(ba => ba.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-Many: BudgetCategory -> CategoryAlerts
            modelBuilder.Entity<BudgetCategory>()
                .HasMany(bc => bc.CategoryAlerts)
                .WithOne(ca => ca.BudgetCategory)
                .HasForeignKey(ca => ca.BudgetCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-Many: User -> Expenses
            modelBuilder.Entity<User>()
                .HasMany(u => u.Expenses)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure One-to-Many: Category -> Expenses
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Expenses)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure One-to-Many: User -> Categories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure Many-to-Many: User <-> Membership via UserMembership
            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.User)
                .WithMany(u => u.UserMemberships)
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.Membership)
                .WithMany(m => m.UserMemberships)
                .HasForeignKey(um => um.MembershipId)
                .OnDelete(DeleteBehavior.Restrict);
                
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
            modelBuilder.Entity<BudgetCategory>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<BudgetAlert>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<CategoryAlert>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Comment>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<SiteAnalytic>().HasQueryFilter(m => !m.IsDelete);

            // Configure One-to-Many: Category -> BudgetCategories
            modelBuilder.Entity<Category>()
                .HasMany(c => c.BudgetCategorys)
                .WithOne(bc => bc.Category)
                .HasForeignKey(bc => bc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure One-to-Many: User -> Comments
            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-Many: Blog -> Comments
            modelBuilder.Entity<Blog>()
                .HasMany(b => b.Comments)
                .WithOne(c => c.Blog)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-Many: User -> UserActivities
            modelBuilder.Entity<UserActivity>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserActivities)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure soft delete for UserActivity
            modelBuilder.Entity<UserActivity>().HasQueryFilter(m => !m.IsDelete);

            //SeedData
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Staff" },
                new Role { Id = 3, Name = "Customer" }
                );
        }
    }
}
