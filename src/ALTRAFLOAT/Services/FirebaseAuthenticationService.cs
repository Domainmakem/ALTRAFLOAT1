using System.Text.RegularExpressions;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Firestore;

namespace ALTRAFLOAT.Services;

public sealed class FirebaseAuthenticationService
{
    private const string PasswordRequirementsMessage =
        "Password must be at least 6 characters and include at least one uppercase letter, one lowercase letter, and one number. Special characters are optional.";

    public static string GetUserFriendlyError(Exception exception)
    {
        var message = exception.ToString().ToLowerInvariant();

        if (message.Contains("weak-password") || message.Contains("weak_password") ||
            message.Contains("weak password") ||
            message.Contains("password-does-not-meet-requirements") ||
            message.Contains("password_does_not_meet_requirements") ||
            message.Contains("password does not meet requirements") ||
            message.Contains("password-policy") || message.Contains("password policy") ||
            message.Contains("password_policy") ||
            message.Contains("password should be at least") ||
            message.Contains("password is too short"))
            return PasswordRequirementsMessage;

        if (exception is ArgumentException)
            return exception.Message;
        if (exception is InvalidOperationException &&
            (exception.Message.StartsWith("Your Liquidity Officer account is pending", StringComparison.Ordinal) ||
             exception.Message.StartsWith("Your Liquidity Officer application was not approved", StringComparison.Ordinal) ||
             exception.Message.StartsWith("Your Liquidity Officer account is suspended", StringComparison.Ordinal) ||
             exception.Message.StartsWith("Your account ", StringComparison.Ordinal) ||
             exception.Message.StartsWith("Agent ", StringComparison.Ordinal) ||
             exception.Message.StartsWith("Liquidity Officer ", StringComparison.Ordinal) ||
             exception.Message.StartsWith("Registration ", StringComparison.Ordinal)))
            return exception.Message;

        if (message.Contains("email-already-in-use") || message.Contains("email already in use"))
            return "An account could not be created with those details. If you already have an account, sign in or reset your password.";
        if (message.Contains("invalid-email") || message.Contains("invalid email"))
            return "Enter a valid email address.";
        if (message.Contains("wrong-password") || message.Contains("user-not-found") || message.Contains("invalid-credential") || message.Contains("invalid login credentials"))
            return "The email or password could not be verified. Check your details and try again.";
        if (message.Contains("too-many-requests") || message.Contains("too many requests"))
            return "There have been too many attempts. Wait a little while and try again.";
        if (message.Contains("network-request-failed") || message.Contains("network error") || message.Contains("unreachable"))
            return "A network error prevented the request. Check your connection and try again.";
        if (message.Contains("operation-not-allowed"))
            return "This sign-in method is currently unavailable. Contact support for help.";

        return "We couldn't complete your request. Check your connection and details, then try again.";
    }

    private static readonly HashSet<string> AgentCategories =
    [
        "Main Agent",
        "Moblie Money Agent",
        "High Agents"
    ];

    public async Task CreateAccountAsync(string email, string password)
    {
        ValidateCredentials(email, password);
        ValidatePasswordRequirements(password);

        await CrossFirebaseAuth.Current
            .CreateUserAsync(email.Trim(), password);
    }


    public async Task CreateAgentAccountAsync(
        string email,
        string password,
        string username,
        string agentId,
        string agentCategory)
    {
        ValidateRegistration(email, password, username);

        if (!Regex.IsMatch(agentId.Trim(), "^[A-Z][A-Z0-9_-]*$"))
            throw new ArgumentException("Enter a valid Agent ID.");

        if (!AgentCategories.Contains(agentCategory.Trim()))
            throw new ArgumentException("Select a valid Agent category.");

        var profile = new Dictionary<string, object>
        {
            ["username"] = username.Trim(),
            ["email"] = email.Trim(),
            ["role"] = "agent",
            ["accountStatus"] = "active",
            ["createdAt"] = FieldValue.ServerTimestamp(),
            ["agentId"] = agentId.Trim(),
            ["agentCategory"] = agentCategory.Trim()
        };

        var reservation = new Dictionary<string, object>
        {
            ["reserved"] = true,
            ["createdAt"] = FieldValue.ServerTimestamp()
        };

        await CreateAccountAndCommitAsync(
            email.Trim(), password, agentId.Trim(), "agentIds",
            profile, reservation);
    }

