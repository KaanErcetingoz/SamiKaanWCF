// ===================================================================
// FILE: Views/TradePage.xaml.cs
// PURPOSE: Handle currency buying and selling
// ===================================================================

using CurrencyExchangeMobile.Services;

namespace CurrencyExchangeMobile.Views;

public partial class TradePage : ContentPage
{
    private readonly WcfService _wcfService;
    private bool _isBuyMode = true;
    private string _selectedCurrency = "";
    private decimal _currentRate = 0;
    private readonly string[] _availableCurrencies = { "USD", "EUR", "GBP", "CHF", "JPY", "CAD", "AUD" };

    public TradePage()
    {
        InitializeComponent();
        _wcfService = new WcfService();
        SetupPage();
    }

    // ===================================================================
    // INITIALIZATION
    // ===================================================================

    private void SetupPage()
    {
        UpdateBalance();
        CreateCurrencyButtons();
        SetBuyMode();
    }

    private void UpdateBalance()
    {
        BalanceLabel.Text = $"{WcfService.CurrentBalance:F2} PLN";
    }

    private void CreateCurrencyButtons()
    {
        CurrencySelectionContainer.Children.Clear();

        foreach (var currency in _availableCurrencies)
        {
            var button = new Button
            {
                Text = currency,
                BackgroundColor = Color.FromArgb("#ecf0f1"),
                TextColor = Color.FromArgb("#7f8c8d"),
                CornerRadius = 8,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                WidthRequest = 60,
                HeightRequest = 40,
                Margin = new Thickness(0, 0, 5, 0)
            };

            button.Clicked += (sender, e) => OnCurrencySelected(currency, button);
            CurrencySelectionContainer.Children.Add(button);
        }
    }

    // ===================================================================
    // MODE SWITCHING
    // ===================================================================

    private void OnBuyTabClicked(object sender, EventArgs e)
    {
        _isBuyMode = true;
        SetBuyMode();
        ClearCalculation();
    }

    private void OnSellTabClicked(object sender, EventArgs e)
    {
        _isBuyMode = false;
        SetSellMode();
        ClearCalculation();
    }

    private void SetBuyMode()
    {
        BuyButton.BackgroundColor = Color.FromArgb("#27ae60");
        BuyButton.TextColor = Colors.White;
        SellButton.BackgroundColor = Color.FromArgb("#ecf0f1");
        SellButton.TextColor = Color.FromArgb("#7f8c8d");

        TradeButton.Text = "BUY CURRENCY";
        TradeButton.BackgroundColor = Color.FromArgb("#27ae60");

        HideStatus();
    }

    private void SetSellMode()
    {
        SellButton.BackgroundColor = Color.FromArgb("#e74c3c");
        SellButton.TextColor = Colors.White;
        BuyButton.BackgroundColor = Color.FromArgb("#ecf0f1");
        BuyButton.TextColor = Color.FromArgb("#7f8c8d");

        TradeButton.Text = "SELL CURRENCY";
        TradeButton.BackgroundColor = Color.FromArgb("#e74c3c");

        HideStatus();
    }

    // ===================================================================
    // CURRENCY SELECTION
    // ===================================================================

    private async void OnCurrencySelected(string currency, Button selectedButton)
    {
        // Update UI
        foreach (Button button in CurrencySelectionContainer.Children.OfType<Button>())
        {
            button.BackgroundColor = Color.FromArgb("#ecf0f1");
            button.TextColor = Color.FromArgb("#7f8c8d");
        }

        selectedButton.BackgroundColor = Color.FromArgb("#3498db");
        selectedButton.TextColor = Colors.White;

        _selectedCurrency = currency;

        // Load exchange rate
        await LoadExchangeRate(currency);

        // Recalculate if amount is entered
        if (!string.IsNullOrEmpty(AmountEntry.Text))
        {
            await CalculatePrice();
        }
    }

