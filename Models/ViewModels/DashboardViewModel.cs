namespace ExpenseTracker.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalThisMonth { get; set; }
        public decimal TotalLastMonth { get; set; }

        // % change vs last month; null when there is no last-month spending to compare with
        public decimal? MonthChangePercent =>
            TotalLastMonth > 0 ? Math.Round((TotalThisMonth - TotalLastMonth) / TotalLastMonth * 100, 1) : null;

        public decimal TotalThisYear { get; set; }
        public decimal TotalAllTime { get; set; }
        public int ExpenseCountThisMonth { get; set; }

        // For Doughnut chart – category breakdown
        public List<CategorySpendingItem> CategoryBreakdown { get; set; } = new();

        // For Line chart – daily spending this month
        public List<DailySpendingItem> DailySpending { get; set; } = new();

        // Recent transactions
        public List<Expense> RecentExpenses { get; set; } = new();

        // Top spending category this month
        public List<BudgetProgressItem> Budgets { get; set; } = new();

        public string TopCategory { get; set; } = "N/A";
    }

    public class BudgetProgressItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }

        public bool HasBudget => Budget > 0;
        public decimal Remaining => Budget - Spent;
        public int Percent => HasBudget ? (int)Math.Min(999, Math.Round(Spent / Budget * 100)) : 0;
        // ok below 80%, warning from 80%, over from 100%
        public string State => !HasBudget ? "none" : Percent >= 100 ? "over" : Percent >= 80 ? "warning" : "ok";
    }

    public class CategorySpendingItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class DailySpendingItem
    {
        public string Day { get; set; } = string.Empty;   // e.g. "Sep 01"
        public decimal Total { get; set; }
    }
}
