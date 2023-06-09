﻿using FormulaOneApp.DTOs;
using FormulaOneApp.Models.AuthModels;
using Microsoft.AspNetCore.Identity;

namespace FormulaOneApp.Services.AuthServices
{
    public interface IJwtAuthenticationService
    {
        Task<AuthResult> GenerateJwtToken(IdentityUser user);
        Task<bool> IsAuthenticated(IdentityUser existingUser, UserLoginRequest login);
        Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest);
    }
}