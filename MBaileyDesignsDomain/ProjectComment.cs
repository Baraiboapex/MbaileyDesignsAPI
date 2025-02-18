using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    [Table("project_comments")]
    public class ProjectComment:Comment
    {
        public Project? Project { get; set; }

        [Column("project_id")]
        public int ProjectId { get; set; }
    }
}