    private async Task LoadExchangeRate(string currency)
    {
        try
        {
            var rate = await _wcfService.GetRateForCurrencyAsync(currency);
            if (rate != null)
            {
                _currentRate = rate.Rate;
                ExchangeRateLabel.Text = $"{_currentRate:F4} PLN";
            }
            else
            {
                ShowError("Failed to load exchange rate");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error loading rate: {ex.Message}");
        }
    }

    // ===================================================================
    // PRICE CALCULATION
    // ===================================================================

    private async void OnAmountChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_selectedCurrency) && !string.IsNullOrEmpty(e.NewTextValue))
        {
            await CalculatePrice();
        }
        else
        {
            ClearCalculation();
        }
    }

    private async Task CalculatePrice()
    {
        try
        {
            if (string.IsNullOrEmpty(_selectedCurrency) ||
                string.IsNullOrEmpty(AmountEntry.Text) ||
                !decimal.TryParse(AmountEntry.Text, out decimal amount) ||
                amount <= 0)
            {
                ClearCalculation();
                return;
            }

            decimal? totalPrice = null;

            if (_isBuyMode)
            {
                totalPrice = await _wcfService.CalculateBuyPriceAsync(_selectedCurrency, amount);
            }
            else
            {
                totalPrice = await _wcfService.CalculateSellPriceAsync(_selectedCurrency, amount);
            }

            if (totalPrice.HasValue)
            {
                ShowCalculation(totalPrice.Value);
                TradeButton.IsEnabled = true;
            }
            else
            {
                ShowError("Failed to calculate price");
                TradeButton.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Calculation error: {ex.Message}");
            TradeButton.IsEnabled = false;
        }
    }

    private void ShowCalculation(decimal totalPrice)
    {
        ExchangeRateLabel.Text = $"{_currentRate:F4} PLN";
        TotalCostLabel.Text = $"{totalPrice:F2} PLN";
        TotalCostLabel.TextColor = _isBuyMode ? Color.FromArgb("#e74c3c") : Color.FromArgb("#27ae60");

        CalculationFrame.IsVisible = true;
        HideStatus();
    }

    private void ClearCalculation()
    {
        CalculationFrame.IsVisible = false;
        TradeButton.IsEnabled = false;
    }

    // ===================================================================
    // TRADING
    // ===================================================================

    private async void OnTradeButtonClicked(object sender, EventArgs e)
    {
        if (!ValidateTradeInput())
            return;

        await ExecuteTrade();
    }

    private bool ValidateTradeInput()
    {
        if (string.IsNullOrEmpty(_selectedCurrency))
        {
            ShowError("Please select a currency");
            return false;
        }

        if (string.IsNullOrEmpty(AmountEntry.Text) ||
            !decimal.TryParse(AmountEntry.Text, out decimal amount) ||
            amount <= 0)
        {
            ShowError("Please enter a valid amount");
            return false;
        }

        return true;
    }

    private async Task ExecuteTrade()
    {
        ShowLoading(true);
        HideStatus();

        try
        {
            var amount = decimal.Parse(AmountEntry.Text);
            TransactionResult result;

            if (_isBuyMode)
            {
                result = await _wcfService.BuyCurrencyAsync(_selectedCurrency, amount);
            }
            else
            {
                result = await _wcfService.SellCurrencyAsync(_selectedCurrency, amount);
            }

            if (result.IsSuccess)
            {
                var action = _isBuyMode ? "purchased" : "sold";
                ShowSuccess($"Successfully {action} {amount:F2} {_selectedCurrency}!");

                // Update balance
                UpdateBalance();

                // Clear form
                ClearForm();
            }
            else
            {
                ShowError(result.Message ?? "Transaction failed");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Transaction failed: {ex.Message}");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void ClearForm()
    {
        AmountEntry.Text = string.Empty;
        ClearCalculation();

        // Clear currency selection
        foreach (Button button in CurrencySelectionContainer.Children.OfType<Button>())
        {
            button.BackgroundColor = Color.FromArgb("#ecf0f1");
            button.TextColor = Color.FromArgb("#7f8c8d");
        }

        _selectedCurrency = "";
    }

    // ===================================================================
    // UI HELPERS
    // ===================================================================

    private void ShowLoading(bool show)
    {
        LoadingIndicator.IsVisible = show;
        LoadingIndicator.IsRunning = show;
        TradeButton.IsEnabled = !show;
        AmountEntry.IsEnabled = !show;
        BuyButton.IsEnabled = !show;
        SellButton.IsEnabled = !show;

        // Disable currency buttons
        foreach (Button button in CurrencySelectionContainer.Children.OfType<Button>())
        {
            button.IsEnabled = !show;
        }
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
    // PAGE LIFECYCLE
    // ===================================================================

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateBalance();
    }
}