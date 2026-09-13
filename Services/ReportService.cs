using ExpenseTracker.Models;
using ExpenseTracker.Models.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SysColor = System.Drawing.Color;
using SysColorTranslator = System.Drawing.ColorTranslator;

namespace ExpenseTracker.Services
{
    public class ReportService
    {
        public ReportViewModel BuildReportViewModel(List<Expense> expenses, int month, int year)
        {
            var grandTotal = expenses.Sum(e => e.Amount);

            var breakdown = expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategoryReportItem
                {
                    CategoryName = g.Key.Name,
                    Color = g.Key.Color,
                    Icon = g.Key.Icon,
                    Total = g.Sum(e => e.Amount),
                    Count = g.Count(),
                    Percentage = grandTotal > 0
                        ? Math.Round((g.Sum(e => e.Amount) / grandTotal) * 100, 1)
                        : 0
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            var availableYears = Enumerable.Range(DateTime.Today.Year - 5, 6).Reverse().ToList();

            return new ReportViewModel
            {
                Month = month,
                Year = year,
                GrandTotal = grandTotal,
                CategoryBreakdown = breakdown,
                Expenses = expenses,
                AvailableYears = availableYears
            };
        }

        public byte[] ExportToExcel(ReportViewModel report)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Expense Report");
            var monthName = new DateTime(report.Year, report.Month, 1).ToString("MMMM yyyy");

            // === Title ===
            ws.Cells["A1:F1"].Merge = true;
            ws.Cells["A1"].Value = $"Expense Report – {monthName}";
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.Font.Size = 16;
            ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(SysColorTranslator.FromHtml("#2c3e50"));
            ws.Cells["A1"].Style.Font.Color.SetColor(SysColor.White);

            // === Category Summary ===
            ws.Cells["A3"].Value = "Category Summary";
            ws.Cells["A3"].Style.Font.Bold = true;
            ws.Cells["A3"].Style.Font.Size = 12;

            string[] summaryHeaders = { "Category", "# Expenses", "Total (PKR)", "% of Total" };
            for (int i = 0; i < summaryHeaders.Length; i++)
            {
                ws.Cells[4, i + 1].Value = summaryHeaders[i];
                ws.Cells[4, i + 1].Style.Font.Bold = true;
                ws.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(SysColorTranslator.FromHtml("#3498db"));
                ws.Cells[4, i + 1].Style.Font.Color.SetColor(SysColor.White);
            }

            int row = 5;
            foreach (var item in report.CategoryBreakdown)
            {
                ws.Cells[row, 1].Value = item.CategoryName;
                ws.Cells[row, 2].Value = item.Count;
                ws.Cells[row, 3].Value = item.Total;
                ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 4].Value = $"{item.Percentage}%";
                row++;
            }

