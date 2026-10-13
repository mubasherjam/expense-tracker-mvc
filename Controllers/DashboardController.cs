using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ExpenseService _expenseService;
        private readonly BudgetService _budgetService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ExpenseService expenseService,
                                   BudgetService budgetService,
                                   UserManager<ApplicationUser> userManager)
        {
            _expenseService = expenseService;
            _budgetService = budgetService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _expenseService.GetDashboardDataAsync(userId);
            vm.Budgets = (await _budgetService.GetProgressAsync(userId))
                .Where(b => b.HasBudget)
                .OrderByDescending(b => b.Percent)
                .ToList();
            return View(vm);
        }
    }
}
