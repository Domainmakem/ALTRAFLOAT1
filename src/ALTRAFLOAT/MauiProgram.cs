using ALTRAFLOAT.ViewModels;
using ALTRAFLOAT.Views;
using Microsoft.Extensions.Logging;

namespace ALTRAFLOAT;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<HomePage>();

        builder.Services.AddTransient<AgentLoginPage>();
        builder.Services.AddTransient<LiquidityOfficerLoginPage>();
        builder.Services.AddTransient<AgentDashboardPage>();
        builder.Services.AddTransient<LiquidityOfficerDashboardPage>();

        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
