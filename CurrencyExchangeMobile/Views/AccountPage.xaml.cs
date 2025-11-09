// ===================================================================
// FILE: Views/AccountPage.xaml.cs
// PURPOSE: Handle account management, top-up, and logout
// ===================================================================

using CurrencyExchangeMobile.Services;
using CurrencyExchangeMobile.Views;

namespace CurrencyExchangeMobile.Views;

public partial class AccountPage : ContentPage
{
    private readonly WcfService _wcfService;

    public AccountPage()
    {
        InitializeComponent();
        _wcfService = new WcfService();
        LoadAccountInfo();
    }

    // ===================================================================
    // DATA LOADING
    // ===================================================================

    private void LoadAccountInfo()
    {
        UpdateUserInfo();
        UpdateBalance();
    }

    private void UpdateUserInfo()
    {
        UsernameLabel.Text = WcfService.CurrentUsername ?? "User";
    }

    private async void UpdateBalance()
    {
        try
        {
            var balance = await _wcfService.GetCurrentBalanceAsync();
            BalanceLabel.Text = $"{balance:F2} PLN";
        }
        catch (Exception ex)
        {
            BalanceLabel.Text = $"{WcfService.CurrentBalance:F2} PLN";
            Console.WriteLine($"Balance update error: {ex.Message}");
        }
    }

    // ===================================================================
    // TOP UP FUNCTIONALITY
    // ===================================================================

    private void OnTopUpToggleClicked(object sender, EventArgs e)
    {
        ToggleTopUpForm();
    }

    private void OnTopUpCancelClicked(object sender, EventArgs e)
    {
        HideTopUpForm();
        ClearTopUpForm();
    }

    private async void OnTopUpConfirmClicked(object sender, EventArgs e)
    {
        if (!ValidateTopUpAmount())
            return;

        await ProcessTopUp();
    }

    private void ToggleTopUpForm()
    {
        if (TopUpForm.IsVisible)
        {
            HideTopUpForm();
        }
        else
        {
            ShowTopUpForm();
        }
    }

    private void ShowTopUpForm()
    {
        TopUpForm.IsVisible = true;
        TopUpToggleButton.Text = "✕ Cancel Top Up";
        TopUpToggleButton.BackgroundColor = Color.FromArgb("#95a5a6");
        TopUpAmountEntry.Focus();
        HideStatus();
    }

    private void HideTopUpForm()
    {
        TopUpForm.IsVisible = false;
        TopUpToggleButton.Text = "💰 Top Up Account";
        TopUpToggleButton.BackgroundColor = Color.FromArgb("#27ae60");
    }

    private void ClearTopUpForm()
    {
        TopUpAmountEntry.Text = string.Empty;
        HideStatus();
    }

    private bool ValidateTopUpAmount()
    {
        if (string.IsNullOrWhiteSpace(TopUpAmountEntry.Text))
        {
            ShowError("Please enter an amount to top up");
            return false;
        }

        if (!decimal.TryParse(TopUpAmountEntry.Text, out decimal amount))
        {
            ShowError("Please enter a valid amount");
            return false;
        }

        if (amount <= 0)
        {
            ShowError("Amount must be greater than 0");
            return false;
        }

        if (amount > 10000)
        {
            ShowError("Maximum top-up amount is 10,000 PLN");
            return false;
        }

        return true;
    }

    private async Task ProcessTopUp()
    {
        ShowTopUpLoading(true);
        HideStatus();

        try
        {
            var amount = decimal.Parse(TopUpAmountEntry.Text);
            var success = await _wcfService.TopUpAccountAsync(amount);

            if (success)
            {
                ShowSuccess($"Successfully added {amount:F2} PLN to your account!");
                UpdateBalance();

                // Hide form after successful top-up
                await Task.Delay(2000);
                HideTopUpForm();
                ClearTopUpForm();
            }
            else
            {
                ShowError("Top-up failed. Please try again.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Top-up failed: {ex.Message}");
        }
        finally
        {
            ShowTopUpLoading(false);
        }
    }

    private void ShowTopUpLoading(bool show)
    {
        TopUpLoadingIndicator.IsVisible = show;
        TopUpLoadingIndicator.IsRunning = show;
        ConfirmTopUpButton.IsEnabled = !show;
        TopUpAmountEntry.IsEnabled = !show;
    }

    // ===================================================================
    // MENU ITEM HANDLERS
    // ===================================================================

    private async void OnTransactionHistoryTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert(
            "Transaction History",
            "Transaction history feature will be available in a future update.",
            "OK"
        );
    }

    private async void OnSettingsTapped(object sender, TappedEventArgs e)
    {
        var result = await DisplayActionSheet(
            "Settings",
            "Cancel",
            null,
            "Change Password",
            "Notification Settings",
            "Privacy Policy",
            "Terms of Service"
        );

        if (!string.IsNullOrEmpty(result) && result != "Cancel")
        {
            await DisplayAlert("Settings", $"{result} feature will be available in a future update.", "OK");
        }
    }

    private async void OnHelpTapped(object sender, TappedEventArgs e)
    {
        var result = await DisplayActionSheet(
            "Help & Support",
            "Cancel",
            null,
            "FAQ",
            "Contact Support",
            "Report a Problem",
            "App Tutorial"
        );

        switch (result)
        {
            case "FAQ":
                await DisplayAlert("FAQ",
                    "Q: How do I buy currency?\nA: Go to Trade tab, select currency, enter amount, and tap Buy.\n\n" +
                    "Q: How do I top up my account?\nA: Go to Account tab and tap 'Top Up Account'.\n\n" +
                    "Q: Where do exchange rates come from?\nA: All rates are real-time from NBP (National Bank of Poland).",
                    "OK");
                break;
            case "Contact Support":
                await DisplayAlert("Contact Support", "Email: support@currencyexchange.com\nPhone: +48 123 456 789", "OK");
                break;
            case "Report a Problem":
                await DisplayAlert("Report Problem", "Please email us at: bugs@currencyexchange.com with details of the issue.", "OK");
                break;
            case "App Tutorial":
                await DisplayAlert("Tutorial",
                    "1. Login or create an account\n" +
                    "2. Top up your PLN balance\n" +
                    "3. Check current exchange rates\n" +
                    "4. Buy or sell currencies\n" +
                    "5. Monitor your balance",
                    "OK");
                break;
        }
    }

    private async void OnLogoutTapped(object sender, TappedEventArgs e)
    {
        var result = await DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes, Logout",
            "Cancel"
        );

        if (result)
        {
            await PerformLogout();
        }
    }

    private async Task PerformLogout()
    {
        try
        {
            // Clear user data
            WcfService.CurrentUserId = null;
            WcfService.CurrentUsername = null;
            WcfService.CurrentBalance = 0;

            // Show logout message
            ShowSuccess("Logged out successfully!");

            // Wait briefly to show the message
            await Task.Delay(1000);

            // Navigate back to login
            Application.Current.MainPage = new LoginPage();
        }
        catch (Exception ex)
        {
            ShowError($"Logout error: {ex.Message}");
        }
    }

    // ===================================================================
    // UI HELPERS
    // ===================================================================

    private void ShowError(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.TextColor = Color.FromArgb("#e74c3c");
        StatusLabel.IsVisible = true;
    }

    private void ShowSuccess(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.TextColor = Color.FromArgb("#27ae60");
        StatusLabel.IsVisible = true;
    }

    private void HideStatus()
    {
        StatusLabel.IsVisible = false;
        StatusLabel.Text = string.Empty;
    }

    // ===================================================================
    // PAGE LIFECYCLE
    // ===================================================================

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAccountInfo();
    }
}