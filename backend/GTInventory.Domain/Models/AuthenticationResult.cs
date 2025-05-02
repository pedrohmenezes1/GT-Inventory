namespace GTInventory.Domain.Models
{
    public record AuthenticationResult(
        bool Authenticated,
        UserResult? User,
        string? Token,
        string? Message
    );

    public record UserResult(
        string DisplayName,
        string Email,
        string UserPrincipalName
    );
}