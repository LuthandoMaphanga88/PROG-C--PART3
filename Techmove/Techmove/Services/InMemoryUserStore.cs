namespace Techmove.Services;

public class InMemoryUserStore
{
    private readonly List<AppUser> _users =
    [
        new("admin", "admin123", "Admin", "System Admin"),
        new("client", "client123", "Client", "Client User")
    ];

    public AppUser? ValidateCredentials(string username, string password)
    {
        return _users.FirstOrDefault(user =>
            string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase) &&
         
       string.Equals(user.Password, password, StringComparison.Ordinal));
    }
}

public record AppUser(string Username, string Password, string Role, string DisplayName);
