using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using ESL.DataAccess.Helpers;
using ESL.DataAccess.Models;

namespace ESL.DataAccess.Mangers
{
    public class ProductsManager:ManagerBase
    {
        public static List<Product> GetProductsByMember(int memberId)
        {
            using (var db = CreateDataContext())
            {
                return db.Products.Where(s=>s.EsMemberID==memberId).ToList();
            }
        }
    }
}
