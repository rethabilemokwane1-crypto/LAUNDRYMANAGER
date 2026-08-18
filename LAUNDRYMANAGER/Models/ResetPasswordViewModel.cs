using System.ComponentModel.DataAnnotations;

namespace LaundryManager.Models
{
    public class ResetPasswordViewModel
    {
        // Carried forward silently (hidden field) so we know which account to update.
        [Required]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter a new password.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[!@#$%^&*(),.?"":{}|<>_\-+=]).*$",
            ErrorMessage = "Password must contain at least one number and one symbol.")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your new password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}