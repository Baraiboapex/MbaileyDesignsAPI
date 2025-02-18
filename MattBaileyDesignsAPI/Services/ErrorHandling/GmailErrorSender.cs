using MattBaileyDesignsAPI.Controllers;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorEmailSender;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorLoggers;
using MBaileyDesignsDomain;

namespace MattBaileyDesignsAPI.Services.ErrorHandling
{
    public class GmailErrorSender : IErrorEmailSender
    {
        private IEmailerService _emailerService;
        private IErrorLoggingHandler _errorLoggingHandler;

        public GmailErrorSender(IEmailerService emailerService, IErrorLoggingHandler errorLoggingHandler) 
        { 
            _emailerService = emailerService;
            _errorLoggingHandler = errorLoggingHandler;
        }

        public async Task<OutboundDTO> SendErrorEmail(OutboundDTO errorToPost)
        {
            try
            {
                await _emailerService.SendEmail(errorToPost);
                return new OutboundDTO(new Dictionary<string, object>()
                {
                    ["message"] = "Sent error email",
                    ["success"] = true
                });
            }
            catch (Exception ex)
            {
                var error = new Error()
                {
                    Message = ex.Message,
                    DatePosted = DateTime.Now
                };

                await _errorLoggingHandler.PostError(error);

                return null;
            }
        }
    }
}
