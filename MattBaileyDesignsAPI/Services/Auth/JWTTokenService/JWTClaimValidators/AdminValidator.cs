using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.Interfaces;
using MBaileyDesignsDomain;
using System.Security.Claims;

namespace MattBaileyDesignsAPI.Services.Auth.JWTTokenService.JWTClaimValidators
{
    public class AdminValidator: IDisposable, IClaimValidator
    {
        private ClaimsPrincipal _claim;

        public AdminValidator(ClaimsPrincipal claim) {
            _claim = claim;    
        }

        public bool ValidateClaim()
        {
            if (_claim != null)
            {
                var getUserId = _claim.FindFirst(ClaimTypes.NameIdentifier);

                if (getUserId != null)
                {
                    var userId = getUserId.Value;

                    if (userId != null) {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public void Dispose()
        {
            return;  
        }
    }
}
