using ALTRAFLOAT.Services;

namespace ALTRAFLOAT.Views;

public partial class LiquidityOfficerLoginPage : ContentPage
{
    public event EventHandler? AgentLoginRequested;
    public event EventHandler? CreateAccountRequested;

    private readonly FirebaseAuthenticationService _authService;
    private bool _loginInProgress;
    private bool _resetInProgress;

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
        if (_resetInProgress)
            return;

        _resetInProgress = true;
        try
        {
            var email = EmailEntry.Text;
            if (string.IsNullOrWhiteSpace(email))
            {
                email = await Shell.Current.DisplayPromptAsync(
                    "Reset password",
                    "Enter your account email address.",
                    "Continue",
                    "Cancel",
                    "Email address",
                    keyboard: Keyboard.Email);
            }

            if (string.IsNullOrWhiteSpace(email))
                return;

            ForgotPasswordLabel.IsEnabled = false;
            ResetActivityIndicator.IsVisible = ResetActivityIndicator.IsRunning = true;
            await _authService.SendPasswordResetEmailAsync(email.Trim());
            await Shell.Current.DisplayAlert(
                "Password reset",
                "If the account is registered, a password reset email will be sent.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Password reset failed",
                FirebaseAuthenticationService.GetUserFriendlyError(ex), "OK");
        }
        finally
        {
            _resetInProgress = false;
            ForgotPasswordLabel.IsEnabled = true;
            ResetActivityIndicator.IsVisible = ResetActivityIndicator.IsRunning = false;
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (_loginInProgress)
            return;

        _loginInProgress = true;
        LoginButton.IsEnabled = false;
        LoginActivityIndicator.IsVisible = LoginActivityIndicator.IsRunning = true;
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
            await Shell.Current.DisplayAlert("Login failed",
                FirebaseAuthenticationService.GetUserFriendlyError(ex), "OK");
        }
        finally
        {
            _loginInProgress = false;
            LoginButton.IsEnabled = true;
            LoginActivityIndicator.IsVisible = LoginActivityIndicator.IsRunning = false;
        }
    }
}
