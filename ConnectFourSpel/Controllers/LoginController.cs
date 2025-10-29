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
        // ---- PROFIL ----
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

        // ---- ÄNDRA ANVÄNDARNAMN ----
        [Authorize]
        [HttpGet]
        public IActionResult EditUsername()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();

            return View(new EditUsernameVm { Username = user.Username });
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditUsername(EditUsernameVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var current = UserMethods.GetById(id);
            if (current == null) return NotFound();

            // uppdatera endast användarnamn
            var ok = UserMethods.Update(id, model.Username, current.PasswordHash);
            if (!ok)
            {
                ModelState.AddModelError("", "Kunde inte uppdatera användarnamnet.");
                return View(model);
            }

            // uppdatera session + claims så namnet slår igenom direkt i UI
            HttpContext.Session.SetString("Username", model.Username);
            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, id.ToString()),
        new(ClaimTypes.Name, model.Username)
    };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
                       .GetAwaiter().GetResult();

            return RedirectToAction(nameof(Profile));
        }

        // ---- BYT LÖSENORD ----
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordVm());

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("", "Nuvarande lösenord stämmer inte.");
                return View(model);
            }

            var newHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            var ok = UserMethods.Update(id, user.Username, newHash);
            if (!ok)
            {
                ModelState.AddModelError("", "Kunde inte uppdatera lösenordet.");
                return View(model);
            }

            return RedirectToAction(nameof(Profile));
        }
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

        // POST: /Login/Delete (genomför)
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ConfirmDelete()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr)) return RedirectToAction(nameof(Login));

            var id = int.Parse(idStr);

            // Radera i DB
            var ok = UserMethods.Delete(id);
            if (!ok)
            {
                ModelState.AddModelError("", "Kunde inte radera kontot.");
                // visa bekräftelsesidan igen
                var user = UserMethods.GetById(id);
                return View("Delete", new ProfileVm { Id = user?.Id ?? id, Username = user?.Username ?? "" });
            }

            // Logga ut och rensa session
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                       .GetAwaiter().GetResult();
            HttpContext.Session.Clear();

            // Skicka användaren till startsidan
            return RedirectToAction("Index", "Home");
        }


    }

}
