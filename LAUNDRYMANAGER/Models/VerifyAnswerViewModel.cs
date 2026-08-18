using System.ComponentModel.DataAnnotations;

namespace LaundryManager.Models
{
    public class VerifyAnswerViewModel
    {
        // Carried forward silently from the previous page (hidden field) —
        // we need it to know WHICH account we're verifying the answer for.
        [Required]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please answer the security question.")]
        public string SecurityAnswer { get; set; } = "";
    }
}
