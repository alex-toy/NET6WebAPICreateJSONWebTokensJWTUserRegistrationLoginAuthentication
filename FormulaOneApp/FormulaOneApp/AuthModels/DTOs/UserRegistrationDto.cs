﻿using System.ComponentModel.DataAnnotations;

namespace FormulaOneApp.AuthModels.DTOs
{
    public class UserRegistrationDto
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
