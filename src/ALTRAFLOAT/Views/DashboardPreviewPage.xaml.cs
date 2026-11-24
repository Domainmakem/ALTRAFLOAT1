using ALTRAFLOAT.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ALTRAFLOAT.Views;

public partial class DashboardPreviewPage : ContentPage
{
    private readonly View _agentDashboard;
    private readonly View _liquidityDashboard;

    public DashboardPreviewPage()
    {
        InitializeComponent();

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Application services are unavailable.");
        var authService = services.GetRequiredService<FirebaseAuthenticationService>();

        var agentPage = new AgentDashboardPage(authService);
        _agentDashboard = agentPage.Content
            ?? throw new InvalidOperationException("Agent dashboard content is missing.");
        agentPage.Content = null;

        var liquidityPage = new LiquidityOfficerDashboardPage(authService);
        _liquidityDashboard = liquidityPage.Content
            ?? throw new InvalidOperationException("Liquidity Officer dashboard content is missing.");
        liquidityPage.Content = null;

        ShowAgentDashboard();
    }

    private void OnAgentTabClicked(object sender, EventArgs e)
    {
        ShowAgentDashboard();
    }

    private void OnLiquidityTabClicked(object sender, EventArgs e)
    {
        DashboardHost.Content = _liquidityDashboard;

        AgentTab.BackgroundColor = Color.FromArgb("#514034");
        AgentTab.TextColor = Colors.White;

        LiquidityTab.BackgroundColor = Color.FromArgb("#FFC400");
        LiquidityTab.TextColor = Color.FromArgb("#443123");
    }

    private void ShowAgentDashboard()
    {
        DashboardHost.Content = _agentDashboard;

        AgentTab.BackgroundColor = Color.FromArgb("#FFC400");
        AgentTab.TextColor = Color.FromArgb("#443123");

        LiquidityTab.BackgroundColor = Color.FromArgb("#514034");
        LiquidityTab.TextColor = Colors.White;
    }
}
