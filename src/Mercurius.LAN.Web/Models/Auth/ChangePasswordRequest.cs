namespace Mercurius.LAN.Web.Models.Auth
{
    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; }
        public string CurrentPassword { get; set; }

    }
}
