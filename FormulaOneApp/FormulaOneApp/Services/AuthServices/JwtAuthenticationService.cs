﻿using FormulaOneApp.Configurations;
using FormulaOneApp.Data;
using FormulaOneApp.DTOs;
using FormulaOneApp.Models.AuthModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FormulaOneApp.Services.AuthServices
{
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _appDbContext;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly RoleManager<IdentityRole> _roleManager;

        public JwtAuthenticationService(IOptionsMonitor<JwtConfig> optionsMonitor, UserManager<IdentityUser> userManager, AppDbContext apiDbContext, TokenValidationParameters tokenValidationParameters, RoleManager<IdentityRole> roleManager)
        {
            _jwtConfig = optionsMonitor.CurrentValue;
            _userManager = userManager;
            _appDbContext = apiDbContext;
            _tokenValidationParameters = tokenValidationParameters;
            _roleManager = roleManager;
        }

        public async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
            byte[] secret = Encoding.UTF8.GetBytes(_jwtConfig.Secret);
            var key = new SymmetricSecurityKey(secret);
            var claims = await GetAllValidClaims(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = jwtTokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = jwtTokenHandler.WriteToken(token);
            RefreshToken refreshToken = GenerateRefreshToken(user, jwtTokenHandler, token, jwtToken);

            await SaveRefreshTokenToDb(refreshToken);

            var authResult = new AuthResult()
            {
                Token = jwtToken,
                IsSuccess = true,
                RefreshToken = refreshToken.Value
            };

            return authResult;
        }

        private async Task SaveRefreshTokenToDb(RefreshToken refreshToken)
        {
            await _appDbContext.RefreshTokens.AddAsync(refreshToken);
            await _appDbContext.SaveChangesAsync();
        }

        private RefreshToken GenerateRefreshToken(IdentityUser user, JwtSecurityTokenHandler jwtTokenHandler, SecurityToken token, string jwtToken)
        {
            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame),
                Value = GenerateRandomString(23)
            };
            return refreshToken;
        }

        private string GenerateRandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(x => x[random.Next(x.Length)]).ToArray());
        }

        public async Task<bool> IsAuthenticated(IdentityUser existingUser, UserLoginRequest login)
        {
            bool isAuthenticated = await _userManager.CheckPasswordAsync(existingUser, login.Password);
            return isAuthenticated;
        }

        private async Task<List<Claim>> GetAllValidClaims(IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim("Id", user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            IList<string> userRoles = await _userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                IdentityRole identityRole = await _roleManager.FindByNameAsync(userRole);
                if (identityRole == null) continue;

                claims.Add(new Claim(ClaimTypes.Role, userRole));
                IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(identityRole);
                claims.AddRange(roleClaims);
            }

            return claims;
        }

        public async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                ClaimsPrincipal? tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Value, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    bool result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCulture);
                    if (result == false) return null;
                }

                string value = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value;
                var utcExpiryDate = long.Parse(value);
                DateTime expiryDate = UnixTimeStampToDateTime(utcExpiryDate);
                if (expiryDate > DateTime.Now) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token has not yet expired"
                    }
                };

                RefreshToken? storedToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Value == tokenRequest.RefreshTokenValue);
                if (storedToken == null) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token does not exist"
                    }
                };

                if (storedToken.IsUsed) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token has been used"
                    }
                };

                if (storedToken.IsRevoked) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token has been reworked"
                    }
                };

                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token doesn't match"
                    }
                };

                storedToken.IsUsed = true;
                await UpdateStoredToken(storedToken);

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        $"server error : {ex.Message}"
                    }
                };
            }
        }

        private async Task UpdateStoredToken(RefreshToken? storedToken)
        {
            _appDbContext.RefreshTokens.Update(storedToken);
            await _appDbContext.SaveChangesAsync();
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTimeVal;
        }
    }
}
