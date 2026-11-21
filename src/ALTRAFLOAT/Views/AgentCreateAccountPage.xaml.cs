using ALTRAFLOAT.Services;

namespace ALTRAFLOAT.Views;

public partial class AgentCreateAccountPage : ContentPage
{
    private readonly FirebaseAuthenticationService _authService;

    public event EventHandler? BackToLoginRequested;

    public AgentCreateAccountPage(FirebaseAuthenticationService authService)
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
            await _authService.CreateAgentAccountAsync(
                EmailEntry.Text ?? string.Empty,
                PasswordEntry.Text ?? string.Empty,
                UsernameEntry.Text ?? string.Empty,
                AgentIdEntry.Text ?? string.Empty,
                AgentCategoryPicker.SelectedItem?.ToString() ?? string.Empty);

            try { await _authService.SignOutAsync(); }
            catch { /* Registration has already succeeded. */ }

            await DisplayAlert(
                "Account created",
                "Your Agent account has been created. You can now sign in.",
                "OK");

            BackToLoginRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Registration failed", ex.Message, "OK");
        }
        finally
        {
            CreateButton.IsEnabled = true;
        }
    }
}
