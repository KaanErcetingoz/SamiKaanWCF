

using CurrencyExchangeMobile.Views;

namespace CurrencyExchangeMobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Start with LoginPage instead of MainPage
        MainPage = new LoginPage();
    }
}