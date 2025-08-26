using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.Models.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}