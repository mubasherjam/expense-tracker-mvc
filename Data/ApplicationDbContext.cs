using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Category -> Expense relationship (restrict delete to avoid cascade conflicts)
            builder.Entity<Expense>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Expense (cascade delete user's expenses when user is deleted)
            builder.Entity<Expense>()
                .HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Category (cascade delete user's custom categories)
            builder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default categories (no UserId = global)
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food & Dining",    Color = "#e74c3c", Icon = "fas fa-utensils",       IsDefault = true },
                new Category { Id = 2, Name = "Transport",        Color = "#3498db", Icon = "fas fa-car",             IsDefault = true },
                new Category { Id = 3, Name = "Shopping",         Color = "#9b59b6", Icon = "fas fa-shopping-bag",    IsDefault = true },
                new Category { Id = 4, Name = "Entertainment",    Color = "#f39c12", Icon = "fas fa-film",            IsDefault = true },
                new Category { Id = 5, Name = "Health",           Color = "#2ecc71", Icon = "fas fa-heartbeat",       IsDefault = true },
                new Category { Id = 6, Name = "Utilities",        Color = "#1abc9c", Icon = "fas fa-bolt",            IsDefault = true },
                new Category { Id = 7, Name = "Rent / Housing",   Color = "#e67e22", Icon = "fas fa-home",            IsDefault = true },
                new Category { Id = 8, Name = "Education",        Color = "#2980b9", Icon = "fas fa-graduation-cap",  IsDefault = true },
                new Category { Id = 9, Name = "Other",            Color = "#95a5a6", Icon = "fas fa-ellipsis-h",      IsDefault = true }
            );
        }
    }
}
