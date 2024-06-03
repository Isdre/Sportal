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
using System.Text;


namespace Sportal.Controllers;

public class AccountController : Controller {
    private readonly SportalDbContext _context;
    private static readonly Random _random = new Random();
    private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

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
            HttpContext.Session.SetString("Token", user.Token);
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
                Role = "User",
                Token = GenerateRandomString(10)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }
        return View();
    }

    public static string GenerateRandomString(int length)
    {
        var stringBuilder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            var randomIndex = _random.Next(_chars.Length);
            stringBuilder.Append(_chars[randomIndex]);
        }

        return stringBuilder.ToString();
    }
}
