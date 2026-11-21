using ALTRAFLOAT.Services;

namespace ALTRAFLOAT.Views;

public partial class LiquidityOfficerDashboardPage : ContentPage
{
    private readonly FirebaseAuthenticationService _authService;
    private bool _isVerifying;
    private bool _redirecting;

    public LiquidityOfficerDashboardPage(FirebaseAuthenticationService authService)
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
            await _authService.VerifyCurrentLiquidityOfficerSessionAsync();
            IsVisible = true;
        }
        catch
        {
            _redirecting = true;

            await DisplayAlert(
                "Access denied",
                "Your account could not be verified, approved, or active. Please sign in again.",
                "OK");

            await Shell.Current.GoToAsync("//LoginPreview");
        }
        finally
        {
            _isVerifying = false;
        }
    }
}
