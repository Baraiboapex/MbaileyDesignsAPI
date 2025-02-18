using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    [Table("errors")]
    public class Error
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("message")]
        public string? Message { get; set; }
        [Column("date_posted")]
        public DateTime DatePosted { get; set; }
    }
}
