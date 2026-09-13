using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Color { get; set; } = "#6c757d"; // Bootstrap default

        [MaxLength(50)]
        public string Icon { get; set; } = "fas fa-tag";

        public bool IsDefault { get; set; } = false; // system-seeded categories

        // Nullable FK — null means it's a default/global category
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
