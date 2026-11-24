using ALTRAFLOAT.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ALTRAFLOAT;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        LoginShellContent.ContentTemplate = new DataTemplate(
            () => services.GetRequiredService<LoginPreviewPage>());

        Routing.RegisterRoute(nameof(AgentDashboardPage), typeof(AgentDashboardPage));
        Routing.RegisterRoute(nameof(LiquidityOfficerDashboardPage), typeof(LiquidityOfficerDashboardPage));
        Routing.RegisterRoute(nameof(DashboardPreviewPage), typeof(DashboardPreviewPage));
    }
}
