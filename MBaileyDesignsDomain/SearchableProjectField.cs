using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    [Table("searchable_projects_fields")]
    public class SearchableProjectField
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("project_field_name")]
        public string? ProjectFieldName { get; set; }
    }
}
