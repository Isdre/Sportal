using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

using Sportal.Data;
using Sportal.Models;


namespace Sportal.Controllers;

public class AccountController : Controller {
    private readonly SportalDbContext _context;

    public AccountController(SportalDbContext context) {
        _context = context;
    }

    public IActionResult Login() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password) {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) {
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.Role);
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View();
    }

    public IActionResult Logout() {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult Register() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string username, string password)
    {
        if (ModelState.IsValid)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already taken.");
                return View();
            }

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }
        return View();
    }
}
