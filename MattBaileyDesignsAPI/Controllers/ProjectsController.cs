﻿using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Controllers.InboundDTO.ProjectsCategoryController;
using MattBaileyDesignsAPI.Controllers.InboundDTO.ProjectsController;
using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MattBaileyDesignsAPI.DbRepository.PostGresDbRepository;
using MattBaileyDesignsAPI.Services.Auth.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.JWTClaimValidators;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using MBaileyDesignsDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {

        private readonly IDBRepository<Project> _db;
        private readonly IDBRepository<ProjectCategory> _dbProjectCategories;
        private readonly IDBRepository<SearchableProjectField> _dbSearchFields;
        private readonly IEmailerService _emailerService;
        private readonly IControllerErrorHandlingHelper _controllerErrorHandlingHelper;
        private readonly ITokenService _tokenService;

        public ProjectsController(
            IDBRepository<Project> postgresDataContext,
            IDBRepository<ProjectCategory> dbProjectCategories,
            IDBRepository<SearchableProjectField> searchableFields,
            IEmailerService emailerService,
            IControllerErrorHandlingHelper controllerErrorHandlingHelper,
            ITokenService tokenService
        )
        {
            _db = postgresDataContext;
            _dbSearchFields = searchableFields;
            _emailerService = emailerService;
            _controllerErrorHandlingHelper = controllerErrorHandlingHelper;
            _tokenService = tokenService;
            _dbProjectCategories = dbProjectCategories;
        }

        [AllowAnonymous]
        [HttpGet("/getLatestProjects/{amountToSelect}")]
        public async Task<ActionResult<OutboundDTO>> GetLatestProjects(int amountToSelect)
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
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                }));
            }
        }

        [AllowAnonymous]
        [HttpGet("/getSingleProjectPost")]
        public async Task<ActionResult<OutboundDTO>> GetSingleProjectPost([FromQuery] int id)
        {
            try
            {
                var dbItem = await _db.GetSingleFromDb(id);

                return Ok(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = dbItem,
                    ["success"] = true
                }));
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                }));
            }
        }

        [AllowAnonymous]
        [HttpGet("/searchProjects")]
        public async Task<ActionResult<OutboundDTO>> SearchProjects([FromQuery] string searchText)
        {
            try
            {
                var foundItems = await _db.GetAllFromDBFromSearchQuery(
                    PostGresFunctionNames.SEARCH_BY_TEXT_STORED_PROC_NAME,
                    new Dictionary<string, object>() {
                        ["table_name"] = PostGresTableNames.BLOG_POSTS_TABLE,
                        ["searchText"] = searchText
                    }
                );

                return Ok(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = foundItems,
                    ["success"] = true
                }));
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                }));
            }
        }

        [AllowAnonymous]
        [HttpPost("/searchProjectsByFilters")]
        public async Task<ActionResult<OutboundDTO>> SearchProjectsByFilters([FromBody] Dictionary<string, object> projectPostFilterContents)
        {
            try
            {
                var qParamsSet = new Dictionary<string, object>() {
                    ["table_name"] = PostGresTableNames.BLOG_POSTS_TABLE
                };

                foreach (var item in projectPostFilterContents)
                {
                    qParamsSet[item.Key] = item.Value.ToString();
                }

                var foundItems = await _db.GetAllFromDBFromSearchQuery(
                    PostGresFunctionNames.SEARCH_BY_FILTERS_STORED_PROC_NAME,
                    qParamsSet
                );

                return Ok(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = foundItems,
                    ["success"] = true
                }));
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                }));
            }
        }

        [AllowAnonymous]
        [HttpGet("/getProjectFieldFieldValues")]
        public async Task<ActionResult<OutboundDTO>> GetProjectFieldValues([FromQuery] string fieldName)
        {
            try
            {
                var retrievedItemValues = _db.GetAllFromDBFromSearchQuery(
                   PostGresFunctionNames.GET_ALL_TABLE_SINGLE_FIELD_VALUES,
                   new Dictionary<string, object>()
                   {
                       ["table_name"] = PostGresTableNames.BLOG_POSTS_TABLE,
                       ["field_name"] = fieldName
                   }
               );

                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = retrievedItemValues,
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                }));
            }
        }

        [AllowAnonymous]
        [HttpGet("/getSearchableProjectFields")]
        public async Task<ActionResult<OutboundDTO>> GetSearchableProjectFields()
        {
            try
            {
                var dbItems = await _dbSearchFields.GetAllFromDb();

                return Ok(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = dbItems,
                    ["success"] = true
                }));
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get items",
                    ["success"] = false
                }));
            }
        }

        [Authorize]
        [HttpPost("/addProject")]
        public async Task<ActionResult<OutboundDTO>> AddProject([FromBody] ProjectsAdd project)
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
                        var newProject = new Project()
                        {
                            Title = project.Title,
                            AboutProject = project.AboutProject,
                            ProjectImage=project.ProjectImage,
                            ProjectLink=project.ProjectLink,
                            DatePosted = DateTime.UtcNow.AddDays(1).ToString("MM/dd/yyyy"),
                            DatePostedIso = DateTime.UtcNow,
                            IsDeleted = false,
                        };

                        var newCategoriesList = _dbProjectCategories.GetAllFromDb();
                        
                        await _db.WriteToDb(newProject);

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
            catch (Exception ex)
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
        [HttpPut("/editProject")]
        public async Task<ActionResult<OutboundDTO>> EditProject([FromBody] ProjectsEdit project)
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
                        var itemToEdit = await _db.GetSingleFromDb(project.Id);

                        itemToEdit.Title = project.Title;
                        itemToEdit.AboutProject = project.AboutProject;
                        itemToEdit.ProjectImage = project.ProjectImage;
                        itemToEdit.ProjectLink = project.ProjectLink;
                        itemToEdit.DatePostedIso = DateTime.UtcNow;

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
            catch (Exception ex)
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
        [HttpDelete("/removeProject/{id}")]
        public async Task<ActionResult<OutboundDTO>> RemoveProject(int id)
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
                else
                {
                    return Unauthorized("User does not have relevant claim!");
                }
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not delete post",
                    ["success"] = false
                }));
            }
        }

        [Authorize]
        [HttpPut("/addProjectCategory")]
        public async Task<ActionResult<OutboundDTO>> AddProjectCategory([FromBody] ProjectCategoryDTO projectCategoryDTO)
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
                        // Load the project and related project categories
                        var project = await _db.GetContext().Projects
                            .Include(p => p.ProjectCategories)
                            .FirstOrDefaultAsync(p => p.Id == projectCategoryDTO.ProjectId);

                        var projectCategory = await _dbProjectCategories.GetContext().ProjectCategories.FirstOrDefaultAsync(pc => pc.Id == projectCategoryDTO.Id);

                        if (project == null || projectCategory == null)
                        {
                            return NotFound(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Project or Project Category not found",
                                ["success"] = false
                            }));
                        }

                        var doesNotAlreadyHasProject = project.ProjectCategories
                            .Any(pc => pc.Id == projectCategoryDTO.Id);

                        if (!doesNotAlreadyHasProject)
                        {
                            project.ProjectCategories.Add(projectCategory);

                            await _db.GetContext().SaveChangesAsync();

                            return Ok(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Project Category Added!",
                                ["success"] = true
                            }));
                        }
                        else
                        {
                            return BadRequest(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Project category already exists",
                                ["success"] = false
                            }));
                        }
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
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not add project category",
                    ["success"] = false
                }));
            }
        }

        [Authorize]
        [HttpPut("/removeProjectCategory")]
        public async Task<ActionResult<OutboundDTO>> RemoveProjectCategory([FromBody] ProjectCategoryDTO projectCategoryDTO)
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
                        // Load the project and related project categories
                        var project = await _db.GetContext().Projects
                            .Include(p => p.ProjectCategories)
                            .FirstOrDefaultAsync(p => p.Id == projectCategoryDTO.ProjectId);

                        var projectCategory = await _dbProjectCategories.GetContext().ProjectCategories.FirstOrDefaultAsync(pc => pc.Id == projectCategoryDTO.Id);

                        if (project == null || projectCategory == null)
                        {
                            return NotFound(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Project or Project Category not found",
                                ["success"] = false
                            }));
                        }

                        var doesAlreadyHasProject = project.ProjectCategories
                            .Any(pc => pc.Id == projectCategoryDTO.Id);

                        if (doesAlreadyHasProject)
                        {
                            project.ProjectCategories.Remove(projectCategory);

                            await _db.GetContext().SaveChangesAsync();

                            return Ok(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Project category removed!",
                                ["success"] = true
                            }));
                        }
                        else
                        {
                            return BadRequest(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Project category not found",
                                ["success"] = false
                            }));
                        }
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
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not add project category",
                    ["success"] = false
                }));
            }
        }

        [AllowAnonymous]
        [HttpPost("/requestAddProjectComment/{id}")]
        public async Task<ActionResult<OutboundDTO>> RequestAddPostComment(int id, [FromBody] Dictionary<string, object> commentData)
        {
            try
            {
                var dbItem = await _db.GetSingleFromDb(id);

                if (dbItem != null)
                {
                    var recipientEmailAddress = commentData["recipientEmail"];
                    var recipientCommentTitle = commentData["title"];
                    var recipientCommentContent = commentData["content"];

                    if (
                        recipientEmailAddress != null &&
                        recipientCommentTitle != null &&
                        recipientCommentContent != null
                    )
                    {
                        var blogPost = dbItem;
                        var comment = new OutboundDTO(new Dictionary<string, object>()
                        {
                            ["recipientEmail"] = recipientEmailAddress,
                            ["title"] = recipientCommentTitle,
                            ["content"] = recipientCommentContent + "\nFrom :" + recipientEmailAddress
                        });

                        await _emailerService.SendEmail(comment);
                    }
                    else
                    {
                        throw new ArgumentNullException("fields cannot be null in the inbound dto");
                    }
                }

                return Ok(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Comment request sent",
                    ["success"] = true
                }));
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not send comment request",
                    ["success"] = false
                }));
            }
        }

        [AllowAnonymous]
        [HttpPost("/addProjectComment/{id}")]
        public async Task<ActionResult<OutboundDTO>> AddComment(int id, [FromBody] Dictionary<string, object> commentData)
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
                        var dbItem = await _db.GetSingleFromDb(id);

                        if (dbItem != null)
                        {
                            var comment = new ProjectComment()
                            {
                                DatePosted = DateTime.Now.ToString("MM/dd/yyyy"),
                                DatePostedIso = DateTime.Now,
                                ProjectId = id,
                                Commenter = commentData["recipientEmail"]?.ToString(),
                                Content = commentData["recipientCommentContent"]?.ToString(),
                                IsDeleted = false,
                                Project = dbItem
                            };

                            if (dbItem.ProjectComments != null)
                            {
                                dbItem.ProjectComments.Add(comment);
                            }

                            await _db.EditOnDb(dbItem);
                        }
                        else
                        {
                            throw new Exception("Post not found");
                        }

                        return Ok(new OutboundDTO(new Dictionary<string, object>
                        {
                            ["message"] = "Comment added",
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
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return BadRequest(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not send comment request",
                    ["success"] = false
                }));
            }
        }
    }
}
