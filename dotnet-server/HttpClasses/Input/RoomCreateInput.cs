using System.ComponentModel.DataAnnotations;

public class RoomCreateInput
{   
    [Required]
    [MinLength(3)]
    public string RoomName { get; set; }

    [MinLength(0)]
    public string Password { get; set; } = "";

    [Required]
    public string Username { get; set; }
}