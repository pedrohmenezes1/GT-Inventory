using System.ComponentModel.DataAnnotations;

namespace GTInventory.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Username é obrigatório")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; }
    }
}