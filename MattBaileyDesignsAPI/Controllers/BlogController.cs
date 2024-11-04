using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MBaileyDesignsDomain;
using MBaileyDesignsDomain.Helpers;
using MbaileyDesignsPersistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private ConfigHelper _configHelper;
        private readonly IDBRepository<BlogPost> _db;

        public BlogController(
             ConfigHelper configHelper,
            IDBRepository<BlogPost> postgresDataContext
        )
        {
            _configHelper = configHelper;
            _db = postgresDataContext;
        }

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

        // GET api/<BlogController>/5
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

        // POST api/<BlogController>
        [HttpPost]
        public async Task<OutboundDTO> Post([FromBody] BlogPost blogPost)
        {
            try
            {
                await _db.WriteToDb(blogPost);

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

        // PUT api/<BlogController>/5
        [HttpPut("{id}")]
        public async Task<OutboundDTO> Put([FromBody] BlogPost blogPost)
        {
            try
            {
                await _db.EditOnDb(blogPost);

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

        // DELETE api/<BlogController>/5
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