    public async Task CreateLiquidityOfficerAccountAsync(
        string email,
        string password,
        string username,
        string loId)
    {
        ValidateRegistration(email, password, username);

        if (!Regex.IsMatch(email.Trim(),
            "^[a-z][a-z0-9._%+-]*@gmail[.]com$"))
            throw new ArgumentException(
                "Enter a valid Gmail address using the required format.");

        if (!Regex.IsMatch(loId.Trim(), "^[SC][0-9]+$"))
            throw new ArgumentException(
                "Enter a valid Liquidity Officer ID, such as S1001 or C1001.");

        var profile = new Dictionary<string, object>
        {
            ["username"] = username.Trim(),
            ["email"] = email.Trim(),
            ["role"] = "liquidityOfficer",
            ["accountStatus"] = "pending",
            ["approvalStatus"] = "pending",
            ["createdAt"] = FieldValue.ServerTimestamp(),
            ["loId"] = loId.Trim()
        };

        var reservation = new Dictionary<string, object>
        {
            ["reserved"] = true,
            ["createdAt"] = FieldValue.ServerTimestamp()
        };

        await CreateAccountAndCommitAsync(
            email.Trim(), password, loId.Trim(), "liquidityOfficerIds",
            profile, reservation);
    }

    public async Task SignInAsync(string email, string password)
    {
        ValidateCredentials(email, password);

        await CrossFirebaseAuth.Current
            .SignInWithEmailAndPasswordAsync(email.Trim(), password);
    }

    public async Task SignInAgentAsync(
        string email,
        string password,
        string username,
        string agentId,
        string agentCategory)
    {
        ValidateCredentials(email, password);
        ValidateRequired(username, "Enter your username.");
        ValidateRequired(agentId, "Enter your Agent ID.");
        ValidateRequired(agentCategory, "Select your Agent category.");

        if (!AgentCategories.Contains(agentCategory.Trim()))
            throw new InvalidOperationException("Select a valid Agent category.");

        try
        {
            await SignInAsync(email, password);

            var profile = await GetCurrentUserProfileAsync();

            RequireProfileValue(profile, "uid",
                CrossFirebaseAuth.Current.CurrentUser!.Uid);
            RequireProfileValue(profile, "email", email.Trim(),
                StringComparison.OrdinalIgnoreCase);
            RequireProfileValue(profile, "username", username.Trim(),
                StringComparison.OrdinalIgnoreCase);
            RequireProfileValue(profile, "agentId", agentId.Trim(),
                StringComparison.OrdinalIgnoreCase);
            RequireProfileValue(profile, "agentCategory", agentCategory.Trim());
            RequireProfileValue(profile, "role", "agent");
            RequireProfileValue(profile, "accountStatus", "active");
        }
        catch
        {
            await SignOutAfterFailedVerificationAsync();
            throw;
        }
    }

