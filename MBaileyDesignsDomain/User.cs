using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain
{
    [Table("users")]
    public class User : IdentityUser<int>
    {
        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("is_deleted")]
        public bool? IsDeleted { get; set; }

        [Column("refresh_token")]
        public string? RefreshToken { get; set; }
    }

}
