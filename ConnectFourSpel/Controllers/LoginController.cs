using System.Security.Claims;
using ConnectFourSpel.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ConnectFourSpel.Models;


namespace ConnectFourSpel.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            var user = UserMethods.GetByUsername(username);
            if (user == null)
            {
                ModelState.AddModelError("", "Fel användarnamn eller lösenord.");
                return View();
            }

            // SÄKERT: BCrypt – behåll detta
            var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            // SUPERSIMPELT (om du vill utan hash): var ok = (user.PasswordHash == password);

            if (!ok)
            {
                ModelState.AddModelError("", "Fel användarnamn eller lösenord.");
                return View();
            }

            SignIn(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Fyll i användarnamn och lösenord.");
                return View();
            }

            if (UserMethods.GetByUsername(username) != null)
            {
                ModelState.AddModelError("", "Användarnamnet är upptaget.");
                return View();
            }

            // SÄKERT: hash
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            // SUPERSIMPELT (utan hash): var hash = password;

            var newId = UserMethods.Create(username, hash);

            var user = new UserDetails
            {
                Id = newId,
                Username = username,
                PasswordHash = hash
            };

            SignIn(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                       .GetAwaiter().GetResult();
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        private void SignIn(UserDetails user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
                       .GetAwaiter().GetResult();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
        }
        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr)) return RedirectToAction(nameof(Login));

            var id = int.Parse(idStr);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();

            return View(new ProfileVm { Id = user.Id, Username = user.Username });
        }
        // GET: /Login/Delete  (bekräftelsesida)
        [Authorize]
        [HttpGet]
        public IActionResult Delete()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr)) return RedirectToAction(nameof(Login));

            var id = int.Parse(idStr);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();

            return View(new ProfileVm { Id = user.Id, Username = user.Username });
        }

        // POST: /Login/ConfirmDelete  (utför raderingen)
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ConfirmDelete()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr)) return RedirectToAction(nameof(Login));

            var id = int.Parse(idStr);

            var ok = UserMethods.Delete(id); // din DAL-metod
            if (!ok)
            {
                // Vanlig orsak: FK-beroenden i DB. Visa fel och gå tillbaka till Delete-vyn.
                ModelState.AddModelError("", "Kunde inte radera kontot. Finns det relaterade poster?");
                var user = UserMethods.GetById(id);
                return View("Delete", new ProfileVm { Id = user?.Id ?? id, Username = user?.Username ?? "" });
            }

            // Logga ut + rensa session
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                .GetAwaiter().GetResult();
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }


    }

}
