// ===================================================================
// FILE: Views/LoginPage.xaml.cs
// PURPOSE: Handle login/register logic and navigation
// ===================================================================

using CurrencyExchangeMobile.Services;

namespace CurrencyExchangeMobile.Views;

public partial class LoginPage : ContentPage
{
    private bool _isLoginMode = true;
    private readonly WcfService _wcfService;

    public LoginPage()
    {
        InitializeComponent();
        _wcfService = new WcfService();
        SetLoginMode();
    }

    // ===================================================================
    // TAB SWITCHING
    // ===================================================================

    private void OnLoginTabClicked(object sender, EventArgs e)
    {
        _isLoginMode = true;
        SetLoginMode();
        ClearForm();
    }

    private void OnRegisterTabClicked(object sender, EventArgs e)
    {
        _isLoginMode = false;
        SetRegisterMode();
        ClearForm();
    }

    private void SetLoginMode()
    {
        // Update tab appearance
        LoginTabButton.BackgroundColor = Color.FromArgb("#3498db");
        LoginTabButton.TextColor = Colors.White;
        RegisterTabButton.BackgroundColor = Color.FromArgb("#ecf0f1");
        RegisterTabButton.TextColor = Color.FromArgb("#7f8c8d");

        // Hide email field and update button
        EmailFrame.IsVisible = false;
        ActionButton.Text = "Login";
        ActionButton.BackgroundColor = Color.FromArgb("#3498db");

        // Clear status
        HideStatus();
    }

    private void SetRegisterMode()
    {
        // Update tab appearance
        RegisterTabButton.BackgroundColor = Color.FromArgb("#3498db");
        RegisterTabButton.TextColor = Colors.White;
        LoginTabButton.BackgroundColor = Color.FromArgb("#ecf0f1");
        LoginTabButton.TextColor = Color.FromArgb("#7f8c8d");

        // Show email field and update button
        EmailFrame.IsVisible = true;
        ActionButton.Text = "Create Account";
        ActionButton.BackgroundColor = Color.FromArgb("#27ae60");

        // Clear status
        HideStatus();
    }

    // ===================================================================
    // FORM HANDLING
    // ===================================================================

    private void ClearForm()
    {
        UsernameEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        HideStatus();
    }

    private bool ValidateForm()
    {
        // Check required fields
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
        {
            ShowError("Please enter username");
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError("Please enter password");
            return false;
        }

        // Additional validation for register mode
        if (!_isLoginMode)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                ShowError("Please enter email address");
                return false;
            }

            if (!IsValidEmail(EmailEntry.Text))
            {
                ShowError("Please enter a valid email address");
                return false;
            }

            if (PasswordEntry.Text.Length < 4)
            {
                ShowError("Password must be at least 4 characters");
                return false;
            }
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // ===================================================================
    // AUTHENTICATION
    // ===================================================================

    private async void OnActionButtonClicked(object sender, EventArgs e)
    {
        if (!ValidateForm())
            return;

        await ProcessAuthentication();
    }

    private async Task ProcessAuthentication()
    {
        ShowLoading(true);
        HideStatus();

        try
        {
            bool success = false;
            string username = UsernameEntry.Text.Trim();
            string password = PasswordEntry.Text;
            string email = EmailEntry.Text?.Trim();

            if (_isLoginMode)
            {
                success = await _wcfService.LoginAsync(username, password);

                if (success)
                {
                    ShowSuccess("Login successful! Welcome back.");
                    await NavigateToMainApp();
                }
                else
                {
                    ShowError("Invalid username or password. Please try again.");
                }
            }
            else
            {
                success = await _wcfService.RegisterAsync(username, email, password);

                if (success)
                {
                    ShowSuccess("Account created successfully! Please login.");
                    await Task.Delay(1500); // Show success message
                    SwitchToLoginMode();
                }
                else
                {
                    ShowError("Registration failed. Username or email might already exist.");
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Connection failed: {ex.Message}");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void SwitchToLoginMode()
    {
        _isLoginMode = true;
        SetLoginMode();
        PasswordEntry.Text = string.Empty; // Keep username and email
    }

    private async Task NavigateToMainApp()
    {
        await Task.Delay(1000); // Show success message briefly

        // Navigate to main app shell
        Application.Current.MainPage = new AppShell();
    }

    // ===================================================================
    // UI HELPERS
    // ===================================================================

    private void ShowLoading(bool show)
    {
        LoadingIndicator.IsVisible = show;
        LoadingIndicator.IsRunning = show;
        ActionButton.IsEnabled = !show;

        // Disable form during loading
        UsernameEntry.IsEnabled = !show;
        EmailEntry.IsEnabled = !show;
        PasswordEntry.IsEnabled = !show;
        LoginTabButton.IsEnabled = !show;
        RegisterTabButton.IsEnabled = !show;
    }

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
    // CLEANUP
    // ===================================================================

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _wcfService?.Dispose();
    }
}