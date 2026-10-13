using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Services
{
    // Per-request access to the signed-in user's currency, used by views via @inject
    public class CurrencyService
    {
        public static readonly string[] Supported = { "PKR", "USD", "EUR", "GBP", "AED", "SAR", "INR", "CAD", "AUD" };

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _http;
        private string? _code;

        public CurrencyService(UserManager<ApplicationUser> userManager, IHttpContextAccessor http)
        {
            _userManager = userManager;
            _http = http;
        }

        public string Code
        {
            get
            {
                if (_code != null) return _code;
                var principal = _http.HttpContext?.User;
                var user = principal?.Identity?.IsAuthenticated == true
                    ? _userManager.GetUserAsync(principal).GetAwaiter().GetResult()
                    : null;
                _code = user != null && Supported.Contains(user.Currency) ? user.Currency : "PKR";
                return _code;
            }
        }

        public string Format(decimal amount, int decimals = 2) =>
            $"{Code} {amount.ToString("N" + decimals)}";
    }

    public static class ColorHelper
    {
        // Picks dark or white text for readable contrast on a "#rrggbb" background
        public static string TextOn(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return "#ffffff";
            var h = hex.TrimStart('#');
            if (h.Length == 3) h = string.Concat(h.Select(c => $"{c}{c}"));
            if (h.Length != 6 || !int.TryParse(h, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
                return "#ffffff";
            double Lin(int v) { var c = v / 255.0; return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4); }
            var lum = 0.2126 * Lin((rgb >> 16) & 255) + 0.7152 * Lin((rgb >> 8) & 255) + 0.0722 * Lin(rgb & 255);
            return lum > 0.4 ? "#111827" : "#ffffff";
        }
    }
}
