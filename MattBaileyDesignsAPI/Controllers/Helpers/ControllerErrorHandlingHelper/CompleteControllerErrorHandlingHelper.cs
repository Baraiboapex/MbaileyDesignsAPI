using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorEmailSender;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorLoggers;
using MBaileyDesignsDomain;

namespace MattBaileyDesignsAPI.Controllers.Helpers
{
    public class CompleteControllerErrorHandlingHelper: ErrorObjectBuilder, IControllerErrorHandlingHelper
    {
        private IErrorEmailSender _errorEmailSender;
        private IErrorLoggingHandler _errorLoggingHandler;
        private IConfiguration _config;

        public CompleteControllerErrorHandlingHelper(
            IErrorEmailSender errorEmailSender, 
            IErrorLoggingHandler errorLoggingHandler,
            IConfiguration config
        ) {
            _errorEmailSender = errorEmailSender;
            _errorLoggingHandler = errorLoggingHandler;
            _config = config;
        }

        public async Task HandleControllerErrors(string exception)
        {
            try
            {
                var errorToLog = CreateErrorObject(exception);
                var adminEmailAddress = _config["EmailSettings:AdminEmailAddress"]?.ToString();

                if (!string.IsNullOrEmpty(adminEmailAddress))
                {
                    var errorEmailData = new OutboundDTO(new Dictionary<string, object>()
                    {
                        ["recipientEmail"] = _config["EmailSettings:AdminEmailAddress"],
                        ["title"] = "Error Caught",
                        ["content"] = "Caught error message:" + exception,
                    });

                    await _errorEmailSender.SendErrorEmail(errorEmailData);
                    await _errorLoggingHandler.PostError(errorToLog);
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
