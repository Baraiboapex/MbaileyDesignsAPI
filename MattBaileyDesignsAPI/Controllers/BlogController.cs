using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Controllers.InboundDTO.BlogCategoryController;
using MattBaileyDesignsAPI.Controllers.InboundDTO.BlogController;
using MattBaileyDesignsAPI.Controllers.InboundDTO.ProjectsCategoryController;
using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MattBaileyDesignsAPI.DbRepository.PostGresDbRepository;
using MattBaileyDesignsAPI.Services.Auth.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService.JWTClaimValidators;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using MBaileyDesignsDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Security.Claims;

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {

        private readonly IDBRepository<BlogPost> _db;
        private readonly IDBRepository<BlogCategory> _dbBlogCategories;
        private readonly IDBRepository<SearchableBlogPostField> _dbSearchFields;
        private readonly IEmailerService _emailerService;
        private readonly IControllerErrorHandlingHelper _controllerErrorHandlingHelper;
        private readonly ITokenService _tokenService;

        public BlogController(
            IDBRepository<BlogPost> postgresDataContext,
            IDBRepository<BlogCategory> dbBlogCategories,
            IDBRepository<SearchableBlogPostField> searchableFields,
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
            _dbBlogCategories = dbBlogCategories;
        }

        [AllowAnonymous]
        [HttpGet("/getLatestBlogPosts/{amountToSelect}")]
        public async Task<ActionResult<OutboundDTO>> GetLatestBlogPosts(int amountToSelect)
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
        [HttpGet("/searchPosts")]
        public async Task<ActionResult<OutboundDTO>> SearchPosts([FromQuery] string searchText)
        {
            try
            {
                var foundItems = await _db.GetAllFromDBFromSearchQuery(
                    PostGresFunctionNames.SEARCH_BY_TEXT_STORED_PROC_NAME,
                    new Dictionary<string, object>()
                    {
                        ["table_name"]=PostGresTableNames.BLOG_POSTS_TABLE,
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
        [HttpPost("/searchPostsByFilters")]
        public async Task<ActionResult<OutboundDTO>> SearchPostsByFilters([FromBody] Dictionary<string, object> blogPostFilterContents)
        {
            try
            {
                var qParamsSet = new Dictionary<string, object>() {
                    ["table_name"] = PostGresTableNames.BLOG_POSTS_TABLE
                };

                foreach (var item in blogPostFilterContents) {
                    qParamsSet[item.Key] = item.Value.ToString();
                }

                var items = qParamsSet;

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
        [HttpGet("/getPostFieldValues")]
        public async Task<ActionResult<OutboundDTO>> GetPostFieldValues([FromQuery] string fieldName)
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
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("/getSingleBlogPost")]
        public async Task<ActionResult<OutboundDTO>> GetSingleBlogPost([FromQuery] int id)
        {
            try
            {
                var dbItem = await _db.GetSingleFromDb(id);

                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = dbItem,
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                await _controllerErrorHandlingHelper.HandleControllerErrors(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("/getSearchableBlogPostFields")]
        public async Task<ActionResult<OutboundDTO>> GetSearchableFields()
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
        [HttpPost("/addBlogPost")]
        public async Task<ActionResult<OutboundDTO>> AddBlogPostt([FromBody] BlogAdd blogPost)
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
                        var newBlogPost = new BlogPost()
                        {
                            Content = blogPost.Content,
                            Title = blogPost.Title,
                            DatePosted = DateTime.UtcNow.AddDays(1).ToString("MM/dd/yyyy"),
                            DatePostedIso = DateTime.UtcNow,
                            IsDeleted = false
                        };

                        await _db.WriteToDb(newBlogPost);

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
        [HttpPut("/editBlogPost")]
        public async Task<ActionResult<OutboundDTO>> EditBlogPost([FromBody] BlogEdit blogPost)
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
                        var itemToEdit = await _db.GetSingleFromDb(blogPost.Id);

                        itemToEdit.Title = blogPost.Title;
                        itemToEdit.Content = blogPost.Content;

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
        [HttpDelete("/removeBlogPost/{id}")]
        public async Task<ActionResult<OutboundDTO>> RemoveBlogPost(int id)
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
        [HttpPut("/addBlogCategory")]
        public async Task<ActionResult<OutboundDTO>> AddBlogCategory([FromBody] BlogCategoryDTO blogCategoryDTO)
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
                        var blog = await _db.GetContext().BlogPosts
                            .Include(p => p.PostCategories)
                            .FirstOrDefaultAsync(p => p.Id == blogCategoryDTO.BlogPostId);

                        var blogCategory = await _dbBlogCategories.GetContext().BlogCategories.FirstOrDefaultAsync(pc => pc.Id == blogCategoryDTO.Id);

                        if (blog == null || blogCategory == null)
                        {
                            return NotFound(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Blog Post or Blog Category not found",
                                ["success"] = false
                            }));
                        }

                        var doesNotAlreadyHasProject = blog.PostCategories
                            .Any(pc => pc.Id == blogCategoryDTO.Id);

                        if (!doesNotAlreadyHasProject)
                        {
                            blog.PostCategories.Add(blogCategory);

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
        [HttpPut("/removeBlogCategory")]
        public async Task<ActionResult<OutboundDTO>> RemoveBlogCategory([FromBody] BlogCategoryDTO blogCategoryDTO)
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
                        var blog = await _db.GetContext().BlogPosts
                            .Include(p => p.PostCategories)
                            .FirstOrDefaultAsync(p => p.Id == blogCategoryDTO.BlogPostId);

                        var blogCategory = await _dbBlogCategories.GetContext().BlogCategories.FirstOrDefaultAsync(pc => pc.Id == blogCategoryDTO.Id);

                        if (blog == null || blogCategory == null)
                        {
                            return NotFound(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Blog Post or Blog Category not found",
                                ["success"] = false
                            }));
                        }

                        var doesAlreadyHasProject = blog.PostCategories
                            .Any(pc => pc.Id == blogCategoryDTO.Id);

                        if (doesAlreadyHasProject)
                        {
                            blog.PostCategories.Remove(blogCategory);

                            await _db.GetContext().SaveChangesAsync();

                            return Ok(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Blog category removed!",
                                ["success"] = true
                            }));
                        }
                        else
                        {
                            return BadRequest(new OutboundDTO(new Dictionary<string, object>
                            {
                                ["message"] = "Blog category not found",
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
        [HttpPost("/requestAddPostComment/{id}")]
        public async Task<ActionResult<OutboundDTO>> RequestAddPostComment(int id, [FromBody] Dictionary<string, object> commentData)
        {
            try
            {
                var dbItem = await _db.GetSingleFromDb(id);
                
                if(dbItem != null)
                {
                    var recipientEmailAddress = commentData["recipientEmail"];
                    var recipientCommentTitle = commentData["title"];
                    var recipientCommentContent = commentData["content"];

                    if (
                        recipientEmailAddress != null &&
                        recipientCommentTitle != null &&
                        recipientCommentContent != null
                    ) {
                        var blogPost = dbItem;
                        var comment = new OutboundDTO(new Dictionary<string, object>()
                        {
                            ["recipientEmail"] = recipientEmailAddress,
                            ["title"] = recipientCommentTitle,
                            ["content"] = recipientCommentContent + " \n From :" + recipientEmailAddress
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

        [Authorize]
        [HttpPost("/addPostComment/{id}")]
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
                            var comment = new BlogComment()
                            {
                                DatePosted = DateTime.Now.ToString("MM/dd/yyyy"),
                                DatePostedIso = DateTime.Now,
                                BlogPostId = id,
                                Commenter = commentData["recipientEmail"]?.ToString(),
                                Content = commentData["recipientCommentContent"]?.ToString(),
                                IsDeleted = false,
                                BlogPost = dbItem
                            };

                            if (dbItem.PostComments != null)
                            {
                                dbItem.PostComments.Add(comment);
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