    public async Task SignInLiquidityOfficerAsync(
        string email,
        string password,
        string username,
        string loId)
    {
        ValidateCredentials(email, password);
        ValidateRequired(username, "Enter your username.");
        ValidateRequired(loId, "Enter your Liquidity Officer ID.");

        try
        {
            await SignInAsync(email, password);

            var profile = await GetCurrentUserProfileAsync();

            var approvalStatus = profile.GetValueOrDefault("approvalStatus")?.ToString();
            var accountStatus = profile.GetValueOrDefault("accountStatus")?.ToString();
            if (IsStatus(approvalStatus, "pending") || IsStatus(accountStatus, "pending"))
                throw new InvalidOperationException(
                    "Your Liquidity Officer account is pending administrator approval. You can sign in after it has been approved.");
            if (IsStatus(approvalStatus, "rejected") || IsStatus(accountStatus, "rejected"))
                throw new InvalidOperationException(
                    "Your Liquidity Officer application was not approved. Contact an administrator for assistance.");
            if (IsStatus(approvalStatus, "suspended") || IsStatus(accountStatus, "suspended"))
                throw new InvalidOperationException(
                    "Your Liquidity Officer account is suspended. Contact an administrator for assistance.");

            RequireProfileValue(profile, "uid",
                CrossFirebaseAuth.Current.CurrentUser!.Uid);
            RequireProfileValue(profile, "email", email.Trim(),
                StringComparison.OrdinalIgnoreCase);
            RequireProfileValue(profile, "username", username.Trim(),
                StringComparison.OrdinalIgnoreCase);
            RequireProfileValue(profile, "loId", loId.Trim(),
                StringComparison.OrdinalIgnoreCase);
            RequireProfileValue(profile, "role", "liquidityOfficer");
            RequireProfileValue(profile, "approvalStatus", "approved");
            RequireProfileValue(profile, "accountStatus", "active");
        }
        catch
        {
            await SignOutAfterFailedVerificationAsync();
            throw;
        }
    }

    public async Task VerifyCurrentAgentSessionAsync()
    {
        try
        {
            var profile = await GetCurrentUserProfileAsync();

            RequireProfileValue(profile, "uid",
                CrossFirebaseAuth.Current.CurrentUser!.Uid);
            RequireProfileValue(profile, "role", "agent");
            RequireProfileValue(profile, "accountStatus", "active");

            ValidateRequired(
                profile.GetValueOrDefault("username")?.ToString() ?? "",
                "Agent username could not be verified.");
            ValidateRequired(
                profile.GetValueOrDefault("agentId")?.ToString() ?? "",
                "Agent ID could not be verified.");

            var category = profile.GetValueOrDefault("agentCategory")?.ToString();

            if (category is null || !AgentCategories.Contains(category))
                throw new InvalidOperationException(
                    "Agent category could not be verified.");
        }
        catch
        {
            await SignOutAfterFailedVerificationAsync();
            throw;
        }
    }

