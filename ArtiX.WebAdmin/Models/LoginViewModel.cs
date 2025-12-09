using System.ComponentModel.DataAnnotations;

namespace ArtiX.WebAdmin.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "User name")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
