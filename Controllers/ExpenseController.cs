using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Models.ViewModels;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly ExpenseService _expenseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpenseController(ExpenseService expenseService,
                                 UserManager<ApplicationUser> userManager)
        {
            _expenseService = expenseService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, string? month)
        {
            var userId = _userManager.GetUserId(User)!;
            var expenses = await _expenseService.GetAllByUserAsync(userId);

            // Filters
            if (!string.IsNullOrWhiteSpace(search))
                expenses = expenses.Where(e => e.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                                             || (e.Notes != null && e.Notes.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();

            if (categoryId.HasValue && categoryId > 0)
                expenses = expenses.Where(e => e.CategoryId == categoryId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(month) && DateTime.TryParse(month + "-01", out var monthDate))
                expenses = expenses.Where(e => e.Date.Month == monthDate.Month && e.Date.Year == monthDate.Year).ToList();

            var categories = await _expenseService.GetCategoriesAsync(userId);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Search = search;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.Month = month;

            return View(expenses);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User)!;
            var categories = await _expenseService.GetCategoriesAsync(userId);
            var vm = new ExpenseViewModel
            {
                Categories = new SelectList(categories, "Id", "Name"),
                Date = DateTime.Today
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;

            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(await _expenseService.GetCategoriesAsync(userId), "Id", "Name");
                return View(model);
            }

            var expense = new Expense
            {
                Title = model.Title,
                Amount = model.Amount,
                Date = model.Date,
                Notes = model.Notes,
                CategoryId = model.CategoryId,
                UserId = userId
            };

            await _expenseService.AddAsync(expense);
            TempData["Success"] = "Expense added successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var expense = await _expenseService.GetByIdAsync(id, userId);
            if (expense == null) return NotFound();

            var categories = await _expenseService.GetCategoriesAsync(userId);
            var vm = new ExpenseViewModel
            {
                Id = expense.Id,
                Title = expense.Title,
                Amount = expense.Amount,
                Date = expense.Date,
                Notes = expense.Notes,
                CategoryId = expense.CategoryId,
                Categories = new SelectList(categories, "Id", "Name", expense.CategoryId)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;

            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(await _expenseService.GetCategoriesAsync(userId), "Id", "Name");
                return View(model);
            }

            var expense = await _expenseService.GetByIdAsync(id, userId);
            if (expense == null) return NotFound();

            expense.Title = model.Title;
            expense.Amount = model.Amount;
            expense.Date = model.Date;
            expense.Notes = model.Notes;
            expense.CategoryId = model.CategoryId;

            await _expenseService.UpdateAsync(expense);
            TempData["Success"] = "Expense updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _expenseService.DeleteAsync(id, userId);
            TempData["Success"] = "Expense deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
