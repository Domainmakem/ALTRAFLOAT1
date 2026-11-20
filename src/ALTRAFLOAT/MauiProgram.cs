using ALTRAFLOAT.ViewModels;
using ALTRAFLOAT.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.Bundled.Platforms.Android;

namespace ALTRAFLOAT;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        var firebaseSettings = new CrossFirebaseSettings(
            isAnalyticsEnabled: false,
            isAuthEnabled: true,
            isCloudMessagingEnabled: false,
            isDynamicLinksEnabled: false,
            isFirestoreEnabled: true,
            isFunctionsEnabled: false,
            isRemoteConfigEnabled: false,
            isStorageEnabled: false
        );

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(
                    activity,
                    () => Platform.CurrentActivity,
                    firebaseSettings
                )
            ));
#endif
        });

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
