using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpenseTracker.Models.ViewModels
{
    public class ExpenseViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public SelectList? Categories { get; set; }
    }
}
