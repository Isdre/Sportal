using System.ComponentModel.DataAnnotations;

namespace Sportal.Models;
public class Match {
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string YouTubeLink { get; set; }
    public DateTime DateAdded { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Rating> Ratings { get; set; }

    public int LikesCount {get; set;}
    public int DislikesCount {get; set;}
}
