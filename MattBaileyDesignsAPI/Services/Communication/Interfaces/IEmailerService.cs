using MattBaileyDesignsAPI.Controllers;

namespace MattBaileyDesignsAPI.Services.Communication.Interfaces
{
    public interface IEmailerService
    {
        Task SendEmail(OutboundDTO commentEmailObject);
    }
}
