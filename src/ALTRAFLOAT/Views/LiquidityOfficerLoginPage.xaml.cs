namespace ALTRAFLOAT.Views;

public partial class LiquidityOfficerLoginPage : ContentPage
{
    public event EventHandler? AgentLoginRequested;

    public LiquidityOfficerLoginPage()
    {
        InitializeComponent();
    }

    private void OnAgentTabClicked(object sender, EventArgs e)
    {
        AgentLoginRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnLiquidityTabClicked(object sender, EventArgs e)
    {
        // Liquidity Officer Login is already the active screen.
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LiquidityOfficerDashboardPage));
    }
}
