using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Controllers.InboundDTO.BlogCategoryController;
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
    public class BlogCategoryController : ControllerBase
    {
        private readonly IDBRepository<BlogCategory> _db;
        private readonly IControllerErrorHandlingHelper _controllerErrorHandlingHelper;
        private readonly ITokenService _tokenService;

        public BlogCategoryController(
            IDBRepository<BlogCategory> postgresDataContext,
            IControllerErrorHandlingHelper controllerErrorHandlingHelper,
            ITokenService tokenService
        )
        {
            _db = postgresDataContext;
            _controllerErrorHandlingHelper = controllerErrorHandlingHelper;
            _tokenService = tokenService;
        }

        [Authorize]
        [HttpPost("/addBlogCategory")]
        public async Task<ActionResult<OutboundDTO>> AddBlogCategory([FromBody] BlogCategoryAdd blogCategory)
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
                        var newCategory = new BlogCategory()
                        {
                            Title = blogCategory.Title
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
        [HttpPut("/editBlogCategory")]
        public async Task<ActionResult<OutboundDTO>> EditBlogCategory([FromBody] BlogCategoryEdit blogCategory)
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
                        var itemToEdit = await _db.GetSingleFromDb(blogCategory.Id);

                        itemToEdit.Title = blogCategory.Title;

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
