using System.ComponentModel.DataAnnotations;

namespace LaundryManager.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your WSU email address.")]
        [RegularExpression(@"^[0-9]{9}@mywsu\.ac\.za$", ErrorMessage = "Email must be exactly 9 digits followed by @mywsu.ac.za")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}