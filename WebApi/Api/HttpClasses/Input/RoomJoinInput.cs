using System.ComponentModel.DataAnnotations;

namespace WebApi.Api.HttpClasses.Input;

public class RoomJoinInput
{   
    [MinLength(0)]
    [MaxLength(35)]
    public string RoomPassword { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(25)]
    public string Username { get; set; }
}