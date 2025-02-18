using MattBaileyDesignsAPI.Controllers;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MattBaileyDesignsAPI.DbRepository.PostGresDbRepository;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorLoggers;
using MBaileyDesignsDomain;
using System.Runtime.InteropServices.JavaScript;

namespace MattBaileyDesignsAPI.Services.ErrorHandling
{
    public class PostGresErrorHandler : IErrorLoggingHandler
    {
        private readonly IDBRepository<Error> _db;

        public PostGresErrorHandler(
            IDBRepository<Error> db
        ) 
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public async Task<OutboundDTO> PostError(Error errorToPost)
        {
            try
            {
                await _db.WriteToDb(errorToPost);

                return new OutboundDTO(new Dictionary<string, object>()
                {
                    ["message"] = "Error posted",
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Could not add new error : " + ex);
            }
        }

        public async Task<OutboundDTO> SearchErrors(OutboundDTO outboundDTO)
        {
            try
            {
                var qparams = outboundDTO.currentDtoValue;
                var qParamsSet = new Dictionary<string, object>();

                foreach (var item in qparams)
                {
                    qParamsSet[item.Key] = item.Value.ToString();
                }

                var foundItems = await _db.GetAllFromDBFromSearchQuery(
                    PostGresFunctionNames.SEARCH_BY_FILTERS_STORED_PROC_NAME,
                    qParamsSet
                );

                return new OutboundDTO(new Dictionary<string, object>()
                {
                    ["message"] = "Error posted",
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Could not find error : " + ex);
            }
        }
    }
}
