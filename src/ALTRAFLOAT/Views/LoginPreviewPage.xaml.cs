namespace ALTRAFLOAT.Views;

public partial class LoginPreviewPage : ContentPage
{
    private readonly View _agentLogin;
    private readonly View _liquidityLogin;

    public LoginPreviewPage()
    {
        InitializeComponent();

        var agentPage = new AgentLoginPage();
        agentPage.LiquidityLoginRequested += OnLiquidityLoginRequested;
        _agentLogin = agentPage.Content
            ?? throw new InvalidOperationException("Agent login content is missing.");
        agentPage.Content = null;

        var liquidityPage = new LiquidityOfficerLoginPage();
        liquidityPage.AgentLoginRequested += OnAgentLoginRequested;
        _liquidityLogin = liquidityPage.Content
            ?? throw new InvalidOperationException("Liquidity Officer login content is missing.");
        liquidityPage.Content = null;

        ShowAgentLogin();
    }

    private void OnLiquidityLoginRequested(object sender, EventArgs e)
    {
        LoginHost.Content = _liquidityLogin;
    }

    private void OnAgentLoginRequested(object sender, EventArgs e)
    {
        ShowAgentLogin();
    }

    private void ShowAgentLogin()
    {
        LoginHost.Content = _agentLogin;
    }
}
