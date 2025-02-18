using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Controllers.InboundDTO.AboutController;
using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.JWTClaimValidators;
using MBaileyDesignsDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {

        private readonly IDBRepository<AboutPost> _db;
        private readonly IControllerErrorHandlingHelper _controllerErrorHandlingHelper;
        private readonly ITokenService _tokenService;

        public AboutController(
            IDBRepository<AboutPost> postgresDataRepo,
            IControllerErrorHandlingHelper controllerErrorHandlingHelper,
            ITokenService tokenService
        ) 
        {
            _db = postgresDataRepo;
            _controllerErrorHandlingHelper = controllerErrorHandlingHelper;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpGet("/getLatestAboutInfo/{amountToSelect}")]
        public async Task<ActionResult<OutboundDTO>> GetLatestAboutInfo(int amountToSelect)
        {
            try
            {
                var dbItems = await _db.GetAllFromDb();
                var sortedItems = dbItems.ToList().OrderByDescending(item => item.DatePosted);
                var topAmountOfItems = sortedItems.Take(amountToSelect).ToList();

                return Ok(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = topAmountOfItems,
                    ["success"] = true
                }));
            }
            catch(Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                }));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OutboundDTO>> Post([FromBody] AboutAdd aboutPost)
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
                    var newAboutPost = new AboutPost()
                    {
                        Message = aboutPost.Message,
                        Title = aboutPost.Title,
                        DatePosted = DateTime.UtcNow.AddDays(1).ToString("MM/dd/yyyy"),
                        DatePostedIso = DateTime.UtcNow,
                        IsDeleted = false
                    }; 

                    await _db.WriteToDb(newAboutPost);

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
            catch(Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not add post",
                    ["success"] = false
                }));
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult<OutboundDTO>> Put([FromBody] AboutEdit aboutPost)
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
                    var itemToEdit = await _db.GetSingleFromDb(aboutPost.Id);

                    itemToEdit.Title = aboutPost.Title;
                    itemToEdit.Message = aboutPost.Message;

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
            catch(Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not edit post",
                    ["success"] = false
                }));
            }
            
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<OutboundDTO>> Delete(int id)
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
                    var dbItem = await _db.GetSingleFromDb(id);

                    dbItem.IsDeleted = true;

                    await _db.DeleteOnDb(dbItem, useHardDelete: false);

                    return Ok(new OutboundDTO(new Dictionary<string, object>
                    {
                        ["message"] = "Post deleted!",
                        ["success"] = true
                    }));
                }
                else
                {
                    return Unauthorized("User is not an admin!");
                }
            }
            catch(Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not delete post",
                    ["success"] = false
                }));
            }
        }
    }
}