    public async Task VerifyCurrentLiquidityOfficerSessionAsync()
    {
        try
        {
            var profile = await GetCurrentUserProfileAsync();

            RequireProfileValue(profile, "uid",
                CrossFirebaseAuth.Current.CurrentUser!.Uid);
            RequireProfileValue(profile, "role", "liquidityOfficer");
            RequireProfileValue(profile, "approvalStatus", "approved");
            RequireProfileValue(profile, "accountStatus", "active");

            ValidateRequired(
                profile.GetValueOrDefault("username")?.ToString() ?? "",
                "Liquidity Officer username could not be verified.");
            ValidateRequired(
                profile.GetValueOrDefault("loId")?.ToString() ?? "",
                "Liquidity Officer ID could not be verified.");
        }
        catch
        {
            await SignOutAfterFailedVerificationAsync();
            throw;
        }
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


    private static void ValidateRegistration(
        string email, string password, string username)
    {
        ValidateCredentials(email, password);
        ValidateRequired(username, "Enter your username.");

        ValidatePasswordRequirements(password);
    }

    private static void ValidatePasswordRequirements(string password)
    {
        if (password.Length < 6)
            throw new ArgumentException(
                "Your password must contain at least 6 characters.");
        if (!password.Any(char.IsUpper))
            throw new ArgumentException(
                "Your password must contain at least one uppercase letter.");
        if (!password.Any(char.IsLower))
            throw new ArgumentException(
                "Your password must contain at least one lowercase letter.");
        if (!password.Any(char.IsDigit))
            throw new ArgumentException(
                "Your password must contain at least one number.");
    }

    private static async Task CreateAccountAndCommitAsync(
        string email,
        string password,
        string reservationId,
        string reservationCollection,
        Dictionary<string, object> profile,
        Dictionary<string, object> reservation)
    {
        var expectedRole = profile["role"]?.ToString()
            ?? throw new InvalidOperationException("Registration role is missing.");

        var idField = reservationCollection == "agentIds"
            ? "agentId"
            : "loId";

        var expectedId = profile[idField]?.ToString()
            ?? throw new InvalidOperationException("Registration ID is missing.");

        var authUser = await CrossFirebaseAuth.Current
            .CreateUserAsync(email, password);

        profile["uid"] = authUser.Uid;
        reservation["uid"] = authUser.Uid;

        try
        {
            var firestore = CrossFirebaseFirestore.Current;
            var profileReference = firestore.GetDocument($"users/{authUser.Uid}");
            var reservationReference = firestore.GetDocument(
                $"{reservationCollection}/{reservationId}");

            var batch = firestore.CreateBatch();
            batch.SetData(profileReference, profile);
            batch.SetData(reservationReference, reservation);
            await batch.CommitAsync();
        }
        catch (Exception commitError)
        {
            try
            {
                var savedProfile = await CrossFirebaseFirestore.Current
                    .GetDocument($"users/{authUser.Uid}")
                    .GetDocumentSnapshotAsync<Dictionary<string, object>>();

                var savedData = savedProfile.Data;
                if (savedData is not null &&
                    savedData.TryGetValue("uid", out var savedUid) &&
                    savedData.TryGetValue("role", out var savedRole) &&
                    savedData.TryGetValue(idField, out var savedId) &&
                    string.Equals(savedUid?.ToString(), authUser.Uid,
                        StringComparison.Ordinal) &&
                    string.Equals(savedRole?.ToString(), expectedRole,
                        StringComparison.Ordinal) &&
                    string.Equals(savedId?.ToString(), expectedId,
                        StringComparison.Ordinal))
                {
                    return;
                }
            }
            catch (Exception verificationError)
            {
                throw new InvalidOperationException(
                    "Registration could not be confirmed. Your account may "
                    + "have been created. Contact support before trying again.",
                    new AggregateException(commitError, verificationError));
            }

            try
            {
                await authUser.DeleteAsync();
            }
            catch (Exception cleanupError)
            {
                throw new InvalidOperationException(
                    "Registration failed and automatic account cleanup "
                    + "could not be confirmed. Contact support before "
                    + "trying again.",
                    new AggregateException(commitError, cleanupError));
            }

            throw new InvalidOperationException(
                "Registration could not be completed. The ID may already "
                + "be reserved, or the profile could not be saved. "
                + "Check your details and try again.",
                commitError);
        }
    }

    private static async Task<Dictionary<string, object>>
        GetCurrentUserProfileAsync()
    {
        var user = CrossFirebaseAuth.Current.CurrentUser;

        if (user is null)
            throw new InvalidOperationException(
                "Your sign-in session could not be verified.");

        var snapshot = await CrossFirebaseFirestore.Current
            .GetDocument($"users/{user.Uid}")
            .GetDocumentSnapshotAsync<Dictionary<string, object>>();

        var profile = snapshot.Data;
        if (profile is null)
            throw new InvalidOperationException(
                "Your account profile could not be found.");

        return profile;
    }

    private static void RequireProfileValue(
        Dictionary<string, object> profile,
        string field,
        string expected,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (!profile.TryGetValue(field, out var value) ||
            !string.Equals(value?.ToString(), expected, comparison))
        {
            throw new InvalidOperationException(
                "Your account details or access status could not be verified.");
        }
    }

    private static async Task SignOutAfterFailedVerificationAsync()
    {
        try
        {
            await CrossFirebaseAuth.Current.SignOutAsync();
        }
        catch
        {
            // Preserve the original profile-verification error.
        }
    }

    private static void ValidateRequired(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(message);
    }

    private static bool IsStatus(string? actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

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
