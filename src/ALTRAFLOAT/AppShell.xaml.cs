using ALTRAFLOAT.Views;

namespace ALTRAFLOAT;

public partial class AppShell : Shell
{
    public AppShell(HomePage homePage)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Title = "Home",
            Route = "Home",
            Content = homePage
        });
    }
}
