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
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ExpenseService expenseService,
                                   UserManager<ApplicationUser> userManager)
        {
            _expenseService = expenseService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _expenseService.GetDashboardDataAsync(userId);
            return View(vm);
        }
    }
}
