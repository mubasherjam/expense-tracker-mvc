using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly BudgetService _budgetService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BudgetController(BudgetService budgetService, UserManager<ApplicationUser> userManager)
        {
            _budgetService = budgetService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            return View(await _budgetService.GetProgressAsync(userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int categoryId, decimal amount)
        {
            var userId = _userManager.GetUserId(User)!;
            await _budgetService.SaveAsync(userId, categoryId, amount);
            TempData["Success"] = amount > 0 ? "Budget saved." : "Budget removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
