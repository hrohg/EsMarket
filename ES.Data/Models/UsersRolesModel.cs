using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ES.Data.Model;

namespace ES.Data.Models
{
    public class UsersRolesModel
    {
        public Guid Id { get; set; }
        public EsUserModel EsUser { get; set; }
        public MemberRolesModel Role { get; set; }

    }
    public class MemberRolesModel
    {
        public long Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
    }
}
