using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    /// <summary>
    /*
     {
            id:60875686897790,
            datePosted:new Date("7/12/2024 1:45 pm.").toISOString(),
            ownerImage:"/assets/images/owner-image-2.png",
            title:"Well, Hi There! How Are You?!",
            message:`Hi! My name is Matthew Bailey! I am a professional web designer and developer
            who specializes in front-end development but also enjoys going full-stack to employ my 
            wizardry on those tech stacks as well be it Node.js or C# .NET! (I'm working on 'getting gud'
            with python and php as well! ;) ) Besides my absolute banger skillz with tech, I am also a HUGE 
            meat connoisseur and look forward to one day tasting jamon iberico and culatello di zebillio! 
            (Not that you needed to know that) When I am not coding, I am most likely practicing taekwondo, 
            gaming, or just enjoying some time in the great outdoors! Because my pasty white computer nerd 
            skin needs some vitamin D too, right?`
        }
     */
    /// </summary>
    [Table("about_posts")]
    public class AboutPost
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("date_posted")]
        public string? DatePosted { get; set; }
        [Column("date_posted_iso", TypeName = "timestamp with time zone")]
        public DateTime DatePostedIso { get; set; }
        [Column("owner_image")]
        public string? OwnerImage { get; set; }
        [Column("title")]
        public string? Title { get; set; }
        [Column("message")]
        public string? Message { get; set; }
        [Column("is_deleted")]
        public bool? IsDeleted { get; set; }
    }
}
