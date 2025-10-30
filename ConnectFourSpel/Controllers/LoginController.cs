using ConnectFourSpel.DAL;
using ConnectFourSpel.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


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


            var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);



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


            var hash = BCrypt.Net.BCrypt.HashPassword(password);



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


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ConfirmDelete()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr)) return RedirectToAction(nameof(Login));

            var id = int.Parse(idStr);

            var ok = UserMethods.Delete(id);
            if (!ok)
            {

                ModelState.AddModelError("", "Kunde inte radera kontot. Finns det relaterade poster?");
                var user = UserMethods.GetById(id);
                return View("Delete", new ProfileVm { Id = user?.Id ?? id, Username = user?.Username ?? "" });
            }


            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                .GetAwaiter().GetResult();
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }


    }

}
