namespace ExpenseTracker.Models.ViewModels
{
    public record EmptyStateModel(string Icon, string Title, string? Text = null, string? ActionText = null, string? ActionUrl = null);
}
