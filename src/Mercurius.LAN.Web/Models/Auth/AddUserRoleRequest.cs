namespace Mercurius.LAN.Web.Models.Auth
{
    public class AddUserRoleRequest
    {
        public string Username { get; set; } = null!;
        public string RoleName { get; set; } = null!;
    }
}