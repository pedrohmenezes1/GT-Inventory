namespace GTInventory.Domain.Models
{
    public record AuthenticationResult(
        bool Authenticated = true,
        UserResult? User = null,
        string? Token = null,
        string? Message = null
    );

    public record UserResult(
        string DisplayName,
        string Email,
        string UserPrincipalName
    );
}