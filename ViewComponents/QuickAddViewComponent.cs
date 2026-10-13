using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.ViewComponents
{
    // Renders the "quick add expense" modal on every authenticated page
    public class QuickAddViewComponent : ViewComponent
    {
        private readonly ExpenseService _expenseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuickAddViewComponent(ExpenseService expenseService, UserManager<ApplicationUser> userManager)
        {
            _expenseService = expenseService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User)!;
            return View(await _expenseService.GetCategoriesAsync(userId));
        }
    }
}
