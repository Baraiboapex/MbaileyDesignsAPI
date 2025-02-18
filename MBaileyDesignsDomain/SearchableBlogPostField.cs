using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    [Table("searchable_blog_post_fields")]
    public class SearchableBlogPostField
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("blog_post_field_name")]
        public string? BlogPostFieldName { get; set; }
    }
}
