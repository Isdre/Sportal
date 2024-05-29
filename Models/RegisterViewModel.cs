using System.ComponentModel.DataAnnotations;

namespace Sportal.Models;

public class RegisterViewModel {
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}
