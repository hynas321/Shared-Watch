using System.ComponentModel.DataAnnotations;

namespace DotnetServer.Api.HttpClasses.Input;

public class RoomCreateInput
{   
    [Required]
    [MinLength(3)]
    [MaxLength(55)]
    public string RoomName { get; set; }

    [MinLength(0)]
    [MaxLength(35)]
    public string RoomPassword { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(25)]
    public string Username { get; set; }
}