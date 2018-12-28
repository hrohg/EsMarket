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
        Admin=1, Moderator=14,
        Director=2,
        Manager=3, SeniorManager = 13, JuniorManager = 12,
        StockKeeper=5, 
        SaleManager=6, SeniorSeller=7, Seller=4, JuniorSeller = 11,
        Cashier = 8, SeniorCashier = 9, JuniorCashier = 10,

    }
}
