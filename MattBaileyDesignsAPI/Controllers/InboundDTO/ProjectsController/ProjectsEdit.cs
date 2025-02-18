using MBaileyDesignsDomain;

namespace MattBaileyDesignsAPI.Controllers.InboundDTO.ProjectsController
{
    public class ProjectsEdit
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AboutProject { get; set; }
        public string ProjectLink { get; set; }
        public string ProjectImage { get; set; }
    }
}
