using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    [Table("blog_comments")]
    public class BlogComment : Comment
    {
        public BlogPost? BlogPost { get; set; }
        [Column("blog_post_id")]
        public int BlogPostId { get; set; }
    }
}
