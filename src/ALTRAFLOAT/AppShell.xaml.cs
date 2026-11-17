using ALTRAFLOAT.Views;

namespace ALTRAFLOAT;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(AgentLoginPage), typeof(AgentLoginPage));
        Routing.RegisterRoute(nameof(LiquidityOfficerLoginPage), typeof(LiquidityOfficerLoginPage));
        Routing.RegisterRoute(nameof(AgentDashboardPage), typeof(AgentDashboardPage));
        Routing.RegisterRoute(nameof(LiquidityOfficerDashboardPage), typeof(LiquidityOfficerDashboardPage));

        var selector = new ContentPage
        {
            Title = "ALTRAFLOAT Screens",
            BackgroundColor = Color.FromArgb("#F7F3EF"),
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 18,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "ALTRAFLOAT",
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#56351F"),
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "Select a screen to preview",
                        FontSize = 16,
                        TextColor = Color.FromArgb("#76685D"),
                        HorizontalOptions = LayoutOptions.Center
                    },
                    CreateScreenButton("Agent Login", nameof(AgentLoginPage)),
                    CreateScreenButton("Liquidity Officer Login", nameof(LiquidityOfficerLoginPage)),
                    CreateScreenButton("Agent Dashboard", nameof(AgentDashboardPage)),
                    CreateScreenButton("Liquidity Officer Dashboard", nameof(LiquidityOfficerDashboardPage))
                }
            }
        };

        Items.Add(new ShellContent
        {
            Title = "Screen Preview",
            Route = "ScreenPreview",
            Content = selector
        });
    }

    private static Button CreateScreenButton(string title, string route)
    {
        return new Button
        {
            Text = title,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#F5D34F"),
            BackgroundColor = Color.FromArgb("#56351F"),
            CornerRadius = 12,
            HeightRequest = 54,
            Command = new Command(async () =>
                await Shell.Current.GoToAsync(route))
        };
    }
}
