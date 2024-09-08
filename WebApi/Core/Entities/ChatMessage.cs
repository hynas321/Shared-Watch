using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Core.Entities;

public class ChatMessage
{
    [Key, Column(Order = 0)]
    public string Username { get; set; }

    [Key, Column(Order = 1)]
    public string Date { get; set; }

    [Required]
    public string Text { get; set; }

    // Foreign Key
    public string RoomHash { get; set; }
}