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
    public class AuthController : ControllerBase
    {
        private ConfigHelper _configHelper;
        private readonly IDBRepository<User> _db;
        public AuthController(
             ConfigHelper configHelper,
            IDBRepository<User> postgresDataContext
        )
        {
            _configHelper = configHelper;
            _db = postgresDataContext;
        }

        // GET api/<AuthController>/5
        [HttpGet("{id}")]
        public async Task<OutboundDTO> Get(int id)
        {
            return new OutboundDTO();
        }

        // POST api/<AuthController>
        [HttpPost]
        public async Task<OutboundDTO> Post([FromBody] string value)
        {
            return new OutboundDTO();
        }

        [HttpPost]
        public async Task<OutboundDTO> Post([FromBody] string value)
        {
            return new OutboundDTO();
        }
    }
}
