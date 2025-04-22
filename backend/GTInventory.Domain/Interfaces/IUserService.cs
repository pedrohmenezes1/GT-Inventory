using GTInventory.Application.DTOs.Auth;

namespace GTInventory.Domain.Interfaces;

public interface IUserService
{
    LoginResponse Authenticate(LoginRequest request);
}
