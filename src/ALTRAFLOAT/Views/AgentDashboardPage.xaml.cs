using ALTRAFLOAT.Services;

namespace ALTRAFLOAT.Views;

public partial class AgentDashboardPage : ContentPage
{
    private readonly FirebaseAuthenticationService _authService;
    private bool _isVerifying;
    private bool _redirecting;

    public AgentDashboardPage(FirebaseAuthenticationService authService)
    {
        _authService = authService;
        InitializeComponent();
        IsVisible = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isVerifying || _redirecting)
            return;

        _isVerifying = true;
        IsVisible = false;

        try
        {
            await _authService.VerifyCurrentAgentSessionAsync();
            IsVisible = true;
        }
        catch
        {
            _redirecting = true;

            await DisplayAlert(
                "Access denied",
                "Your account could not be verified or is not active. Please sign in again.",
                "OK");

            await Shell.Current.GoToAsync("//LoginPreview");
        }
        finally
        {
            _isVerifying = false;
        }
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        try
        {
            await _authService.SignOutAsync();
            await Shell.Current.GoToAsync("//LoginPreview");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Sign-out failed", ex.Message, "OK");
        }
    }
}
