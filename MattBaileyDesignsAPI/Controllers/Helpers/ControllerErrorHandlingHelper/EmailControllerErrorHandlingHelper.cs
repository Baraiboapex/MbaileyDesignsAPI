
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorEmailSender;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorLoggers;

namespace MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper
{
    public class EmailControllerErrorHandlingHelper : ErrorObjectBuilder, IControllerErrorHandlingHelper
    {
        private IErrorLoggingHandler _errorLoggingHandler;
        private IErrorEmailSender _errorEmailSender;
        private IConfiguration _config;

        public EmailControllerErrorHandlingHelper(
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
                var adminEmailAddress = _config["EmailSettings:AdminEmailAddress"]?.ToString();
                if (!string.IsNullOrEmpty(adminEmailAddress))
                {
                    var errorEmailData = new OutboundDTO(new Dictionary<string, object>()
                    {
                        ["recipientEmail"] = "",
                        ["title"] = "Error Caught",
                        ["content"] = "Caught error message:" + exception,
                    });
                    await _errorEmailSender.SendErrorEmail(errorEmailData);
                }
                else
                {
                    throw new Exception("Admin email not found in complete error handler for controller");
                }
            }
            catch (Exception ex)
            {
                var errorToLog = CreateErrorObject(ex.Message);
                await _errorLoggingHandler.PostError(errorToLog);
            }
        }
    }
}
