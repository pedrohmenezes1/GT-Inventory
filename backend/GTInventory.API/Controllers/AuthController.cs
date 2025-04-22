using GTInventory.Application.DTOs.Auth;
using GTInventory.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GTInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = _userService.Authenticate(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Usuário ou senha inválidos.");
        }
    }
}
