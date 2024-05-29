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
                };
                _context.Add(match);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var match = await _context.Matches
                .Include(m => m.Comments)
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
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return BadRequest("User not found.");


            var comment = new Comment
            {
                MatchId = matchId,
                Content = content,
                UserId = Int16.Parse(userId),
                User = user,
                DatePosted = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = matchId });
        }

        [HttpPost]
        public async Task<IActionResult> RateMatch(int matchId, bool like)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var rating = new Rating
            {
                MatchId = matchId,
                UserId = Int16.Parse(userId),
                IsLike = like
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = matchId });
        }
    }
}
