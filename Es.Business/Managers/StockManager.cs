using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ES.Business.Helpers;
using ES.Data.Model;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class StockManager : BaseManager
    {
        #region Converter
        private static StockModel Convert(EsStock item)
        {
            if (item == null) return null;
            return new StockModel
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
            return new EsStock()
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
        public static List<StockModel> GetStocks(long memberId)
        {
            return TryGetStocks(memberId).Select(Convert).ToList();

        }
        public static IEnumerable<StockModel> GetStocks(List<long> ids, long memberId)
        {
            return TryGetStocks(ids,memberId).Select(Convert);

        }
        public static StockModel GetStock(long? id, long memberId)
        {
            return Convert(TryGetStock(id, memberId));
        }
        #endregion
        #region Private methods
        private static EsStock TryGetStock(long? stockId, long memberId)
        {
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
        private static List<EsStock> TryGetStocks(long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsStock
                        .Include(s => s.EsUsers)
                        .Include(s => s.EsMembers)
                        .Include(s => s.EsStock2)
                        .Where(s => s.EsMemberId == memberId && s.IsEnable).ToList();
                }
                catch (Exception)
                {
                    return new List<EsStock>();
                }
                
            }
        }
        private static IEnumerable<EsStock> TryGetStocks(IEnumerable<long> ids, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.EsStock
                        .Include(s => s.EsUsers)
                        .Include(s => s.EsMembers)
                        .Include(s => s.EsStock2)
                        .Where(s => s.EsMemberId == memberId && s.IsEnable && ids.Contains(s.Id)).ToList();
                }
                catch (Exception)
                {
                    return new List<EsStock>();
                }

            }
        }
        #endregion
    }
}
