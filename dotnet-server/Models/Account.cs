using System.ComponentModel.DataAnnotations;

public class Account {
    [Key]
    [Required]
    [MinLength(3)]
    [MaxLength(15)]
    public string Username { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}