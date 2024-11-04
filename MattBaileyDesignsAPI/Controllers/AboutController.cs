using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MBaileyDesignsDomain;
using MBaileyDesignsDomain.Helpers;
using MbaileyDesignsPersistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MattBaileyDesignsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {

        private ConfigHelper _configHelper;
        private readonly IDBRepository<AboutPost> _db;

        public AboutController(
            ConfigHelper configHelper,
            IDBRepository<AboutPost> postgresDataRepo
        ) 
        {
            _configHelper = configHelper;
            _db = postgresDataRepo;
        }


        // GET api/<AboutController>/5
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
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not get item",
                    ["success"] = false
                });
            }
        }

        // POST api/<AboutController>
        [HttpPost]
        public async Task<OutboundDTO> Post([FromBody] AboutPost aboutPost)
        {
            try
            {
                await _db.WriteToDb(aboutPost);

                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Post added!",
                    ["success"] = true
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not add post",
                    ["success"] = false
                });
            }
        }

        // PUT api/<AboutController>/5
        [HttpPut("{id}")]
        public async Task<OutboundDTO> Put([FromBody] AboutPost aboutPost)
        {
            try
            {
                await _db.EditOnDb(aboutPost);

                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Post added!",
                    ["success"] = true
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OutboundDTO(new Dictionary<string, object>
                {
                    ["message"] = "Could not edit post",
                    ["success"] = false
                });
            }
            
        }

        // DELETE api/<AboutController>/5
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
            catch(Exception ex)
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
