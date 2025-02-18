using MattBaileyDesignsAPI.Controllers;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.Interfaces;
using MBaileyDesignsDomain;
using System.Security.Claims;

namespace MattBaileyDesignsAPI.Services.Auth.Interfaces
{
    public interface ITokenService
    {
        public string CreateNewToken(OutboundDTO user);
        public string CreateNewRefreshToken(OutboundDTO user);
        public ClaimsPrincipal ValidateRefreshToken(string refreshToken);
        public bool ValidateClaims(List<IClaimValidator> claimValidationList);
    }
}
