// ===================================================================
// FILE: Views/RatesPage.xaml.cs
// PURPOSE: Display all current exchange rates from NBP API
// ===================================================================

using CurrencyExchangeMobile.Services;

namespace CurrencyExchangeMobile.Views;

public partial class RatesPage : ContentPage
{
    private readonly WcfService _wcfService;
    private List<ExchangeRate> _allRates;

    public RatesPage()
    {
        InitializeComponent();
        _wcfService = new WcfService();
        LoadExchangeRates();
    }

    // ===================================================================
    // DATA LOADING
    // ===================================================================

    private async void LoadExchangeRates()
    {
        await RefreshRates();
    }

    private async Task RefreshRates()
    {
        try
        {
            ShowLoading(true);
            ShowError(false);

            _allRates = await _wcfService.GetCurrentRatesAsync();

            if (_allRates != null && _allRates.Count > 0)
            {
                DisplayAllRates();
                UpdateLastUpdatedLabel();
                ShowLoading(false);
                ShowRatesContainer(true);
            }
            else
            {
                ShowLoading(false);
                ShowError(true);
            }
        }
        catch (Exception ex)
        {
            ShowLoading(false);
            ShowError(true);
            Console.WriteLine($"Error loading rates: {ex.Message}");
        }
    }

    private void DisplayAllRates()
    {
        RatesContainer.Children.Clear();

        foreach (var rate in _allRates)
        {
            var rateCard = CreateRateCard(rate);
            RatesContainer.Children.Add(rateCard);
        }
    }

    private Frame CreateRateCard(ExchangeRate rate)
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 12,
            Padding = new Thickness(20),
            HasShadow = true,
            Margin = new Thickness(0, 0, 0, 15)
        };

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
            }
        };

        // Currency info section
        var currencyStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            VerticalOptions = LayoutOptions.Center
        };

        var flagLabel = new Label
        {
            Text = GetCurrencyFlag(rate.Currency),
            FontSize = 28,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 15, 0)
        };

        var currencyInfoStack = new StackLayout
        {
            VerticalOptions = LayoutOptions.Center
        };

        var currencyCodeLabel = new Label
        {
            Text = rate.Currency,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2c3e50")
        };

        var tableTypeLabel = new Label
        {
            Text = $"Table {rate.TableType}",
            FontSize = 12,
            TextColor = Color.FromArgb("#7f8c8d")
        };

        currencyInfoStack.Children.Add(currencyCodeLabel);
        currencyInfoStack.Children.Add(tableTypeLabel);
        currencyStack.Children.Add(flagLabel);
        currencyStack.Children.Add(currencyInfoStack);

        // Rate info section
        var rateStack = new StackLayout
        {
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };

        var rateLabel = new Label
        {
            Text = $"{rate.Rate:F4}",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#27ae60"),
            HorizontalTextAlignment = TextAlignment.End
        };

        var plnLabel = new Label
        {
            Text = "PLN",
            FontSize = 14,
            TextColor = Color.FromArgb("#7f8c8d"),
            HorizontalTextAlignment = TextAlignment.End
        };

        rateStack.Children.Add(rateLabel);
        rateStack.Children.Add(plnLabel);

        // Effective date (spans both columns)
        var dateLabel = new Label
        {
            Text = $"Updated: {rate.EffectiveDate}",
            FontSize = 12,
            TextColor = Color.FromArgb("#95a5a6"),
            Margin = new Thickness(0, 10, 0, 0)
        };

        // Add tap gesture for trading
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (sender, e) => OnRateCardTapped(rate);
        frame.GestureRecognizers.Add(tapGesture);

        // Add visual feedback for tapping
        frame.Opacity = 1.0;

        mainGrid.Add(currencyStack, 0, 0);
        mainGrid.Add(rateStack, 1, 0);
        mainGrid.Add(dateLabel, 0, 1);
        Grid.SetColumnSpan(dateLabel, 2);

        frame.Content = mainGrid;
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
            "CZK" => "🇨🇿",
            "HUF" => "🇭🇺",
            "RON" => "🇷🇴",
            "BGN" => "🇧🇬",
            "HRK" => "🇭🇷",
            "RUB" => "🇷🇺",
            "TRY" => "🇹🇷",
            "ILS" => "🇮🇱",
            "CLP" => "🇨🇱",
            "PHP" => "🇵🇭",
            "MXN" => "🇲🇽",
            "ZAR" => "🇿🇦",
            "BRL" => "🇧🇷",
            "MYR" => "🇲🇾",
            "IDR" => "🇮🇩",
            "INR" => "🇮🇳",
            "KRW" => "🇰🇷",
            "CNY" => "🇨🇳",
            "SGD" => "🇸🇬",
            "THB" => "🇹🇭",
            "NZD" => "🇳🇿",
            _ => "💱"
        };
    }

    private void UpdateLastUpdatedLabel()
    {
        LastUpdatedLabel.Text = $"Last updated: {DateTime.Now:HH:mm:ss}";
    }

    // ===================================================================
    // UI STATE MANAGEMENT
    // ===================================================================

    private void ShowLoading(bool show)
    {
        LoadingContainer.IsVisible = show;
        RefreshButton.IsEnabled = !show;

        if (show)
        {
            RefreshButton.Text = "⟳";
        }
        else
        {
            RefreshButton.Text = "↻";
        }
    }

    private void ShowError(bool show)
    {
        ErrorContainer.IsVisible = show;
    }

    private void ShowRatesContainer(bool show)
    {
        RatesContainer.IsVisible = show;
    }

    // ===================================================================
    // EVENT HANDLERS
    // ===================================================================

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await RefreshRates();
    }

    private async void OnRetryClicked(object sender, EventArgs e)
    {
        await RefreshRates();
    }

    private async void OnRateCardTapped(ExchangeRate rate)
    {
        var result = await DisplayAlert(
            $"Trade {rate.Currency}",
            $"Current rate: {rate.Rate:F4} PLN\n\nWould you like to trade this currency?",
            "Go to Trade",
            "Cancel"
        );

        if (result)
        {
            await Shell.Current.GoToAsync("//trade");
        }
    }

    // ===================================================================
    // PAGE LIFECYCLE
    // ===================================================================

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Refresh rates if they're older than 5 minutes
        if (_allRates == null)
        {
            _ = Task.Run(RefreshRates);
        }
    }
}