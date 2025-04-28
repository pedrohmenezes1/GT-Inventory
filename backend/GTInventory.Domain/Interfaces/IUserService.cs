// GTInventory.Domain/Interfaces/IUserService.cs
using GTInventory.Domain.Models;

namespace GTInventory.Domain.Interfaces
{
    public interface IUserService
    {
        AuthenticationResult Authenticate(string username, string password);
    }
}