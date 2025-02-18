using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    [Table("project_categories")]
    public class ProjectCategory : Category
    {
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        [Column("project_id")]
        public int ProjectId { get; set; }
    }
}
