namespace ES.Common.Enumerations
{
	

	public class Role
	{
		public int ID { get; set; }
		public string Name { get; set; }

        //public static List<Role> GetRoles()
        //{
        //    var roles = Enum.GetValues(typeof(EslRoles));
        //    List<Role> list = new List<Role>();
        //    foreach (var role in roles)
        //    {
        //        list.Add(new Role { ID = (int)(EslRoles)role, Name = role.ToString() });
        //    }
        //    return list;
        //}
	}

    public enum UserRoleEnum
    {
        Admin, Director, Manager, StockKeeper, SaleManager, SeniorSaler, Saller, 
    }
}
