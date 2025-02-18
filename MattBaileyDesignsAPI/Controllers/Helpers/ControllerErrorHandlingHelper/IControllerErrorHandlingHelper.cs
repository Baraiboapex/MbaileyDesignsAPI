namespace MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper
{
    public interface IControllerErrorHandlingHelper
    {
        public Task HandleControllerErrors(string exception);
    }
}
