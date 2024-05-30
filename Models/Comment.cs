using System.ComponentModel.DataAnnotations;

namespace Sportal.Models;
public class Comment {
    [Key]
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime DatePosted { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int MatchId { get; set; }
    public Match Match { get; set; }
}
