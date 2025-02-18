using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Controllers.InboundDTO.AuthController;
using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MattBaileyDesignsAPI.DbRepository.PostGresDbRepository;
using MattBaileyDesignsAPI.Services.Auth.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.JWTClaimValidators;
using MBaileyDesignsDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDBRepository<User> _db;
        private readonly ITokenService _tokenService;
        private readonly IControllerErrorHandlingHelper _controllerErrorHandlingHelper;

        public AuthController(
            IDBRepository<User> postgresDataContext,
            ITokenService tokenService,
            IControllerErrorHandlingHelper controllerErrorHandlingHelper
        )
        {
            _db = postgresDataContext;
            _tokenService = tokenService;
        }

            _controllerErrorHandlingHelper = controllerErrorHandlingHelper;
        [AllowAnonymous]
        [HttpPost("/login")]
        public async Task<ActionResult<OutboundDTO>> Login([FromBody] LoginDTO loginDto)
        {
            try
            {

                var foundUser = await _db.GetAllFromDBFromSearchQuery(
                    PostGresFunctionNames.SEARCH_BY_TEXT_STORED_PROC_NAME,
                    new Dictionary<string, object>()
                    {
                        ["table_name"] = PostGresTableNames.USERS_TABLE,
                        ["searchText"] = loginDto.Email
                    }
                );

                var retrievedUser = foundUser.SingleOrDefault();

                if(retrievedUser != null)
                {
                    return Ok(new OutboundDTO(new Dictionary<string, object>
                    {
                        ["token"] = _tokenService.CreateNewToken(retrievedUser),
                        ["refreshToken"] = _tokenService.CreateNewRefreshToken(retrievedUser),
                        ["success"] = true
                    }));
                }
                else
                {
                    throw new Exception("User not found");
                }
                
            }
            catch(Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return Unauthorized(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                }));
            }
        }

        [Authorize]
        [HttpPost("/registerNewAdmin")]
        public async Task<ActionResult<OutboundDTO>> RegisterNewAdmin([FromBody] UserRegisterDTO userRegisterDTO)
        {
            try
            {
                var getUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var claimsToCheck = new List<IClaimValidator>()
                {
                    new AdminValidator(User),
                };

                var allClaimsAreValid = _tokenService.ValidateClaims(claimsToCheck);

                if (allClaimsAreValid)
                {
                    //Check token and spit out refresh token.
                    var foundUser = await _db.GetAllFromDBFromSearchQuery(
                        PostGresFunctionNames.SEARCH_BY_TEXT_STORED_PROC_NAME,
                        new Dictionary<string, object>()
                        {
                            ["table_name"] = PostGresTableNames.USERS_TABLE,
                            ["searchText"] = userRegisterDTO.Email
                        }
                    );

                    if (foundUser != null)
                    {
                        var newUser = new User
                        {
                            FirstName = userRegisterDTO.FirstName,
                            LastName = userRegisterDTO.LastName,
                            Email = userRegisterDTO.Email,
                            Password = userRegisterDTO.Password
                        };

                        await _db.WriteToDb(newUser);

                        return Ok(new OutboundDTO(new Dictionary<string, object>
                        {
                            ["message"] = "New user added!",
                            ["success"] = true
                        }));
                    }
                    else
                    {
                        throw new Exception("Admin user could not be found to add new admin");
                    }
                }
                else
                {
                    return Unauthorized("User is not an admin!");
                }
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return Unauthorized(
                    new OutboundDTO(new Dictionary<string, object>
                    {
                        ["message"] = "Could not register user: " + ex,
                        ["success"] = false
                    })
                );
            }
        }

        [Authorize]
        [HttpPost("/refreshToken")]
        public async Task<ActionResult<OutboundDTO>> RefreshToken([FromBody] TokenDTO tokenDTO)
        {
            try
            {
                // Check token and spit out refresh token.
                var principal = _tokenService.ValidateRefreshToken(tokenDTO.RefreshToken);
                var emailClaim = principal.Claims.FirstOrDefault(uc => uc.Type == ClaimTypes.Email); // Use "email" instead of ClaimTypes.Email

                if (emailClaim == null)
                {
                    throw new SecurityTokenException("Email claim is missing.");
                }

                var getUserEmail = emailClaim.Value;

                var foundUser = await _db.GetAllFromDBFromSearchQuery(
                    PostGresFunctionNames.SEARCH_BY_TEXT_STORED_PROC_NAME,
                    new Dictionary<string, object>()
                    {
                        ["table_name"]= PostGresTableNames.USERS_TABLE,
                        ["searchText"] = getUserEmail
                    }
                );

                var retrievedUser = foundUser.SingleOrDefault();

                if (retrievedUser != null)
                {
                    return Ok(new OutboundDTO(new Dictionary<string, object>
                    {
                        ["token"] = _tokenService.CreateNewToken(retrievedUser),
                        ["refreshToken"] = _tokenService.CreateNewRefreshToken(retrievedUser),
                        ["success"] = true
                    }));
                }
                else
                {
                    throw new Exception("User does not exist");
                }
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return Unauthorized(
                    new OutboundDTO(new Dictionary<string, object>
                    {
                        ["message"] = "Could not get item",
                        ["success"] = false
                    })
                );
            }
        }

    }
}
