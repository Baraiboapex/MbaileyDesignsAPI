using MBaileyDesignsDomain;

namespace MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper
{
    public class ErrorObjectBuilder
    {
        protected Error CreateErrorObject(string message)
        {
            var error = new Error()
            {
                Message = message,
                DatePosted = DateTime.UtcNow
            };

            return error;
        }
    }
}
