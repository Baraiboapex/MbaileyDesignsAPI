using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Controllers.InboundDTO.ErrorsController;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MattBaileyDesignsAPI.DbRepository.PostGresDbRepository;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorEmailSender;
using MBaileyDesignsDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]/errors")]
    [ApiController]
    public class ErrorsController : ControllerBase
    {
        private IDBRepository<Error> _db;
        private IErrorEmailSender _errorEmailerService;
        private IConfiguration _config;

        public ErrorsController(
            IDBRepository<Error> db, 
            IErrorEmailSender errorEmailerService, 
            IConfiguration config
        ){
            _db = db;
            _errorEmailerService = errorEmailerService;
            _config = config;
        }

        [Authorize]
        [HttpGet("/getErrors")]
        public async Task<ActionResult<OutboundDTO>> GetErrors(int amountToSelect)
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
            catch (Exception error)
            {
                return BadRequest(error);
            }
        }

        [Authorize]
        [HttpPost("/searchErrorsByFilters")]
        public async Task<ActionResult<OutboundDTO>> SearchErrorsByFilters([FromBody] Dictionary<string, object> errorsFilterContents)
        {
            try
            {
                var qParamsSet = new Dictionary<string, object>()
                {
                    ["table_name"] = PostGresTableNames.ERRORS_TABLE
                };

                foreach (var item in errorsFilterContents)
                {
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
            catch (Exception error)
            {
                return BadRequest(error);
            }
        }

        [AllowAnonymous]
        [HttpPost("/addError")]
        public async Task<ActionResult<OutboundDTO>> AddErrorFromFE(ErrorAdd newError)
        {
            try
            {

                var newBlogPost = new Error()
                {
                    Message = newError.Message,
                    DatePosted = DateTime.UtcNow,
                };

                await _db.WriteToDb(newBlogPost);

                return Ok(new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Post added!",
                    ["success"] = true
                }));
            }
            catch (Exception error)
            {
                //recipientEmail
                await _errorEmailerService.SendErrorEmail(new OutboundDTO(new Dictionary<string, object>
                {
                    ["recipientEmail"] = _config["EmailSettings:AdminEmailAddress"],
                    ["title"] = "Error Found - Could Not Add New Error",
                    ["Message"] = @"Error : {error}"
                }));
                return BadRequest(error);
            }
        }
    }
}
