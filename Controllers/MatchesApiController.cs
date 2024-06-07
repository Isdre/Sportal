using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sportal.Data;
using Sportal.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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

        private async Task<bool> ValidateTokenAsync()
        {
            var usernameHeader = HttpContext.Request.Headers["username"].ToString();
            var tokenHeader = HttpContext.Request.Headers["token"].ToString();

            Console.WriteLine($"{usernameHeader} {tokenHeader}");

            if (string.IsNullOrEmpty(usernameHeader) || string.IsNullOrEmpty(tokenHeader))
            {
                return false;
            }

            var user = await _context.Users
                                    .Where(u => u.Username == usernameHeader && u.Token == tokenHeader)
                                    .SingleOrDefaultAsync();

            return user != null;
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches()
        {
            if (!await ValidateTokenAsync())
            {
                return Unauthorized();
            }

            var matches = await _context.Matches.ToListAsync();
            return Ok(matches);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Match>> GetMatch(int id)
        {
            if (!await ValidateTokenAsync())
                return Unauthorized();

            var match = await _context.Matches.FindAsync(id);

            if (match == null)
            {
                return NotFound();
            }

            return match;
        }

        [HttpPost]
        public async Task<ActionResult<Match>> PostMatch(Match match)
        {
            if (!await ValidateTokenAsync())
                return Unauthorized();

            // Extract username and token from headers
            var usernameHeader = HttpContext.Request.Headers["username"].ToString();
            var tokenHeader = HttpContext.Request.Headers["token"].ToString();

            // Find the user based on username and token
            var user = await _context.Users
                                    .FirstOrDefaultAsync(u => u.Username == usernameHeader && u.Token == tokenHeader);

            if (user == null)
            {
                return Unauthorized();
            }

            // Assign the userId to the match
            match.UserId = user.Id;

            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, match);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMatch(int id, Match match)
        {
            if (!await ValidateTokenAsync())
                return Unauthorized();

            // Extract username and token from headers
            var usernameHeader = HttpContext.Request.Headers["username"].ToString();
            var tokenHeader = HttpContext.Request.Headers["token"].ToString();

            // Find the user based on username and token
            var user = await _context.Users
                                    .FirstOrDefaultAsync(u => u.Username == usernameHeader && u.Token == tokenHeader);

            if (user == null)
            {
                return Unauthorized();
            }

            // Ensure the match belongs to the user
            if (id != match.Id || match.UserId != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(match).State = EntityState.Modified;

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            if (!await ValidateTokenAsync())
                return Unauthorized();

            var match = await _context.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
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
