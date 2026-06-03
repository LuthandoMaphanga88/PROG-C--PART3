namespace Techmove.Services;

public class InMemoryUserStore
{
    private readonly List<InMemoryAppUser> _users =
    [
        new("admin", "admin123", "Admin", "System Admin"),
        new("client", "client123", "Client", "Client User")
    ];

    public InMemoryAppUser? ValidateCredentials(string username, string password)
    {
        return _users.FirstOrDefault(user =>
            string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(user.Password, password, StringComparison.Ordinal));
    }
}

public record InMemoryAppUser(string Username, string Password, string Role, string DisplayName);
