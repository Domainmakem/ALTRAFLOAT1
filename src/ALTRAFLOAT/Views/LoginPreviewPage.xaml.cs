namespace ALTRAFLOAT.Views;

public partial class LoginPreviewPage : ContentPage
{
    private readonly View _agentLogin;
    private readonly View _liquidityLogin;

    public LoginPreviewPage()
    {
        InitializeComponent();

        var agentPage = new AgentLoginPage();
        _agentLogin = agentPage.Content
            ?? throw new InvalidOperationException("Agent login content is missing.");
        agentPage.Content = null;

        var liquidityPage = new LiquidityOfficerLoginPage();
        _liquidityLogin = liquidityPage.Content
            ?? throw new InvalidOperationException("Liquidity Officer login content is missing.");
        liquidityPage.Content = null;

        ShowAgentLogin();
    }

    private void OnAgentTabClicked(object sender, EventArgs e)
    {
        ShowAgentLogin();
    }

    private void OnLiquidityTabClicked(object sender, EventArgs e)
    {
        LoginHost.Content = _liquidityLogin;

        AgentTab.BackgroundColor = Color.FromArgb("#514034");
        AgentTab.TextColor = Colors.White;

        LiquidityTab.BackgroundColor = Color.FromArgb("#FFC400");
        LiquidityTab.TextColor = Color.FromArgb("#443123");
    }

    private void ShowAgentLogin()
    {
        LoginHost.Content = _agentLogin;

        AgentTab.BackgroundColor = Color.FromArgb("#FFC400");
        AgentTab.TextColor = Color.FromArgb("#443123");

        LiquidityTab.BackgroundColor = Color.FromArgb("#514034");
        LiquidityTab.TextColor = Colors.White;
    }
}
