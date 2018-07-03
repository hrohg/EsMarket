using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ES.Business.Helpers;
using ES.Common.Managers;
using ES.Data.Models;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class StockManager : BaseManager
    {
        #region Converter
        private static StockModel Convert(EsStock item)
        {
            if (item == null) return null;
            return new StockModel(item.EsMemberId)
            {
                Id = item.Id,
                ParentId = item.ParentStockId,
                StorekeeperId = item.StorekeeperId,
                Name = item.Name,
                Description = item.Description,
                Address = item.Address,
                SpecialCode = item.SpecialCode,
                IsEnable = item.IsEnable,
                MemberId = item.EsMemberId,
                //ParentStock = Convert(item.EsStock2),
                Storekeeper = UsersManager.ConvertEsUser(item.EsUsers),
                //Member = MembersManager.ConvertMember(item.EsMembers)
            };
        }
        private static EsStock Convert(StockModel item)
        {
            if (item == null) return null;
            return new EsStock
            {
                Id = item.Id,
                ParentStockId = item.ParentId,
                StorekeeperId = item.StorekeeperId,
                Name = item.Name,
                Description = item.Description,
                Address = item.Address,
                SpecialCode = item.SpecialCode,
                IsEnable = item.IsEnable,
                EsMemberId = item.MemberId
            };
        }
        #endregion

        #region Public methods
        public static EsStock GetDefaultStock(long memberId)
        {
            using (var db = GetDataContext())
            {
                return db.EsStock.FirstOrDefault(s => s.EsMemberId == memberId && s.IsEnable);
            }
        }
        public static List<StockModel> GetStocks()
        {
            return TryGetStocks().Select(Convert).ToList();

        }
        public static List<StockModel> GetAllStocks()
        {
            return TryGetAllStocks().Select(Convert).ToList();

        }
        public static IEnumerable<StockModel> GetStocks(List<long> ids)
        {
            return TryGetStocks(ids).Select(Convert);
        }
        public static StockModel GetStock(long? id)
        {
            return Convert(TryGetStock(id));
        }
        public static bool ManageStock(StockModel item)
        {
            return TryManageStock(Convert(item));
        }
        #endregion

        #region Private methods
        private static EsStock TryGetStock(long? stockId)
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsStock
                        .Include(s=>s.EsUsers)
                        .Include(s=>s.EsMembers)
                        .Include(s=>s.EsStock2)
                        .SingleOrDefault(s => s.Id == stockId && s.EsMemberId == memberId && s.IsEnable);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static List<EsStock> TryGetStocks()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsStock
                        .Include(s => s.EsUsers)
                        .Include(s => s.EsMembers)
                        .Include(s => s.EsStock2)
                        .Where(s => s.EsMemberId == ApplicationManager.Member.Id && s.IsEnable).ToList();
                }
                catch (Exception)
                {
                    return new List<EsStock>();
                }
                
            }
        }
        private static List<EsStock> TryGetAllStocks()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsStock
                        .Include(s => s.EsUsers)
                        .Include(s => s.EsMembers)
                        .Include(s => s.EsStock2)
                        .Where(s => s.EsMemberId == ApplicationManager.Member.Id).ToList();
                }
                catch (Exception)
                {
                    return new List<EsStock>();
                }

            }
        }
        private static IEnumerable<EsStock> TryGetStocks(IEnumerable<long> ids)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsStock
                        .Include(s => s.EsUsers)
                        .Include(s => s.EsMembers)
                        .Include(s => s.EsStock2)
                        .Where(s => s.EsMemberId == ApplicationManager.Member.Id && s.IsEnable && ids.Contains(s.Id)).ToList();
                }
                catch (Exception)
                {
                    return new List<EsStock>();
                }

            }
        }
        private static bool TryManageStock(EsStock item)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var exItem = db.EsStock.SingleOrDefault(s => s.Id == item.Id && s.EsMemberId == item.EsMemberId);
                    if (exItem == null)
                    {
                        db.EsStock.Add(item);
                    }
                    else
                    {
                        exItem.Name = item.Name;
                        exItem.Description = item.Description;
                        exItem.Address = item.Address;
                        exItem.SpecialCode = item.SpecialCode;
                        exItem.IsEnable = item.IsEnable;
                    }
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage("Գործողության ընդհատում");
                    MessageManager.OnMessage(ex.ToString());
                    return false;
                }
            }
        }
        #endregion

        
    }
}
