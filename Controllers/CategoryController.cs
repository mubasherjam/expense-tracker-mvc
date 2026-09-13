using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(ApplicationDbContext context,
                                  UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.IsDefault ? 0 : 1)
                .ThenBy(c => c.Name)
                .ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create() => View(new Category());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User)!;
            model.UserId = userId;
            model.IsDefault = false;

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category created!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var category = await _context.Categories.FindAsync(id);

            if (category == null) return NotFound();
            if (category.IsDefault || category.UserId != userId)
            {
                TempData["Error"] = "You cannot edit default categories.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            var userId = _userManager.GetUserId(User)!;
            var category = await _context.Categories.FindAsync(id);

            if (category == null) return NotFound();
            if (category.IsDefault || category.UserId != userId) return Forbid();

            if (!ModelState.IsValid) return View(model);

            category.Name = model.Name;
            category.Color = model.Color;
            category.Icon = model.Icon;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Category updated!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var category = await _context.Categories
                .Include(c => c.Expenses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();
            if (category.IsDefault || category.UserId != userId)
            {
                TempData["Error"] = "Cannot delete default categories.";
                return RedirectToAction(nameof(Index));
            }

            if (category.Expenses.Any())
            {
                TempData["Error"] = "Cannot delete a category that has expenses. Reassign expenses first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
