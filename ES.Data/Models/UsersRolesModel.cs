using System;

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
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string Name { get { return string.IsNullOrEmpty(Description) ? RoleName : Description; } }
        public string Description { get; set; }
    }
}
