using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sportal.Data;
using Sportal.Models;


namespace Sportal.Controllers;

public class RatingsController : Controller {
    private readonly SportalDbContext _context;

    public RatingsController(SportalDbContext context) {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int matchId, bool isLike) {
        if (ModelState.IsValid) {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var existingRating = await _context.Ratings.FirstOrDefaultAsync(r => r.MatchId == matchId && r.UserId == userId);
            if (existingRating != null) {
                existingRating.IsLike = isLike;
                existingRating.DateRated = DateTime.Now;
            } else {
                var rating = new Rating {
                    MatchId = matchId,
                    IsLike = isLike,
                    DateRated = DateTime.Today,
                    UserId = userId
                };
                _context.Ratings.Add(rating);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Matches", new { id = matchId });
        }
        return View();
    }
}
