using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ES.Business.Helpers;
using ES.Business.Models;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class StockTakeManager:BaseManager
    {
        /// <summary>
        /// StockTaking converters
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        #region Converters
        public static StockTakeModel ConvertStockTake(StockTake item)
        {
            if (item == null) return null;
            return new StockTakeModel
            {
                Id = item.Id,
                MemberId = item.MemberId,
                StockId = item.StockId,
                CreateDate = item.CreateDate,
                ClosedDate = item.ClosedDate,
                StockTakeNumber = item.StockTakeNumber,
                CreatorId = item.CreatorId,
                ModifierId = item.ModifierId,
                CloserId = item.CloserId,
                Description=item.Description
            };
        }
        public static StockTake ConvertStockTake(StockTakeModel item)
        {
            if (item == null) return null;
            return new StockTake
            {
                Id = item.Id,
                MemberId = item.MemberId,
                StockId = item.StockId,
                CreateDate = item.CreateDate,
                ClosedDate = item.ClosedDate,
                StockTakeNumber = item.StockTakeNumber,
                CreatorId = item.CreatorId,
                ModifierId = item.ModifierId,
                CloserId = item.CloserId,
                Description = item.Description
            };
        }
        public static StockTakeItemsModel ConvertStockTakeItem(StockTakeItems item)
        {
            if (item == null) return null;
            return new StockTakeItemsModel(item.StockTakeId)
            {
                Id = item.Id,
                StockTakeId = item.StockTakeId,
                ProductId = item.ProductId,
                ProductDescription = item.ProductDescription,
                CodeOrBarcode = item.CodeOrBarcode,
                Mu = item.Mu,
                Price = item.Price,
                Description = item.Description,
                Quantity = item.Quantity,
                StockTakeQuantity = item.StockTakeQuantity??0,
                StockTakeDate = item.StockTakeDate
            };
        }
        public static StockTakeItems ConvertStockTakeItem(StockTakeItemsModel item)
        {
            if (item == null) return null;
            return new StockTakeItems
            {
                Id = item.Id,
                StockTakeId = item.StockTakeId,
                ProductId = item.ProductId,
                ProductDescription = item.ProductDescription,
                CodeOrBarcode = item.CodeOrBarcode,
                Mu = item.Mu,
                Price = item.Price,
                Description = item.Description,
                Quantity = item.Quantity,
                StockTakeQuantity = item.StockTakeQuantity,
                StockTakeDate = item.StockTakeDate
            };
        }
        #endregion
        #region StockTake public methods
        public static StockTakeModel GetStockTaking(Guid id, long memberId)
        {
            return ConvertStockTake(TryGetStockTake(id, memberId));
        }
        public static StockTakeModel GetLastStockTake(long memberId)
        {
            return ConvertStockTake(TryGetLastStockTake(memberId));
        }
        public static List<StockTakeModel> GetOpeningStockTakes(long memberId)
        {
            return TryGetOpeningStockTakes(memberId).Select(ConvertStockTake).ToList();
        }
        public static List<StockTakeModel> GetStockTakeByCreateDate(DateTime startDate, DateTime endDate, long memberId)
        {
            return TryGetStockTakeByCreateDate(startDate, endDate, memberId).Select(ConvertStockTake).ToList();
        }
        public static StockTakeModel CreateStockTaking(long stockId, long creatorId, long memberId)
        {
            return ConvertStockTake(TryCreateStockTake(stockId, creatorId, memberId));
        }
        #endregion
        #region StockTakeItems public methods
        public static StockTakeItemsModel GetStockTakeItem(Guid stockTakeId, string codeOrBarcode, long memberId)
        {
            return ConvertStockTakeItem(TryGetStockTakeItem(stockTakeId,codeOrBarcode, memberId));
        }
        public static List<StockTakeItemsModel> GetStockTakeItems(Guid stockTakingId, long memberId)
        {
            return TryGetStockTakeItems(stockTakingId, memberId).Select(ConvertStockTakeItem).ToList();
        }
        public static bool EditStockTakeItems(StockTakeItemsModel item, long? stockId, long memberId)
        {
            return TryEditStockTakeItems(ConvertStockTakeItem(item), stockId, memberId);
        }
        public static bool RemoveStoCkakeItem(Guid id, long memberId)
        {
            return TryRemoveStoCkakeItem(id, memberId);
        }
        #endregion
        #region StockTake Private methods
        private static StockTake TryGetStockTake(Guid id, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.StockTake.Single(s => s.Id == id && s.MemberId==memberId);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static StockTake TryGetLastStockTake(long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.StockTake.Where(s=>s.MemberId==memberId).OrderByDescending(s=>s.CreateDate).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static List<StockTake> TryGetOpeningStockTakes(long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.StockTake.Where(s =>s.MemberId==memberId && s.ClosedDate == null).ToList();
                }
            }
            catch (Exception)
            {
                return new List<StockTake>();
            }
        }
        private static List<StockTake> TryGetStockTakeByCreateDate(DateTime startDate, DateTime endDate, long memberid)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.StockTake.Where(s => s.CreateDate >= startDate && s.CreateDate<=endDate && s.MemberId==memberid).ToList();
                }
            }
            catch (Exception)
            {
                return new List<StockTake>();
            }
        }
        private static StockTake TryCreateStockTake(long stockId, long creatorId, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var lastNumber = db.StockTake.Where(s => s.MemberId == memberId).OrderByDescending(s => s.StockTakeNumber)
                                .Select(s => s.StockTakeNumber).FirstOrDefault();
                    var newItem = new StockTake
                    {
                        Id = Guid.NewGuid(),
                        MemberId = memberId,
                        StockTakeNumber = lastNumber!=null? ++lastNumber: 1,
                        StockId = stockId,
                        CreateDate = DateTime.Now,
                        CreatorId = creatorId
                    };
                    db.StockTake.Add(newItem);
                    db.SaveChanges();
                return newItem;
            }
        }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
        #region StockTakeItems private methods
        private static StockTakeItems TryGetStockTakeItem(Guid stockTakeId, string codeOrBarCode, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.StockTakeItems.Include(s => s.StockTake).
                        SingleOrDefault(s =>s.StockTake.MemberId == memberId && s.StockTakeId==stockTakeId && s.CodeOrBarcode == codeOrBarCode );
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private static List<StockTakeItems> TryGetStockTakeItems(Guid stockTakingId, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return
                        db.StockTakeItems.Include(s => s.StockTake)
                            .Where(s => s.StockTakeId == stockTakingId && s.StockTake.MemberId==memberId).ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<StockTakeItems>();
            }
        }
        private static bool TryEditStockTakeItems(StockTakeItems item, long? stockId, long memberId)
        {
            try
            {
                using (var db=GetDataContext())
                {
                    if(item.ProductId!=null)
                        item.Quantity = stockId != null ? ProductsManager.GetProductItemCountFromStock(item.ProductId, new List<long> { (long)stockId }, memberId) : 0;
                    item.StockTakeDate = DateTime.Now;
                    var exItem=db.StockTakeItems.
                        SingleOrDefault(s => s.StockTakeId==item.StockTakeId && (s.Id==item.Id || s.CodeOrBarcode==item.CodeOrBarcode));
                    if (exItem != null)
                    {
                        exItem.ProductId = item.ProductId;
                        exItem.ProductDescription = item.ProductDescription;
                        exItem.CodeOrBarcode = item.CodeOrBarcode;
                        exItem.Description = item.Description;
                        exItem.Mu = item.Mu;
                        exItem.Price = item.Price;
                        exItem.Quantity = item.Quantity;
                        exItem.StockTakeQuantity = item.StockTakeQuantity;
                        exItem.StockTakeDate = item.StockTakeDate;
                        exItem.Description = item.Description;
                    }
                    else
                    {
                        db.StockTakeItems.Add(item);
                    }
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception es)
            {
                return false;
            }
        }
        private static bool TryRemoveStoCkakeItem(Guid id, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var exItem =
                        db.StockTakeItems.SingleOrDefault(s => s.Id == id && s.StockTake.MemberId == memberId && s.StockTake.ClosedDate == null);
                    if (exItem == null) {return false;}
                    db.StockTakeItems.Remove(exItem);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
              return false;
            }
        }
        #endregion
    }
}
