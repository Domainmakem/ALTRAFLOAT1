namespace ALTRAFLOAT.Views;

public partial class AgentLoginPage : ContentPage
{
    public AgentLoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AgentDashboardPage));
    }
}
