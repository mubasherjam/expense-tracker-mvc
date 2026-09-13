using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.ViewModels
{
    public class ReportViewModel
    {
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Today.Year;

        public decimal GrandTotal { get; set; }

        public List<CategoryReportItem> CategoryBreakdown { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();

        // For month/year dropdowns
        public List<int> AvailableYears { get; set; } = new();
    }

    public class CategoryReportItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}
