using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    [Table("blog_categories")]
    public class BlogCategory : Category
    {
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
        [Column("blog_post_id")]
        public int BlogPostId { get; set; }
    }
}