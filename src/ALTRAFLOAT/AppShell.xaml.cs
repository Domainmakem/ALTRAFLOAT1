using ALTRAFLOAT.Views;

namespace ALTRAFLOAT;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(AgentDashboardPage), typeof(AgentDashboardPage));
        Routing.RegisterRoute(nameof(LiquidityOfficerDashboardPage), typeof(LiquidityOfficerDashboardPage));
        Routing.RegisterRoute(nameof(DashboardPreviewPage), typeof(DashboardPreviewPage));
    }
}
