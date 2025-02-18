using MattBaileyDesignsAPI.Controllers;
using MattBaileyDesignsAPI.Services.Auth.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.Interfaces;
using MBaileyDesignsDomain;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MattBaileyDesignsAPI.Services.Auth.JWTTokenService
{
    public class JWTTokenService : ITokenService
    {
        private IConfiguration _config;

        public JWTTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateNewToken(OutboundDTO user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, (string)user.currentDtoValue["Email"]),
                new Claim(ClaimTypes.NameIdentifier, (string)user.currentDtoValue["Email"]),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var tokenKey = Encoding.UTF8.GetBytes(_config["Jwt:TokenKey"]);
            var enKey = new SymmetricSecurityKey(tokenKey);
            var keyCreds = new SigningCredentials(enKey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"], // Ensure Audience is set here
                SigningCredentials = keyCreds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDesc);

            return tokenHandler.WriteToken(token);
        }

        public string CreateNewRefreshToken(OutboundDTO user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, (string)user.currentDtoValue["Email"]),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var tokenKey = Encoding.UTF8.GetBytes(_config["Jwt:TokenKey"]);
            var enKey = new SymmetricSecurityKey(tokenKey);
            var keyCreds = new SigningCredentials(enKey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"], // Ensure Audience is set here
                SigningCredentials = keyCreds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDesc);

            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var validationKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:TokenKey"]));
                var tokenValidationParams = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = validationKey,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    RequireSignedTokens = true, // Ensure tokens are signed
                    ValidateIssuerSigningKey = true // Validate signing key without expecting `kid`
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var validatedPrinciple = tokenHandler.ValidateToken(refreshToken, tokenValidationParams, out var newToken);

                if (newToken is JwtSecurityToken && validatedPrinciple != null)
                {
                    return validatedPrinciple;
                }

                throw new SecurityTokenException("Invalid token");
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token");
            }
        }

        public bool ValidateClaims(List<IClaimValidator> claimValidationList)
        {
            if(claimValidationList != null)
            {
                int validatedClaims = 0;

                foreach (var claimValidation in claimValidationList)
                {
                    if (claimValidation.ValidateClaim()) 
                    {
                        validatedClaims++;
                    }
                }

                return validatedClaims >= claimValidationList.Count;
            }

            return false;
        }
    }
}
