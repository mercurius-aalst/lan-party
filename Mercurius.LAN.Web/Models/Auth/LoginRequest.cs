using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.Models.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}