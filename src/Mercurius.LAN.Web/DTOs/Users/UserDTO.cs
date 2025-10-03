namespace Mercurius.LAN.Web.DTOs.Users
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public IEnumerable<string> Roles { get; set; } = null!;
    }
}
