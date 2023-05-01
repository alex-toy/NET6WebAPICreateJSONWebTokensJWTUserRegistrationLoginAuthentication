using System.ComponentModel.DataAnnotations;

namespace FormulaOneApp.DTOs
{
    public class TokenRequest
    {
        [Required]
        public string Value { get; set; }

        [Required]
        public string RefreshTokenValue { get; set; }
    }
}
