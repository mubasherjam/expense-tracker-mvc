using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
