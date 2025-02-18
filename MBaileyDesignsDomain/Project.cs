using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    /// <summary>
    /// json object
    /*
        {
            id:867056670,
            datePostedIso:new Date("07/12/2024 1:31 pm.").toISOString(),
            datePosted:dayjs(new Date("07/12/2024 1:31 pm.").toISOString()).format("MM/DD/YYYY"),
            title:"GNBC Japanese Class Tools Suite",
            projectImage:"/assets/images/japanese-class-suite.png",
            aboutProject:"For my church, I built a handful of tools for our japanese class using node.js, google apps scripts, and Vue.js!",
            projectLink:null,
            categories:[
                {
                    id:75965689,
                    title:"Not-For-Profit Projects"
                }
            ],
            projectComments:[
                {
                    id:66789589,
                    datePosted:new Date("07/12/2024 1:31 pm.").toISOString(),
                    projectId:867056670,
                    commenter:"matthewpbaileydesigns@gmail.com",
                    content:"Comment on dis"
                }
            ]
        }
     */
    /// </summary>
    [Table("projects")]
    public class Project
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("date_posted")]
        public string? DatePosted { get; set; }
        [Column("date_posted_iso", TypeName = "timestamp with time zone")]
        public DateTime DatePostedIso { get; set; }
        [Column("title")]
        public string? Title { get; set; }
        [Column("project_image")]
        public string? ProjectImage { get; set; }
        [Column("about_project")]
        public string? AboutProject { get; set; }
        [Column("project_link")]
        public string? ProjectLink { get; set; }
        [Column("is_deleted")]
        public bool? IsDeleted { get; set; }
        public ICollection<ProjectComment>? ProjectComments { get; set; } = new List<ProjectComment>();
        public ICollection<ProjectCategory>? ProjectCategories { get; set; } = new List<ProjectCategory>();
    }
}
