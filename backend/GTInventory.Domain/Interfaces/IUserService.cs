using GTInventory.Domain.Models;
using System.Threading.Tasks;

namespace GTInventory.Domain.Interfaces
{
    public interface IUserService
    {
        Task<AuthenticationResult> AuthenticateAsync(string username, string password);
    }
}