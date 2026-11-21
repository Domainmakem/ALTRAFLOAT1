using Plugin.Firebase.Auth;

namespace ALTRAFLOAT.Services;

public sealed class FirebaseAuthenticationService
{
    public async Task CreateAccountAsync(string email, string password)
    {
        ValidateCredentials(email, password);

        await CrossFirebaseAuth.Current
            .CreateUserWithEmailAndPasswordAsync(email.Trim(), password);
    }

    public async Task SignInAsync(string email, string password)
    {
        ValidateCredentials(email, password);

        await CrossFirebaseAuth.Current
            .SignInWithEmailAndPasswordAsync(email.Trim(), password);
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Enter your email address.", nameof(email));

        await CrossFirebaseAuth.Current
            .SendPasswordResetEmailAsync(email.Trim());
    }

    public async Task SignOutAsync()
    {
        await CrossFirebaseAuth.Current.SignOutAsync();
    }

    private static void ValidateCredentials(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Enter your email address.", nameof(email));

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException(
                "Enter your password.", nameof(password));
    }
}
