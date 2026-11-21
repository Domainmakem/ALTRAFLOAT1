using System.Text.RegularExpressions;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Firestore;

namespace ALTRAFLOAT.Services;

public sealed class FirebaseAuthenticationService
{
    private static readonly HashSet<string> AgentCategories =
    [
        "Main Agent",
        "Moblie Money Agent",
        "High Agents"
    ];

    public async Task CreateAccountAsync(string email, string password)
    {
        ValidateCredentials(email, password);

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

        if (password.Length < 6)
            throw new ArgumentException(
                "Your password must contain at least 6 characters.");
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

                if (savedProfile.Exists &&
                    savedProfile.Data is not null &&
                    savedProfile.Data.TryGetValue("uid", out var savedUid) &&
                    savedProfile.Data.TryGetValue("role", out var savedRole) &&
                    savedProfile.Data.TryGetValue(idField, out var savedId) &&
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

        if (!snapshot.Exists || snapshot.Data is null)
            throw new InvalidOperationException(
                "Your account profile could not be found.");

        return snapshot.Data;
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
            await SignOutAsync();
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
