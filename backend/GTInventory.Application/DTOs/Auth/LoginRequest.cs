namespace GTInventory.Application.DTOs.Auth
{
    public record LoginRequestDto(
        string Username,
        string Password
    );
}