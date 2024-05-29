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

public class CommentsController : Controller {
    private readonly SportalDbContext _context;

    public CommentsController(SportalDbContext context) {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int matchId, string content) {
        if (ModelState.IsValid) {
            var comment = new Comment {
                MatchId = matchId,
                Content = content,
                DatePosted = DateTime.Today,
                UserId = int.Parse(HttpContext.Session.GetString("UserId"))
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Matches", new { id = matchId });
        }
        return View();
    }
}
