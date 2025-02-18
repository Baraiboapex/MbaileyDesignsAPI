using MattBaileyDesignsAPI.Controllers.InboundDTO.BlogCategoryController;
using MBaileyDesignsDomain;

namespace MattBaileyDesignsAPI.Controllers.InboundDTO.BlogController
{
    public class BlogAdd
    {
        public string Content { get; set; }
        public string Title { get; set; }
        public ICollection<BlogCategoryDTO> BlogPostCategories { get; set; }
    }
}
