using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MBaileyDesignsDomain;
using MBaileyDesignsDomain.Helpers;
using MbaileyDesignsPersistence;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {

        private ConfigHelper _configHelper;
        private readonly IDBRepository<Project> _db;

        public ProjectsController(
            ConfigHelper configHelper,
            IDBRepository<Project> postgresDataContext
        )
        {
            _configHelper = configHelper;
            _db = postgresDataContext;
        }

        // GET: api/<ProjectsController>
        [HttpGet]
        public async Task<OutboundDTO> Get()
        {
            try
            {
                var dbItems = await _db.GetAllFromDb();

                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = dbItems,
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get all items",
                    ["success"] = false
                });
            }
        }

        // GET api/<ProjectsController>/5
        [HttpGet("{id}")]
        public async Task<OutboundDTO> Get(int id)
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
                Console.WriteLine(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                });
            }
        }

        // POST api/<ProjectsController>
        [HttpPost]
        public async Task<OutboundDTO> Post([FromBody] Project project)
        {
            try
            {
                await _db.WriteToDb(project);

                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Post added!",
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not add post",
                    ["success"] = false
                });
            }
        }

        // PUT api/<ProjectsController>/5
        [HttpPut("{id}")]
        public async Task<OutboundDTO> Put([FromBody] Project project)
        {
            try
            {
                await _db.EditOnDb(project);

                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Post added!",
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not edit post",
                    ["success"] = false
                });
            }
        }

        // DELETE api/<ProjectsController>/5
        [HttpDelete("{id}")]
        public async Task<OutboundDTO> Delete(int id)
        {
            try
            {
                var dbItem = await _db.GetSingleFromDb(id);

                dbItem.IsDeleted = true;

                await _db.DeleteOnDb(dbItem, useHardDelete: false);

                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Post deleted!",
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not delete post",
                    ["success"] = false
                });
            }
        }
    }
}
