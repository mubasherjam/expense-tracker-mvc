using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class BudgetService
    {
        private readonly ApplicationDbContext _context;

        public BudgetService(ApplicationDbContext context) => _context = context;

        // One row per category visible to the user, with this month's spend and budget (if any)
        public async Task<List<BudgetProgressItem>> GetProgressAsync(string userId, DateTime? forMonth = null)
        {
            var month = forMonth ?? DateTime.Today;
            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId)
                .ToDictionaryAsync(b => b.CategoryId, b => b.Amount);

            var spent = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == month.Month && e.Date.Year == month.Year)
                .GroupBy(e => e.CategoryId)
                .Select(g => new { g.Key, Total = g.Sum(e => e.Amount) })
                .ToDictionaryAsync(x => x.Key, x => x.Total);

            return categories.Select(c => new BudgetProgressItem
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                Color = c.Color,
                Icon = c.Icon,
                Budget = budgets.GetValueOrDefault(c.Id),
                Spent = spent.GetValueOrDefault(c.Id)
            }).ToList();
        }

        // amount <= 0 removes the budget
        public async Task SaveAsync(string userId, int categoryId, decimal amount)
        {
            var visible = await _context.Categories
                .AnyAsync(c => c.Id == categoryId && (c.UserId == null || c.UserId == userId));
            if (!visible) return;

            var existing = await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == categoryId);

            if (amount <= 0)
            {
                if (existing != null) _context.Budgets.Remove(existing);
            }
            else if (existing != null)
            {
                existing.Amount = amount;
            }
            else
            {
                _context.Budgets.Add(new Budget { UserId = userId, CategoryId = categoryId, Amount = amount });
            }

            await _context.SaveChangesAsync();
        }
    }
}
