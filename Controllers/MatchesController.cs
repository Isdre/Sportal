using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sportal.Data;
using Sportal.Models;

namespace Sportal.Controllers
{
    public class MatchesController : Controller
    {
        private readonly SportalDbContext _context;

        public MatchesController(SportalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var matches = await _context.Matches.Include(m => m.Comments).ToListAsync();
            return View(matches);
        }

        // GET: Matches/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Title, string Description, string YouTubeLink)
        {
            if (ModelState.IsValid)
            {
                var match = new Match{
                    Title = Title,
                    Description = Description,
                    YouTubeLink = YouTubeLink,
                    DateAdded = DateTime.Today,
                    UserId = Int16.Parse(HttpContext.Session.GetString("UserId")),
                    LikesCount = 0,
                    DislikesCount = 0,
                };
                _context.Add(match);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? matchId)
        {
            if (matchId == null) return NotFound();

            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null) return NotFound();
            
            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var match = await _context.Matches
                .Include(m => m.Comments)
                .ThenInclude(c => c.User)
                .ThenInclude(r => r.Ratings)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }
            return View(match);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int matchId, string content)
        {
            if (HttpContext.Session.GetString("UserId") == null) 
                return RedirectToAction("Login", "Account");

            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var comment = new Comment
            {
                MatchId = matchId,
                Content = content,
                UserId = userId,
                DatePosted = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = matchId });
        }

        [HttpPost]
        public async Task<IActionResult> Rate(int matchId, bool like)
        {
            if (HttpContext.Session.GetString("UserId") == null) return RedirectToAction("Login", "Account");

            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Find existing rating
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.MatchId == matchId && r.UserId == userId);

            var match = await _context.Matches.FindAsync(matchId);
            if (match == null) return NotFound();
            
            if (existingRating != null) {
                // Update existing rating
                if (existingRating.IsLike && !like) {
                    match.LikesCount--;
                    match.DislikesCount++;
                    existingRating.IsLike = like;
                }
                else if (!existingRating.IsLike && like) {
                    match.LikesCount++;
                    match.DislikesCount--;
                    existingRating.IsLike = like;
                } else if (existingRating.IsLike == like) {
                    if (like) match.LikesCount--;
                    else match.DislikesCount--;
                    _context.Ratings.Remove(existingRating);
                }
                
            }
            else {
                var rating = new Rating {
                    MatchId = matchId,
                    UserId = userId,
                    IsLike = like
                };

                _context.Ratings.Add(rating);

                if (like)  match.LikesCount++;
                else match.DislikesCount++;
            }


            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = matchId });
        }

 
    }
}
