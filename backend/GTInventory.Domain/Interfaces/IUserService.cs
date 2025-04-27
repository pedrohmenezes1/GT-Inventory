namespace GTInventory.Domain.Interfaces
{
    public interface IUserService
    {
        AuthenticationResult Authenticate(string username, string password);
    }
}