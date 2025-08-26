using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Users
{
    public class RegisterUserDTO
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
