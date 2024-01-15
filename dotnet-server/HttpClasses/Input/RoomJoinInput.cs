using System.ComponentModel.DataAnnotations;

public class RoomJoinInput
{   
    [MinLength(0)]
    public string RoomPassword { get; set; }

    [Required]
    public string Username { get; set; }
}