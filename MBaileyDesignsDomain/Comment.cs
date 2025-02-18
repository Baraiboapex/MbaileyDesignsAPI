using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    /// <summary>
    /// json object:
    /*
        {
            id:66789589,
            datePosted:new Date("07/12/2024 1:31 pm.").toISOString(),
            postId:75306345875869,
            commenter:"matthewpbaileydesigns@gmail.com",
            content:"Hahahahahahaha! Poor 'Ol Gil!"
        }
     */
    /// </summary>
    public class Comment
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("date_posted")]
        public string? DatePosted { get; set; }
        [Column("date_posted_iso", TypeName = "timestamp with time zone")]
        public DateTime DatePostedIso { get; set; }
        [Column("commenter")]
        public string? Commenter {  get; set; }
        [Column("content")]
        public string? Content { get; set; }
        [Column("is_deleted")]
        public bool? IsDeleted { get; set; }
    }
}
