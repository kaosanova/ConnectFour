using System.ComponentModel.DataAnnotations;

namespace ConnectFourSpel.Models
{
    public class EditUsernameVm
    {
        [Required, StringLength(40)]
        public string Username { get; set; } = "";
    }
}
