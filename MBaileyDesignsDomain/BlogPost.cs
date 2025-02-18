using System.ComponentModel.DataAnnotations.Schema;

namespace MBaileyDesignsDomain
{
    /// <summary>
    /// json object:
    /*
        {
            id:75306345875869,
            datePosted:dayjs(new Date("07/12/2024 1:31 pm.").toISOString()).format("MM/DD/YYYY"),
            datePostedIso:new Date("07/12/2024 1:31 pm.").toISOString(),
            title:"The difficulty of marketting yourself and maintaining a positive outlook as an autistic developer when you are unemployed",
            content:`Help`,
            categories:[
                {
                    id:75965689,
                    title:"Soft-Skills Growth"
                }
            ],
            postComments:[
                {
                    id:66789589,
                    datePosted:new Date("07/12/2024 1:31 pm.").toISOString(),
                    postId:75306345875869,
                    commenter:"matthewpbaileydesigns@gmail.com",
                    content:"Hahahahahahaha! Poor 'Ol Gil!"
                }
            ]
        }
     */
    /// </summary>
    [Table("blog_posts")]
    public class BlogPost
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("date_posted")]
        public string? DatePosted { get; set; }
        [Column("date_posted_iso", TypeName = "timestamp with time zone")]
        public DateTime DatePostedIso { get; set; }
        [Column("title")]
        public string? Title { get; set; }
        [Column("content")]
        public string? Content { get; set; }
        [Column("is_deleted")]
        public bool? IsDeleted { get; set; }
        public ICollection<BlogCategory>? PostCategories { get; set; } = new List<BlogCategory>();
        public ICollection<BlogComment>? PostComments { get; set; } = new List<BlogComment>();
    }
}
