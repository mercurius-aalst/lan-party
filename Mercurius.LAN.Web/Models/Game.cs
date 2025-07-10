namespace Mercurius.LAN.Web.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PictureUrl { get; set; }
    public string Status { get; set; }
    public string BracketType { get; set; }
    public string Format { get; set; }
    public string FinalsFormat { get; set; }
    public ParticipantType ParticipantType { get; set; }
    public string Description { get; set; }
}
