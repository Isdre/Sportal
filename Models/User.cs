using System.ComponentModel.DataAnnotations;

namespace Sportal.Models;
public class User {
    [Key]
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // "Admin" or "User"
    public ICollection<Match> Matches { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Rating> Ratings { get; set; }
}