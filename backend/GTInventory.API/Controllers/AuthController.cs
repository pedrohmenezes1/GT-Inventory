using Microsoft.AspNetCore.Mvc;
using GTInventory.Application.DTOs.Auth;
using GTInventory.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace GTInventory.API.Controllers
{
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
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var result = _userService.Authenticate(request.Username, request.Password);
            
            if (!result.Authenticated)
                return Unauthorized(new { message = result.Message });

            return Ok(new LoginResponseDto(
                result.Token,
                result.User.DisplayName,
                result.User.Email
            ));
        }
    }
}