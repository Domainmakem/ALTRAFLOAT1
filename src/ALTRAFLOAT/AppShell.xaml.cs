using ALTRAFLOAT.Views;

namespace ALTRAFLOAT;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(AgentLoginPage), typeof(AgentLoginPage));
        Routing.RegisterRoute(nameof(LiquidityOfficerLoginPage), typeof(LiquidityOfficerLoginPage));
    }
}
