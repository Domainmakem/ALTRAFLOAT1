using ALTRAFLOAT.Services;

namespace ALTRAFLOAT.Views;

public partial class LiquidityOfficerCreateAccountPage : ContentPage
{
    private readonly FirebaseAuthenticationService _authService;

    public event EventHandler? BackToLoginRequested;

    public LiquidityOfficerCreateAccountPage(FirebaseAuthenticationService authService)
    {
        _authService = authService;
        InitializeComponent();
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        BackToLoginRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnCreateAccountClicked(object sender, EventArgs e)
    {
        CreateButton.IsEnabled = false;

        try
        {
            await _authService.CreateLiquidityOfficerAccountAsync(
                EmailEntry.Text ?? string.Empty,
                PasswordEntry.Text ?? string.Empty,
                UsernameEntry.Text ?? string.Empty,
                LiquidityOfficerIdEntry.Text ?? string.Empty);

            try
            {
                await _authService.SignOutAsync();
            }
            catch
            {
                // Registration has already succeeded.
            }

            await Shell.Current.DisplayAlert(
                "Registration submitted",
                "Your Liquidity Officer account has been created and is awaiting administrator approval.",
                "OK");

            BackToLoginRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Registration failed",
                FirebaseAuthenticationService.GetUserFriendlyError(ex), "OK");
        }
        finally
        {
            CreateButton.IsEnabled = true;
        }
    }
}
