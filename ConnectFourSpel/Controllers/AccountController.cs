using ConnectFourSpel.DAL;
using ConnectFourSpel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace ConnectFourSpel.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        [HttpGet]
        public IActionResult EditUsername()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();

            return View(new EditUsernameVm { Username = user.Username });
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditUsername(EditUsernameVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();


            var ok = UserMethods.Update(id, model.Username, user.PasswordHash);
            if (!ok)
            {
                ModelState.AddModelError("", "Kunde inte uppdatera användarnamnet.");
                return View(model);
            }


            HttpContext.Session.SetString("Username", model.Username);

            return RedirectToAction(nameof(EditUsername));
        }


        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordVm());


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

            return RedirectToAction(nameof(ChangePassword));
        }
    }
}
