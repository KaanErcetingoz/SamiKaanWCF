
using CurrencyExchangeMobile.Services;

namespace CurrencyExchangeMobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly WcfService _wcfService;
    private List<ExchangeRate> _currentRates;

    public DashboardPage()
    {
        InitializeComponent();
        _wcfService = new WcfService();
        LoadDashboardData();
    }

    // ===================================================================
    // DATA LOADING
    // ===================================================================

    private async void LoadDashboardData()
    {
        await LoadUserInfo();
        await LoadExchangeRates();
    }

    private async Task LoadUserInfo()
    {
        try
        {
            // Update username
            UsernameLabel.Text = WcfService.CurrentUsername ?? "User";

            // Update balance
            await RefreshBalance();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load user info: {ex.Message}", "OK");
        }
    }

    private async Task RefreshBalance()
    {
        try
        {
            var balance = await _wcfService.GetCurrentBalanceAsync();
            BalanceLabel.Text = $"{balance:F2} PLN";
        }
        catch (Exception ex)
        {
            BalanceLabel.Text = $"{WcfService.CurrentBalance:F2} PLN";
            Console.WriteLine($"Balance refresh error: {ex.Message}");
        }
    }

    private async Task LoadExchangeRates()
    {
        try
        {
            RatesLoadingContainer.IsVisible = true;
            RatesListContainer.IsVisible = false;
            ViewAllRatesButton.IsVisible = false;

            _currentRates = await _wcfService.GetCurrentRatesAsync();

            if (_currentRates != null && _currentRates.Count > 0)
            {
                DisplayTopRates();
                RatesLoadingContainer.IsVisible = false;
                RatesListContainer.IsVisible = true;
                ViewAllRatesButton.IsVisible = true;
            }
            else
            {
                ShowRatesError();
            }
        }
        catch (Exception ex)
        {
            ShowRatesError();
            Console.WriteLine($"Rates loading error: {ex.Message}");
        }
    }

    private void DisplayTopRates()
    {
        RatesListContainer.Children.Clear();

        // Show top 5 currencies
        var topRates = _currentRates.Take(5).ToList();

        foreach (var rate in topRates)
        {
            var rateFrame = CreateRateItem(rate);
            RatesListContainer.Children.Add(rateFrame);
        }
    }

    private Frame CreateRateItem(ExchangeRate rate)
    {
        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#f8f9fa"),
            CornerRadius = 8,
            Padding = new Thickness(15),
            HasShadow = false,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
            }
        };

        // Currency info
        var currencyStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            VerticalOptions = LayoutOptions.Center
        };

        var flagLabel = new Label
        {
            Text = GetCurrencyFlag(rate.Currency),
            FontSize = 20,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var currencyLabel = new Label
        {
            Text = rate.Currency,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2c3e50"),
            VerticalOptions = LayoutOptions.Center
        };

        currencyStack.Children.Add(flagLabel);
        currencyStack.Children.Add(currencyLabel);

        // Rate info
        var rateStack = new StackLayout
        {
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };

        var rateLabel = new Label
        {
            Text = $"{rate.Rate:F4}",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#27ae60"),
            HorizontalTextAlignment = TextAlignment.End
        };

        var plnLabel = new Label
        {
            Text = "PLN",
            FontSize = 12,
            TextColor = Color.FromArgb("#7f8c8d"),
            HorizontalTextAlignment = TextAlignment.End
        };

        rateStack.Children.Add(rateLabel);
        rateStack.Children.Add(plnLabel);

        grid.Add(currencyStack, 0, 0);
        grid.Add(rateStack, 1, 0);
        frame.Content = grid;

        return frame;
    }

    private string GetCurrencyFlag(string currency)
    {
        return currency switch
        {
            "USD" => "🇺🇸",
            "EUR" => "🇪🇺",
            "GBP" => "🇬🇧",
            "CHF" => "🇨🇭",
            "JPY" => "🇯🇵",
            "CAD" => "🇨🇦",
            "AUD" => "🇦🇺",
            "SEK" => "🇸🇪",
            "NOK" => "🇳🇴",
            "DKK" => "🇩🇰",
            _ => "💱"
        };
    }

    private void ShowRatesError()
    {
        RatesLoadingContainer.IsVisible = false;
        RatesListContainer.Children.Clear();

        var errorLabel = new Label
        {
            Text = "Unable to load exchange rates",
            FontSize = 14,
            TextColor = Color.FromArgb("#e74c3c"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 20)
        };

        RatesListContainer.Children.Add(errorLabel);
        RatesListContainer.IsVisible = true;
    }

    // ===================================================================
    // EVENT HANDLERS
    // ===================================================================

    private async void OnRefreshBalanceClicked(object sender, EventArgs e)
    {
        RefreshBalanceButton.IsEnabled = false;
        RefreshBalanceButton.Text = "Refreshing...";

        await RefreshBalance();

        RefreshBalanceButton.Text = "↻ Refresh";
        RefreshBalanceButton.IsEnabled = true;
    }

    private async void OnTopUpTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//account");
    }

    private async void OnTradeTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//trade");
    }

    private async void OnRatesTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//rates");
    }

    private async void OnViewAllRatesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//rates");
    }

    // ===================================================================
    // PAGE LIFECYCLE
    // ===================================================================

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Refresh data when page appears
        _ = Task.Run(async () =>
        {
            await RefreshBalance();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}