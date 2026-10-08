using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class ExpenseService
    {
        private readonly ApplicationDbContext _context;

        public ExpenseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Expense>> GetAllByUserAsync(string userId)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<Expense>> GetByMonthAsync(string userId, int month, int year)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId
                         && e.Date.Month == month
                         && e.Date.Year == year)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<Expense?> GetByIdAsync(int id, string userId)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }

        public async Task AddAsync(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Expense expense)
        {
            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var expense = await GetByIdAsync(id, userId);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var now = DateTime.Today;
            var allExpenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var thisMonthExpenses = allExpenses
                .Where(e => e.Date.Month == now.Month && e.Date.Year == now.Year)
                .ToList();

            var lastMonth = now.AddMonths(-1);
            var totalLastMonth = allExpenses
                .Where(e => e.Date.Month == lastMonth.Month && e.Date.Year == lastMonth.Year)
                .Sum(e => e.Amount);

            var categoryBreakdown = thisMonthExpenses
                .GroupBy(e => e.Category)
                .Select(g => new CategorySpendingItem
                {
                    CategoryName = g.Key.Name,
                    Color = g.Key.Color,
                    Total = g.Sum(e => e.Amount)
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            // Daily spending for this month (all days 1..end of month)
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var dailySpending = Enumerable.Range(1, daysInMonth).Select(day =>
            {
                var total = thisMonthExpenses
                    .Where(e => e.Date.Day == day)
                    .Sum(e => e.Amount);
                return new DailySpendingItem
                {
                    Day = new DateTime(now.Year, now.Month, day).ToString("MMM dd"),
                    Total = total
                };
            }).ToList();

            return new DashboardViewModel
            {
                TotalThisMonth = thisMonthExpenses.Sum(e => e.Amount),
                TotalLastMonth = totalLastMonth,
                TotalThisYear = allExpenses.Where(e => e.Date.Year == now.Year).Sum(e => e.Amount),
                TotalAllTime = allExpenses.Sum(e => e.Amount),
                ExpenseCountThisMonth = thisMonthExpenses.Count,
                CategoryBreakdown = categoryBreakdown,
                DailySpending = dailySpending,
                RecentExpenses = allExpenses.Take(6).ToList(),
                TopCategory = categoryBreakdown.FirstOrDefault()?.CategoryName ?? "N/A"
            };
        }

        public async Task<List<Category>> GetCategoriesAsync(string userId)
        {
            return await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
