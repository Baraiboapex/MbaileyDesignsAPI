using MattBaileyDesignsAPI.Controllers;

namespace MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorEmailSender
{
    public interface IErrorEmailSender
    {
        public Task<OutboundDTO> SendErrorEmail(OutboundDTO errorToPost);
    }
}