            // Total row
            ws.Cells[row, 1].Value = "TOTAL";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 3].Value = report.GrandTotal;
            ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 3].Style.Font.Bold = true;
            row += 2;

            // === Expense Details ===
            ws.Cells[row, 1].Value = "Expense Details";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 12;
            row++;

            string[] detailHeaders = { "#", "Date", "Title", "Category", "Notes", "Amount (PKR)" };
            for (int i = 0; i < detailHeaders.Length; i++)
            {
                ws.Cells[row, i + 1].Value = detailHeaders[i];
                ws.Cells[row, i + 1].Style.Font.Bold = true;
                ws.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(SysColorTranslator.FromHtml("#2ecc71"));
                ws.Cells[row, i + 1].Style.Font.Color.SetColor(SysColor.White);
            }
            row++;

            int idx = 1;
            foreach (var e in report.Expenses)
            {
                ws.Cells[row, 1].Value = idx++;
                ws.Cells[row, 2].Value = e.Date.ToString("dd MMM yyyy");
                ws.Cells[row, 3].Value = e.Title;
                ws.Cells[row, 4].Value = e.Category?.Name;
                ws.Cells[row, 5].Value = e.Notes;
                ws.Cells[row, 6].Value = e.Amount;
                ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                row++;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportToPdf(ReportViewModel report)
        {
            var monthName = new DateTime(report.Year, report.Month, 1).ToString("MMMM yyyy");

            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 40f, 40f, 60f, 40f);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // Fonts
            var titleFont   = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(44, 62, 80));
            var headerFont  = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.WHITE);
            var bodyFont    = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
            var boldBody    = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
            var subTitle    = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, new BaseColor(52, 73, 94));

            // Title
            var title = new Paragraph($"Expense Report – {monthName}\n", titleFont) { Alignment = Element.ALIGN_CENTER };
            doc.Add(title);
            doc.Add(new Paragraph($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}\n\n", bodyFont));

            // Summary cards
            doc.Add(new Paragraph("Summary", subTitle));
            doc.Add(new Paragraph($"Total Expenses: PKR {report.GrandTotal:N2}    |    Transactions: {report.Expenses.Count}\n\n", boldBody));

            // Category Breakdown Table
            doc.Add(new Paragraph("Category Breakdown\n", subTitle));
            var catTable = new PdfPTable(4) { WidthPercentage = 100 };
            catTable.SetWidths(new float[] { 3f, 1.5f, 2f, 1.5f });

            Action<PdfPTable, string, iTextSharp.text.Font, BaseColor> addCell = (tbl, text, font, bg) =>
            {
                var cell = new PdfPCell(new Phrase(text, font))
                {
                    BackgroundColor = bg,
                    Padding = 6,
                    BorderColor = new BaseColor(220, 220, 220)
                };
                tbl.AddCell(cell);
            };

            var headerBg = new BaseColor(52, 152, 219);
            addCell(catTable, "Category",    headerFont, headerBg);
            addCell(catTable, "# Expenses",  headerFont, headerBg);
            addCell(catTable, "Total (PKR)", headerFont, headerBg);
            addCell(catTable, "% of Total",  headerFont, headerBg);

            bool alt = false;
            foreach (var item in report.CategoryBreakdown)
            {
                var rowBg = alt ? new BaseColor(245, 245, 245) : BaseColor.WHITE;
                addCell(catTable, item.CategoryName,         bodyFont, rowBg);
                addCell(catTable, item.Count.ToString(),     bodyFont, rowBg);
                addCell(catTable, $"PKR {item.Total:N2}",   bodyFont, rowBg);
                addCell(catTable, $"{item.Percentage}%",     bodyFont, rowBg);
                alt = !alt;
            }

            // Total row
            var totalBg = new BaseColor(236, 240, 241);
            addCell(catTable, "TOTAL",                           boldBody, totalBg);
            addCell(catTable, report.Expenses.Count.ToString(),  boldBody, totalBg);
            addCell(catTable, $"PKR {report.GrandTotal:N2}",    boldBody, totalBg);
            addCell(catTable, "100%",                            boldBody, totalBg);

            doc.Add(catTable);
            doc.Add(new Paragraph("\n"));

            // Expense Details Table
            doc.Add(new Paragraph("Expense Details\n", subTitle));
            var expTable = new PdfPTable(5) { WidthPercentage = 100 };
            expTable.SetWidths(new float[] { 2f, 3f, 2f, 3f, 2f });

            var greenBg = new BaseColor(46, 204, 113);
            addCell(expTable, "Date",     headerFont, greenBg);
            addCell(expTable, "Title",    headerFont, greenBg);
            addCell(expTable, "Category", headerFont, greenBg);
            addCell(expTable, "Notes",    headerFont, greenBg);
            addCell(expTable, "Amount",   headerFont, greenBg);

            alt = false;
            foreach (var e in report.Expenses)
            {
                var rowBg = alt ? new BaseColor(245, 245, 245) : BaseColor.WHITE;
                addCell(expTable, e.Date.ToString("dd MMM yyyy"),   bodyFont, rowBg);
                addCell(expTable, e.Title,                          bodyFont, rowBg);
                addCell(expTable, e.Category?.Name ?? "",           bodyFont, rowBg);
                addCell(expTable, e.Notes ?? "-",                   bodyFont, rowBg);
                addCell(expTable, $"PKR {e.Amount:N2}",            bodyFont, rowBg);
                alt = !alt;
            }

            doc.Add(expTable);
            doc.Close();
            return ms.ToArray();
        }
    }
}
