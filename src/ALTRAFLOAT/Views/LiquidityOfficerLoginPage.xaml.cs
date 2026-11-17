namespace ALTRAFLOAT.Views;

public partial class LiquidityOfficerLoginPage : ContentPage
{
    public LiquidityOfficerLoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LiquidityOfficerDashboardPage));
    }
}
