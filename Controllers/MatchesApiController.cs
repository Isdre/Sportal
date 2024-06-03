using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sportal.Data;
using Sportal.Models;

namespace Sportal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesApiController : ControllerBase
    {
        private readonly SportalDbContext _context;

        public MatchesApiController(SportalDbContext context)
        {
            _context = context;
        }

        private bool ValidateUser(string username, string token, out User user)
        {
            user = _context.Users.FirstOrDefault(u => u.Username == username && u.Token == token);
            return user != null;
        }

        // GET: api/MatchesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Match>>> GetMatches([FromHeader] string username, [FromHeader] string token)
        {
            if (!ValidateUser(username, token, out _)) return Unauthorized();

            return await _context.Matches.Include(m => m.User).Include(m => m.Comments).Include(m => m.Ratings).ToListAsync();
        }

        // GET: api/MatchesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Match>> GetMatch(int id, [FromHeader] string username, [FromHeader] string token)
        {
            if (!ValidateUser(username, token, out _)) return Unauthorized();

            var match = await _context.Matches.Include(m => m.User).Include(m => m.Comments).Include(m => m.Ratings).FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            return match;
        }

        // POST: api/MatchesApi
        [HttpPost]
        public async Task<ActionResult<Match>> PostMatch(Match match, [FromHeader] string username, [FromHeader] string token)
        {
            if (!ValidateUser(username, token, out var user)) return Unauthorized();

            match.UserId = user.Id;
            match.DateAdded = DateTime.UtcNow;

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, match);
        }

        // PUT: api/MatchesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMatch(int id, Match match, [FromHeader] string username, [FromHeader] string token)
        {
            if (!ValidateUser(username, token, out var user)) return Unauthorized();

            if (id != match.Id)
            {
                return BadRequest();
            }

            // Ensure the user owns the match or is an admin
            var existingMatch = await _context.Matches.FindAsync(id);
            if (existingMatch == null || (existingMatch.UserId != user.Id && user.Role != "Admin"))
            {
                return Forbid();
            }

            match.DateAdded = existingMatch.DateAdded;  // Preserve the original DateAdded
            match.UserId = existingMatch.UserId;        // Preserve the original UserId
            match.LikesCount = existingMatch.LikesCount; // Preserve the original LikesCount
            match.DislikesCount = existingMatch.DislikesCount; // Preserve the original DislikesCount

            _context.Entry(existingMatch).CurrentValues.SetValues(match);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MatchExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/MatchesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatch(int id, [FromHeader] string username, [FromHeader] string token)
        {
            if (!ValidateUser(username, token, out var user)) return Unauthorized();

            var match = await _context.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            // Ensure the user owns the match or is an admin
            if (match.UserId != user.Id && user.Role != "Admin")
            {
                return Forbid();
            }

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MatchExists(int id)
        {
            return _context.Matches.Any(e => e.Id == id);
        }
    }
}
