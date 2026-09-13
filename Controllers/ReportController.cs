using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ExpenseService _expenseService;
        private readonly ReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(ExpenseService expenseService,
                                ReportService reportService,
                                UserManager<ApplicationUser> userManager)
        {
            _expenseService = expenseService;
            _reportService = reportService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var now = DateTime.Today;
            var selectedMonth = month ?? now.Month;
            var selectedYear = year ?? now.Year;

            var userId = _userManager.GetUserId(User)!;
            var expenses = await _expenseService.GetByMonthAsync(userId, selectedMonth, selectedYear);
            var vm = _reportService.BuildReportViewModel(expenses, selectedMonth, selectedYear);

            return View(vm);
        }

        public async Task<IActionResult> ExportExcel(int month, int year)
        {
            var userId = _userManager.GetUserId(User)!;
            var expenses = await _expenseService.GetByMonthAsync(userId, month, year);
            var vm = _reportService.BuildReportViewModel(expenses, month, year);

            var bytes = _reportService.ExportToExcel(vm);
            var fileName = $"ExpenseReport_{new DateTime(year, month, 1):MMMM_yyyy}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> ExportPdf(int month, int year)
        {
            var userId = _userManager.GetUserId(User)!;
            var expenses = await _expenseService.GetByMonthAsync(userId, month, year);
            var vm = _reportService.BuildReportViewModel(expenses, month, year);

            var bytes = _reportService.ExportToPdf(vm);
            var fileName = $"ExpenseReport_{new DateTime(year, month, 1):MMMM_yyyy}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
    }
}
