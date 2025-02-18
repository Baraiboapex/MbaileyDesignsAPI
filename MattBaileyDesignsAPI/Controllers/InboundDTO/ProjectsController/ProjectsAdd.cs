using MBaileyDesignsDomain;

namespace MattBaileyDesignsAPI.Controllers.InboundDTO.ProjectsController
{
    public class ProjectsAdd
    {
        public string Title { get; set; }
        public string AboutProject { get; set; }
        public string ProjectLink { get; set; }
        public string ProjectImage { get; set; }
        public ICollection<ProjectCategory> ProjectCategories { get; set; }
    }
}
