using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Controllers.InboundDTO.BlogController;
using MattBaileyDesignsAPI.Controllers.InboundDTO.ProjectsCategoryController;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.JWTClaimValidators;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using MBaileyDesignsDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectCategoryController : ControllerBase
    {
        private readonly IDBRepository<ProjectCategory> _db;
        private readonly IControllerErrorHandlingHelper _controllerErrorHandlingHelper;
        private readonly ITokenService _tokenService;

        public ProjectCategoryController(
            IDBRepository<ProjectCategory> postgresDataContext,
            IControllerErrorHandlingHelper controllerErrorHandlingHelper,
            ITokenService tokenService
        )
        {
            _db = postgresDataContext;
            _controllerErrorHandlingHelper = controllerErrorHandlingHelper;
            _tokenService = tokenService;
        }

        [Authorize]
        [HttpPost("/addProjectCategory")]
        public async Task<ActionResult<OutboundDTO>> AddProjectCategory([FromBody] ProjectCategoryAdd projectCategory)
        {
            try
            {
                var getUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (getUserIdClaim != null)
                {
                    var claimsToCheck = new List<IClaimValidator>()
                    {
                        new AdminValidator(User),
                    };

                    var allClaimsAreValid = _tokenService.ValidateClaims(claimsToCheck);

                    if (allClaimsAreValid)
                    {
                        var newCategory = new ProjectCategory()
                        {
                            Title = projectCategory.Title
                        };

                        await _db.WriteToDb(newCategory);

                        return Ok(new OutboundDTO(new Dictionary<string, object>
                        {
                            ["message"] = "Category added!",
                            ["success"] = true
                        }));
                    }
                    else
                    {
                        return Unauthorized("User is not an admin!");
                    }
                }
                else
                {
                    return Unauthorized("User does not have relevant claim!");
                }
            }
            catch (Exception error)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(error.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not send comment request",
                    ["success"] = false
                }));
            }
        }

        [Authorize]
        [HttpPut("/editProjectCategory")]
        public async Task<ActionResult<OutboundDTO>> EditProjectCategory([FromBody] ProjectCategoryEdit projectCategory)
        {
            try
            {
                var getUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (getUserIdClaim != null)
                {
                    var claimsToCheck = new List<IClaimValidator>()
                    {
                        new AdminValidator(User),
                    };

                    var allClaimsAreValid = _tokenService.ValidateClaims(claimsToCheck);

                    if (allClaimsAreValid)
                    {
                        var itemToEdit = await _db.GetSingleFromDb(projectCategory.Id);

                        itemToEdit.Title = projectCategory.Title;

                        await _db.EditOnDb(itemToEdit);

                        return Ok(new OutboundDTO(new Dictionary<string, object>
                        {
                            ["message"] = "Post added!",
                            ["success"] = true
                        }));
                    }
                    else
                    {
                        return Unauthorized("User is not an admin!");
                    }
                }
                else
                {
                    return Unauthorized("User does not have relevant claim!");
                }
            }
            catch (Exception error)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(error.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not send comment request",
                    ["success"] = false
                }));
            }
        }
    }
}
