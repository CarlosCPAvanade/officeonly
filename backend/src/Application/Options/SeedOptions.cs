namespace Application.Options;

public class SeedOptions
{
    public const string SectionName = "Seed";
    public string AdminUserName { get; set; } = "admin";
    public string AdminEmail { get; set; } = "admin@example.com";
    public string AdminPassword { get; set; } = "ChangeThisAdminPassword!";
}
