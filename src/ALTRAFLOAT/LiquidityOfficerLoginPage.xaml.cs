using ALTRAFLOAT.Services;

namespace ALTRAFLOAT.Views;

public partial class LiquidityOfficerLoginPage : ContentPage
{
    public event EventHandler? AgentLoginRequested;
    public event EventHandler? CreateAccountRequested;

    private readonly FirebaseAuthenticationService _authService;

    public LiquidityOfficerLoginPage(FirebaseAuthenticationService authService)
    {
        _authService = authService;
        InitializeComponent();
        CreateAccountSpan.GestureRecognizers.Add(
            new TapGestureRecognizer
            {
                Command = new Command(() =>
                    CreateAccountRequested?.Invoke(this, EventArgs.Empty))
            });
        ForgotPasswordLabel.GestureRecognizers.Add(
            new TapGestureRecognizer
            {
                Command = new Command(async () => await OnForgotPasswordClicked())
            });
    }

    private void OnAgentTabClicked(object sender, EventArgs e)
    {
        AgentLoginRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnLiquidityTabClicked(object sender, EventArgs e)
    {
        // Liquidity Officer Login is already the active screen.
    }

    private void OnPasswordVisibilityClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        PasswordVisibilityButton.Text = PasswordEntry.IsPassword ? "◉" : "◎";
    }

    private async Task OnForgotPasswordClicked()
    {
        var email = EmailEntry.Text;

        if (string.IsNullOrWhiteSpace(email))
        {
            email = await DisplayPromptAsync(
                "Reset password",
                "Enter your account email address.",
                "Continue",
                "Cancel",
                "Email address",
                keyboard: Keyboard.Email);
        }

        if (string.IsNullOrWhiteSpace(email))
            return;

        try
        {
            await _authService.SendPasswordResetEmailAsync(email.Trim());
            await DisplayAlert(
                "Password reset",
                "If the account is registered, a password reset email will be sent.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Password reset failed", ex.Message, "OK");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            await _authService.SignInLiquidityOfficerAsync(
                EmailEntry.Text ?? string.Empty,
                PasswordEntry.Text ?? string.Empty,
                UsernameEntry.Text ?? string.Empty,
                LiquidityOfficerIdEntry.Text ?? string.Empty);

            await Shell.Current.GoToAsync(nameof(LiquidityOfficerDashboardPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Login failed", ex.Message, "OK");
        }
    }
}
