using System.ComponentModel.DataAnnotations;

namespace FormulaOneApp.DTOs
{
    public class UserRegistrationRequestDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
