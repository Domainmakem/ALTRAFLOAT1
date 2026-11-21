using ALTRAFLOAT.Services;

namespace ALTRAFLOAT.Views;

public partial class LoginPreviewPage : ContentPage
{
    private readonly View _agentLogin;
    private readonly View _liquidityLogin;
    private readonly View _agentCreateAccount;
    private readonly View _liquidityCreateAccount;

    public LoginPreviewPage(FirebaseAuthenticationService authService)
    {
        InitializeComponent();

        var agentPage = new AgentLoginPage(authService);
        agentPage.LiquidityLoginRequested += OnLiquidityLoginRequested;
        agentPage.CreateAccountRequested += OnAgentCreateAccountRequested;
        _agentLogin = agentPage.Content
            ?? throw new InvalidOperationException("Agent login content is missing.");
        agentPage.Content = null;

        var liquidityPage = new LiquidityOfficerLoginPage(authService);
        liquidityPage.AgentLoginRequested += OnAgentLoginRequested;
        liquidityPage.CreateAccountRequested += OnLiquidityCreateAccountRequested;
        _liquidityLogin = liquidityPage.Content
            ?? throw new InvalidOperationException("Liquidity Officer login content is missing.");
        liquidityPage.Content = null;

        var agentRegistration = new AgentCreateAccountPage(authService);
        agentRegistration.BackToLoginRequested += OnAgentRegistrationBack;
        _agentCreateAccount = agentRegistration.Content
            ?? throw new InvalidOperationException("Agent registration content is missing.");
        agentRegistration.Content = null;

        var liquidityRegistration = new LiquidityOfficerCreateAccountPage(authService);
        liquidityRegistration.BackToLoginRequested += OnLiquidityRegistrationBack;
        _liquidityCreateAccount = liquidityRegistration.Content
            ?? throw new InvalidOperationException("Liquidity Officer registration content is missing.");
        liquidityRegistration.Content = null;

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

    private void OnAgentCreateAccountRequested(object sender, EventArgs e)
    {
        LoginHost.Content = _agentCreateAccount;
    }

    private void OnLiquidityCreateAccountRequested(object sender, EventArgs e)
    {
        LoginHost.Content = _liquidityCreateAccount;
    }

    private void OnAgentRegistrationBack(object sender, EventArgs e)
    {
        ShowAgentLogin();
    }

    private void OnLiquidityRegistrationBack(object sender, EventArgs e)
    {
        LoginHost.Content = _liquidityLogin;
    }

    private void ShowAgentLogin()
    {
        LoginHost.Content = _agentLogin;
    }
}
