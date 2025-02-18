using MattBaileyDesignsAPI.Controllers;
using MBaileyDesignsDomain;

namespace MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorLoggers
{
    public interface IErrorLoggingHandler
    {
        public Task<OutboundDTO> PostError(Error errorToPost);
        public Task<OutboundDTO> SearchErrors(OutboundDTO outboundDTO);
    }
}
