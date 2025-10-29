using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ConnectFourSpel.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConnectFourSpel.Models;


namespace ConnectFourSpel.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // GET /Account/EditUsername
        [HttpGet]
        public IActionResult EditUsername()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();

            return View(new EditUsernameVm { Username = user.Username });
        }

        // POST /Account/EditUsername
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditUsername(EditUsernameVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();

            // uppdatera endast användarnamnet, behåll hash
            var ok = UserMethods.Update(id, model.Username, user.PasswordHash);
            if (!ok)
            {
                ModelState.AddModelError("", "Kunde inte uppdatera användarnamnet.");
                return View(model);
            }

            // uppdatera session/claim om du visar Username från claims/session
            HttpContext.Session.SetString("Username", model.Username);

            return RedirectToAction(nameof(EditUsername));
        }

        // GET /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordVm());

        // POST /Account/ChangePassword
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = UserMethods.GetById(id);
            if (user == null) return NotFound();

            // verifiera nuvarande lösen
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

 /*  public class EditUsernameVm
    {
        [Required, StringLength(40)]
        public string Username { get; set; } = "";
    }

    public class ChangePasswordVm
    {
        [Required]
        public string CurrentPassword { get; set; } = "";

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = "";

        [Required, Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; } = "";
    }
}//
 */