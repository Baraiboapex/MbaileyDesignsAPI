
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorEmailSender;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorLoggers;

namespace MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper
{
    public class LoggingControllerErrorHandlingHelper : ErrorObjectBuilder, IControllerErrorHandlingHelper
    {
        private IErrorLoggingHandler _errorLoggingHandler;
        private IErrorEmailSender _errorEmailSender;
        private IConfiguration _config;

        public LoggingControllerErrorHandlingHelper(
            IErrorLoggingHandler errorLoggingHandler,
            IErrorEmailSender errorEmailSender,
            IConfiguration config
        )
        { 
            _errorLoggingHandler = errorLoggingHandler;
            _errorEmailSender = errorEmailSender;
            _config = config;
        }

        public async Task HandleControllerErrors(string exception)
        {
            try
            {
                var errorToLog = CreateErrorObject(exception);
                await _errorLoggingHandler.PostError(errorToLog);
            }
            catch(Exception ex)
            {
                var adminEmailAddress = _config["EmailSettings:AdminEmailAddress"]?.ToString();
                if (!string.IsNullOrEmpty(adminEmailAddress))
                {
                    var errorEmailData = new OutboundDTO(new Dictionary<string, object>()
                    {
                        ["recipientEmail"] = adminEmailAddress,
                        ["title"] = "Error Caught",
                        ["content"] = "Caught error message:" + exception,
                    });
                    await _errorEmailSender.SendErrorEmail(errorEmailData);
                }
                else
                {
                    var errorEmailData = new OutboundDTO(new Dictionary<string, object>()
                    {
                        ["recipientEmail"] = adminEmailAddress,
                        ["title"] = "You screwed up bad with the error handler, buddy!",
                        ["content"] = "Caught error message:" + exception,
                    });
                    await _errorEmailSender.SendErrorEmail(errorEmailData);
                }
            }
        }
    }
}
