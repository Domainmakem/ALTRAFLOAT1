namespace ALTRAFLOAT.Views;

public partial class AgentLoginPage : ContentPage
{
    public event EventHandler? LiquidityLoginRequested;

    public AgentLoginPage()
    {
        InitializeComponent();
    }

    private void OnAgentTabClicked(object sender, EventArgs e)
    {
        // Agent Login is already the active screen.
    }

    private void OnLiquidityTabClicked(object sender, EventArgs e)
    {
        LiquidityLoginRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AgentDashboardPage));
    }
}
