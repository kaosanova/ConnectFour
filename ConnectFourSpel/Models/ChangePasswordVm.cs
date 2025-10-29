using System.ComponentModel.DataAnnotations;
namespace ConnectFourSpel.Models
{
    public class ChangePasswordVm
    {
        [Required] public string CurrentPassword { get; set; } = "";
        [Required, MinLength(6)] public string NewPassword { get; set; } = "";
        [Required, Compare(nameof(NewPassword))] public string ConfirmNewPassword { get; set; } = "";
    }
}
