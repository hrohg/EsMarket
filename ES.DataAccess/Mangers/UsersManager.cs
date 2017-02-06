using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESL.DataAccess.Helpers;
using ESL.DataAccess.Models;
namespace ESL.DataAccess.Mangers
{
    public class UsersManager
    {
        public List<EsUser> GetEsUsers()
        {
            using (var db = new ESStockDBEntities())
            {
                return db.EsUsers.ToList();
            }
        }
    }
}
