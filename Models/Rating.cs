using System.ComponentModel.DataAnnotations;

namespace Sportal.Models;
public class Rating {
    [Key]
    public int Id { get; set; }
    public bool IsLike { get; set; } // True if 'like', False if 'dislike'
    public DateTime DateRated { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int MatchId { get; set; }
    public Match Match { get; set; }
}
