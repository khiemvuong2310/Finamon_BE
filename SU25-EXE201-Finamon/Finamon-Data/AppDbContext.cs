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
        public DbSet<BudgetDetail> BudgetDetails { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<UserMembership> UserMemberships { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BlogImage> BlogImages { get; set; }

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

            // Configure One-to-Many: Budget -> BudgetDetails
            modelBuilder.Entity<Budget>()
                .HasMany<BudgetDetail>()
                .WithOne(bd => bd.Budget)
                .HasForeignKey(bd => bd.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-Many: Budget -> BudgetAlerts
            modelBuilder.Entity<Budget>()
                .HasMany(b => b.Alerts)
                .WithOne(ba => ba.Budget)
                .HasForeignKey(ba => ba.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-Many: User -> Expenses
            modelBuilder.Entity<User>()
                .HasMany(u => u.Expenses)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
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
            modelBuilder.Entity<BudgetDetail>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<BudgetAlert>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Image>().HasQueryFilter(m => !m.IsDelete);
            modelBuilder.Entity<Comment>().HasQueryFilter(m => !m.IsDelete);

            // Configure One-to-Many: Category -> BudgetDetails
            modelBuilder.Entity<Category>()
                .HasMany(c => c.BudgetDetails)
                .WithOne(bd => bd.Category)
                .HasForeignKey(bd => bd.CategoryId)
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

            // Configure Many-to-Many: Blog <-> Image via BlogImage
            modelBuilder.Entity<BlogImage>()
                .HasKey(bi => new { bi.BlogId, bi.ImageId });

            modelBuilder.Entity<BlogImage>()
                .HasOne(bi => bi.Blog)
                .WithMany(b => b.PostImages) 
                .HasForeignKey(bi => bi.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BlogImage>()
                .HasOne(bi => bi.Image)
                .WithMany(i => i.BlogImages)
                .HasForeignKey(bi => bi.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            //SeedData
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Staff" },
                new Role { Id = 3, Name = "Customer" }
                );
        }
    }
}
