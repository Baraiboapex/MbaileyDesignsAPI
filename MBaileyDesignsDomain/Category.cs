using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            id:75965689,
            title:"Soft-Skills Growth"
        }
     */
    /// </summary>
    public class Category
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("title")]
        public string? Title { get; set; }
        [Category("is_deleted")]
        public bool? IsDeleted { get; set; }
    }
}
