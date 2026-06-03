namespace Techmove.Models;

/// <summary>
/// Represents a client in the database.
/// </summary>
public class Client
{
    public int Id { get; set; }
    public string AccountUsername { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactDetails { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Contract> Contracts { get; set; } = [];
}
