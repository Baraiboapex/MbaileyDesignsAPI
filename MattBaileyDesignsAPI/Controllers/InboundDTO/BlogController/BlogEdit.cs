using MattBaileyDesignsAPI.Controllers.InboundDTO.BlogCategoryController;
using MBaileyDesignsDomain;

namespace MattBaileyDesignsAPI.Controllers.InboundDTO.BlogController
{
    public class BlogEdit
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
    }
}
