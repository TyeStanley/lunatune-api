namespace Lunatune.Core.Models;

public class User
{
    public Guid Id { get; set; }
    public required string Auth0Id { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
