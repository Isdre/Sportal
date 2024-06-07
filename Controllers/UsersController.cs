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
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly SportalDbContext _context;

        public UsersController(SportalDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetByUsername")]
        public async Task<ActionResult<User>> GetByUsername()
        {
            var username = HttpContext.Request.Headers["username"].ToString();
            var token = HttpContext.Request.Headers["token"].ToString();

            var user = await _context.Users
                                    .FirstOrDefaultAsync(u => u.Username == username && u.Token == token);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
    }
}