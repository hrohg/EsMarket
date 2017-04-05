using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using ES.Business.Helpers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Enumerations;
using ES.Data.Models;
using ES.Data.Models.EsModels;
using ES.DataAccess.Models;
using DataTable = System.Data.DataTable;
using ProductModel = ES.Business.Models.ProductModel;

namespace ES.Business.Managers
{
    public class ProductsManager : BaseManager
    {
        /// <summary>
        /// Categories methods
        /// </summary>
        /// <returns></returns>
        #region Categories public methods
        public static List<EsCategories> GetCategories()
        {
            return TryGetCategories();
        }

        #endregion
        #region Categories private methods
        private static List<EsCategories> TryGetCategories()
        {
            using (var db = GetDataContext())
            {
                return db.EsCategories.ToList();
            }
        }
        #endregion
        /// <summary>
        /// Brands methods
        /// </summary>
        /// <returns></returns>
        #region Brand public methods
        public static List<Brands> GetServerBrands()
        {
            return TryGetServerBrands();
        }
        public static List<Brands> GetMemberBrands(long memberId)
        {
            return TryGetMemberBrands(memberId);
        }
        public static List<Brands> GetAllBrands(bool? isActive = null)
        {
            return TryGetAllBrands(isActive);
        }
        public static List<Brands> GetBrands(long memberId)
        {
            return TryGetBrands(memberId);
        }
        public static bool EditBrands(List<Brands> brands, long memberId)
        {
            using (var db = GetDataContext())
            {
                var exMemberBrands = db.MembersBrands.Where(s => s.MemberId == memberId).ToList();
                var exRemovedBrands = exMemberBrands.Where(s => !brands.Select(t => t.Id).Contains(s.BrandId)).ToList();
                foreach (var removedBrand in exRemovedBrands)
                {
                    db.MembersBrands.Remove(removedBrand);
                }
                foreach (var brand in brands)
                {
                    var exBrand = db.MembersBrands.SingleOrDefault(s => s.MemberId == memberId && s.BrandId == brand.Id);
                    if (exBrand == null)
                    {
                        db.MembersBrands.Add(new MembersBrands { Id = Guid.NewGuid(), BrandId = brand.Id, MemberId = memberId });
                    }
                }
                db.SaveChanges();
            }
            return true;
        }
        #endregion
        #region Brand private methods
        private static List<Brands> TryGetMemberBrands(long memberId)
        {
            using (var db = GetDataContext())
            {
                return db.MembersBrands.Where(s => s.MemberId == memberId).Select(s => s.Brands).OrderBy(s => s.BrandName).ToList();
            }
        }
        private static List<Brands> TryGetAllBrands(bool? isActive)
        {
            using (var db = GetDataContext())
            {
                return isActive == null ? db.Brands.OrderBy(s => s.BrandName).ToList() : db.Brands.Where(s => s.IsActive != null && s.IsActive == isActive).OrderBy(s => s.BrandName).ToList();
            }
        }
        private static List<Brands> TryGetBrands(long memberId)
        {
            using (var db = GetDataContext())
            {
                return db.MembersBrands.Where(s => s.MemberId == memberId).Select(s => s.Brands).OrderBy(s => s.BrandName).ToList();
            }
        }
        private static List<Brands> TryGetServerBrands()
        {
            using (var db = GetServerDataContext())
            {
                return db.Brands.OrderBy(s => s.BrandName).ToList();
            }
        }
        #endregion

        ///  <summary>
        ///  ProductMethods
        ///  </summary>
        ///  <param name="memberId"></param>
        /// <param name="item"></param>
        /// <returns></returns>

        #region Products

        #region Internal cash
        //private Dictionary<Guid, ProductModel> _products = new Dictionary<Guid, ProductModel>();
        #endregion
        #region Convert Products and items
        public static ProductModel CopyProduct(ProductModel item)
        {
            if (item == null) return null;
            var exItem = new ProductModel(item.EsMemberId, item.LastModifierId, item.IsEnabled);
            exItem.Id = item.Id;
            exItem.Code = item.Code;
            exItem.Barcode = item.Barcode;
            exItem.HcdCs = item.HcdCs;
            exItem.Description = item.Description;
            exItem.Mu = item.Mu;
            exItem.IsWeight = item.IsWeight;
            exItem.Note = item.Note;
            exItem.CostPrice = item.CostPrice;
            exItem.OldPrice = item.OldPrice;
            exItem.Price = item.Price;
            exItem.Discount = item.Discount;
            exItem.DealerPrice = item.DealerProfitPercent != null ? item.DealerPrice : null;
            exItem.DealerDiscount = item.DealerDiscount;
            exItem.MinQuantity = item.MinQuantity;
            exItem.ExpiryDays = item.ExpiryDays;
            exItem.ImagePath = item.ImagePath;
            exItem.BrandId = item.BrandId;
            exItem.Brand = item.Brand;
            exItem.EsMemberId = item.EsMemberId;
            exItem.EsMember = item.EsMember;
            exItem.LastModifierId = item.LastModifierId;
            exItem.ProductGroups = item.ProductGroups;
            return exItem;
        }
        public static void CopyProduct(ProductModel product, ProductModel item)
        {
            if (item == null || product == null)
            {
                return;
            }
            product.Id = item.Id;
            product.Code = item.Code;
            product.Barcode = item.Barcode;
            product.HcdCs = item.HcdCs;
            product.Description = item.Description;
            product.Mu = item.Mu;
            product.IsWeight = item.IsWeight;
            product.Note = item.Note;
            product.CostPrice = item.CostPrice;
            product.OldPrice = item.OldPrice;
            product.Price = item.Price;
            product.Discount = item.Discount;
            product.DealerPrice = item.DealerProfitPercent != null ? item.DealerPrice : null;
            product.DealerDiscount = item.DealerDiscount;
            product.MinQuantity = item.MinQuantity;
            product.ExpiryDays = item.ExpiryDays;
            product.ImagePath = item.ImagePath;
            product.BrandId = item.BrandId;
            product.Brand = item.Brand;
            product.EsMemberId = item.EsMemberId;
            product.EsMember = item.EsMember;
            product.LastModifierId = item.LastModifierId;
            product.ProductGroups = item.ProductGroups;
            product.IsEnabled = item.IsEnabled;
        }
        public List<ProductModel> Convert(List<Products> items)
        {
            return items.Select(Convert).ToList();
        }
        public static ProductModel Convert(Products item)
        {
            if (item == null) return null;
            var exItem = new ProductModel(item.EsMemberId, item.LastModifierId, item.IsEnable);
            exItem.Id = item.Id;
            exItem.Code = item.Code;
            exItem.Barcode = item.Barcode;
            exItem.HcdCs = item.HCDCS;
            exItem.Description = item.Description;
            exItem.Mu = item.Mu;
            exItem.IsWeight = item.IsWeight;
            exItem.Note = item.Note;
            exItem.CostPrice = item.CostPrice;
            exItem.OldPrice = item.OldPrice;
            exItem.Price = item.Price;
            exItem.Discount = item.Discount;
            exItem.DealerPrice = item.DealerPrice;
            exItem.DealerDiscount = item.DealerDiscount;
            exItem.MinQuantity = item.MinQuantity;
            exItem.ExpiryDays = item.ExpiryDays;
            exItem.ImagePath = item.ImagePath;
            exItem.IsEnabled = item.IsEnable;
            exItem.BrandId = item.BrandId;
            //exItem.Brand = item.Brand;
            exItem.EsMemberId = item.EsMemberId;
            //exItem.EsMember = item.EsMember;
            exItem.LastModifierId = item.LastModifierId;
            exItem.ProductCategories = Convert(item.ProductCategories);
            exItem.ProductGroups = Convert(item.ProductGroup.ToList());
            //_products.Add(exItem.Id, exItem);
            return exItem;
        }

        private static Products Convert(ProductModel item)
        {
            if (item == null) return null;
            var exItem = new Products();
            exItem.Id = item.Id;
            exItem.Code = item.Code;
            exItem.Barcode = item.Barcode;
            exItem.HCDCS = item.HcdCs;
            exItem.Description = item.Description;
            exItem.Mu = item.Mu;
            exItem.IsWeight = item.IsWeight;
            exItem.Note = item.Note;
            exItem.CostPrice = item.CostPrice;
            exItem.OldPrice = item.OldPrice;
            exItem.Price = item.Price;
            exItem.Discount = item.Discount;
            exItem.DealerPrice = item.DealerProfitPercent != null ? item.DealerPrice : null;
            exItem.DealerDiscount = item.DealerDiscount;
            exItem.MinQuantity = item.MinQuantity;
            exItem.ExpiryDays = item.ExpiryDays;
            exItem.ImagePath = item.ImagePath;
            exItem.IsEnable = item.IsEnabled;
            exItem.BrandId = item.BrandId;
            //exItem.Brand = item.Brand;
            exItem.EsMemberId = item.EsMemberId;
            //exItem.EsMember = item.EsMember;
            exItem.LastModifierId = item.LastModifierId;
            exItem.ProductCategories = Convert(item.ProductCategories);
            exItem.ProductGroup = Convert(item.ProductGroups);
            return exItem;
        }

        private static ICollection<ProductCategories> Convert(List<ProductCategoriesModel> items)
        {
            if (items == null) return new List<ProductCategories>();
            return items.Select(s => new ProductCategories { Id = s.Id, CategoryId = s.CategoriesId, ProductId = s.ProductId }).ToList();
        }
        private static List<ProductCategoriesModel> Convert(ICollection<ProductCategories> items)
        {
            if (items == null) return new List<ProductCategoriesModel>();
            return items.Select(s => new ProductCategoriesModel() { Id = s.Id, CategoriesId = (int)s.CategoryId, ProductId = s.ProductId }).ToList();
        }
        public static ICollection<ProductGroup> Convert(List<ProductGroupModel> items)
        {
            if (items == null || items.Count == 0) return new List<ProductGroup>();
            return items.Select(s => new ProductGroup { Id = Guid.NewGuid(), Barcode = s.Barcode, ProductId = s.ProductId, MemberId = s.MemberId }).ToList();

        }
        public static List<ProductGroupModel> Convert(List<ProductGroup> items)
        {
            if (items == null || items.Count == 0) return new List<ProductGroupModel>();
            return items.Select(s => new ProductGroupModel { Barcode = s.Barcode, ProductId = s.ProductId, MemberId = s.MemberId }).ToList();
        }
        #endregion
        #region Product Public methods
        public static bool ChangeProductCode(Guid id, string productCode, long memberId)
        {
            return TryChangeProductCode(id, productCode, memberId);
        }
        public static bool ChangeProductEnabled(Guid id, long memberId)
        {
            return TryChangeProductEnabled(id, memberId);
        }
        //public static List<ProductModel> GetProductsByMember(long memberId)
        //{
        //    return TryGetProductsByMember(memberId).Select(ConvertProduct).OrderByDescending(s => s.Code).ToList();
        //}
        public ProductModel GetProductsByCodeOrBarcode(string code, long memberId)
        {
            return Convert(TryGetProductsByCodeOrBarcode(code, memberId));
        }
        public ProductModel GetProduct(Guid id, long memberId)
        {
            return Convert(TryGetProduct(id, memberId));
        }
        public List<ProductModel> GetExistingProduct(long memberId)
        {
            return TryGetExistingProduct(memberId);
        }
        public List<ProductModel> GetProducts(long memberId)
        {
            return TryGetProducts(memberId).Select(Convert).ToList();
        }
        public List<ProductModel> GetProductsShortData(long memberId)
        {
            return TryGetProductsShortData(memberId);
        }
        public List<ProductModel> GetRegisteredProducts(long memberId)
        {
            return TryGetRegisteredProducts(memberId).Select(Convert).ToList();
        }
        public List<ProductModel> GetProductsBy(ProductViewType type, long memberId)
        {
            return TryGetProductsBy(type, memberId);

        }
        public ProductModel EditProduct(ProductModel product)
        {
            return Convert(TryEditProduct(Convert(product)));
        }
        public static bool EditProducts(List<ProductModel> products)
        {
            return TryEditProducts(products.Select(Convert).ToList());
        }
        public static bool InsertProduct(ProductModel product)
        {
            return TryInsertProduct(Convert(product));
        }
        public static bool DeleteProduct(ProductModel product)
        {
            return TryDeleteProduct(Convert(product));
        }
        public static bool DeleteProduct(Guid id, long memebrId)
        {
            return TryDeleteProduct(id, memebrId);
        }
        public static decimal GetProductItemCount(Guid? productId, List<long> fromStocks, long memberId)
        {
            return TryGetProductItemCount(productId, fromStocks, memberId);
        }
        public static decimal GetProductItemCount(Guid? productId, long memberId)
        {
            return TryGetProductItemCount(productId, memberId);
        }
        public static decimal GetProductItemCountFromStock(Guid? productId, List<long> stockIds, long memeberId)
        {
            return TryGetProductItemCountFromStock(productId, stockIds, memeberId);
        }
        public List<ProductItemModel> GetProductItems(long memberId)
        {
            var productItems = TryGetProductItems(memberId);
            var products = productItems.GroupBy(s => s.Products).Select(s => Convert(s.Key)).ToList();
            return productItems.Select(s => Convert(s, products)).ToList();
        }
        public static List<ProductItemModel> GetProductItemsByStock(long stockId, long memberId)
        {
            var productItems = TryGetProductItemsByStock(stockId, memberId);
            var products = productItems.GroupBy(s => s.Products).Select(s => Convert(s.Key)).ToList();
            return productItems.Select(s => Convert(s, products)).ToList();
        }
        public static List<ProductResidue> GeProductResidues(long memberId)
        {
            return TryGetProductResidue(memberId);
        }
        public List<ProductItemModel> GetProductItemsForInvoices(IEnumerable<Guid> invoiceIds, long memberId)
        {
            var products = GetProducts(memberId);
            return TryGetProductItemsForInvocies(invoiceIds, memberId).Select(s => Convert(s, products)).ToList();
        }
        public List<ProductItemModel> GetAllProductItems(long memberId)
        {
            var products = GetProducts(memberId);
            return TryGetAllProductItems(memberId).Select(s => Convert(s, products)).ToList();
        }
        public List<ProductItemModel> GetProductItemsFromStocks(List<long> stockIds, long memberId)
        {
            var products = GetProducts(memberId);
            return TryGetProductItemsFromStocks(stockIds, memberId).Select(s => Convert(s, products)).ToList();
        }
        public List<ProductItemModel> GetUnavailableProductItems(List<Guid> productItemIds, long memberId)
        {
            var products = GetProducts(memberId);
            return TryGetUnavilableProductItems(productItemIds, memberId).Select(s => Convert(s, products)).ToList();
        }
        public static long GetProductCount(long memberId)
        {
            return TryGetProductCount(memberId);
        }
        public static List<ProductOrderItemsModel> GetProductOrderByBrands(List<Brands> brands, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var productItems = db.ProductItems.Where(s => s.MemberId == memberId && s.Products.IsEnable && s.Products.MinQuantity != null).ToList();
                    var brandIds = brands.Select(t => t.Id).ToList();
                    var products = db.Products.Where(s => s.EsMemberId == memberId && s.IsEnable && s.MinQuantity != null && s.BrandId != null && brandIds.Contains((long)s.BrandId)).ToList();
                    products = products.Where(s => s.MinQuantity > productItems.Where(t => t.ProductId == s.Id)
                        .Sum(t => t.Quantity)).OrderByDescending(s => s.InvoiceItems.First().Invoices.CreateDate).ToList();
                    var productOrderItems =
                        products.Select(s => new ProductOrderItemsModel
                    {
                        Code = s.Code,
                        Description = s.Description,
                        Price = s.Price,
                        Quantity = s.MinQuantity - productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity),
                        ExistingQuantity = productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity),
                        ProviderId = s.InvoiceItems.First().Invoices.PartnerId

                    }).ToList();
                    return productOrderItems;




                    //var products = from pi in db.ProductItems where pi.Products.EsMemberId != memberId && pi.Products.MinQuantity != null select pi;

                    //var newProductOrder = db.InvoiceItems.Where(s=>products.Contains(s.ProductId)).OrderByDescending(s=>s.Invoices.CreateDate).GroupBy(s=>s.ProductId)
                    //    .Select(s=> new ProductOrderItemsModel
                    //                      {
                    //                          Code = s.FirstOrDefault().Products.Code,
                    //                          Description = s.FirstOrDefault().Products.Description,
                    //                          Price = s.FirstOrDefault().Products.Price,
                    //                          Quantity = s.FirstOrDefault().Products.MinQuantity - db.ProductItems.Where(t=>t.ProductId==s.FirstOrDefault().ProductId).Sum(t=>t.Quantity),
                    //                          ExistingQuantity = s.Sum(t => t.Quantity),
                    //                          ProviderId = s.FirstOrDefault().Invoices.PartnerId
                    //                      }).ToList();
                    //return newProductOrder.ToList();

                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public static List<ProductOrderItemsModel> GetProductOrderByProduct(long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    //var productItems = db.ProductItems.Where(s => s.MemberId == memberId && s.Products.IsEnable && s.Products.MinQuantity != null);
                    //var products = db.Products.Where(s => s.EsMemberId == memberId && s.IsEnable && s.MinQuantity != null);
                    //var productOrderItems = products.Where(s => s.MinQuantity > productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity))
                    //    .Select(s => new ProductOrderItemsModel()
                    //{
                    //    Code = s.Code,
                    //    Description = s.Description,
                    //    Price = s.Price,
                    //    Quantity = s.MinQuantity - productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity),
                    //    ExistingQuantity = productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity)
                    //}).ToList();
                    //return productOrderItems;
                    var products = db.Products.Where(s => s.EsMemberId == memberId && s.IsEnable && s.MinQuantity != null);

                    var productIds = products.Select(t => t.Id);
                    var productItems = db.ProductItems.Where(s => s.MemberId == memberId && productIds.Contains(s.ProductId));
                    products =
                        products.Where(s => s.MinQuantity > productItems.Where(t => t.ProductId == s.Id)
                            .Sum(t => t.Quantity));
                    var invoiceItems = db.InvoiceItems.Where(s => s.Invoices.InvoiceTypeId == (long)InvoiceType.PurchaseInvoice).OrderByDescending(s => s.Invoices.CreateDate);
                    var productOrderItems = products.Select(s => new ProductOrderItemsModel
                        {
                            Code = s.Code,
                            Description = s.Description,
                            Price = s.Price,
                            CostPrice = s.CostPrice,
                            Quantity = s.MinQuantity - productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity),
                            ExistingQuantity = productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity),
                            ProviderId = invoiceItems.Where(t => t.ProductId == s.Id).Select(t => t.Invoices.PartnerId).FirstOrDefault()
                        }).ToList();
                    return productOrderItems;




                    //var products = from pi in db.ProductItems where pi.Products.EsMemberId != memberId && pi.Products.MinQuantity != null select pi;

                    //var newProductOrder = db.InvoiceItems.Where(s=>products.Contains(s.ProductId)).OrderByDescending(s=>s.Invoices.CreateDate).GroupBy(s=>s.ProductId)
                    //    .Select(s=> new ProductOrderItemsModel
                    //                      {
                    //                          Code = s.FirstOrDefault().Products.Code,
                    //                          Description = s.FirstOrDefault().Products.Description,
                    //                          Price = s.FirstOrDefault().Products.Price,
                    //                          Quantity = s.FirstOrDefault().Products.MinQuantity - db.ProductItems.Where(t=>t.ProductId==s.FirstOrDefault().ProductId).Sum(t=>t.Quantity),
                    //                          ExistingQuantity = s.Sum(t => t.Quantity),
                    //                          ProviderId = s.FirstOrDefault().Invoices.PartnerId
                    //                      }).ToList();
                    //return newProductOrder.ToList();

                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static List<ProductProvider> GetProductsProviders(List<Guid> productIds)
        {
            return TryGetProductsProviders(productIds);
        }
        public static int GetNextProductCode(long memberId)
        {
            return TryGetNextProductCode(memberId);
        }
        #endregion
        #region Convert ProductItem
        public static ProductItemModel Convert(ProductItems item, List<ProductModel> products)
        {
            if (item == null) return null;
            var exItem = new ProductItemModel();
            exItem.Id = item.Id;
            exItem.MemberId = item.MemberId;
            exItem.ProductId = item.ProductId;
            exItem.Product = products != null ? products.SingleOrDefault(s => s.Id == item.ProductId) : Convert(item.Products);
            exItem.DeliveryInvoiceId = item.DeliveryInvoiceId;
            exItem.StockId = item.StockId;
            exItem.ReservedById = item.ReservedById;
            exItem.Quantity = item.Quantity;
            exItem.ExpiryDate = item.ExpiryDate;
            exItem.CostPrice = item.CostPrice;
            exItem.Description = item.Description;
            exItem.CoordinateX = item.CoordinateX;
            exItem.CoordinateY = item.CoordinateY;
            exItem.CoordinateZ = item.CoordinateZ;

            return exItem;
        }
        public ProductItemModel Convert(ProductItems item)
        {
            if (item == null) return null;
            var exItem = new ProductItemModel();
            exItem.Id = item.Id;
            exItem.MemberId = item.MemberId;
            exItem.ProductId = item.ProductId;
            exItem.Product = Convert(item.Products);
            exItem.DeliveryInvoiceId = item.DeliveryInvoiceId;
            exItem.StockId = item.StockId;
            exItem.ReservedById = item.ReservedById;
            exItem.Quantity = item.Quantity;
            exItem.ExpiryDate = item.ExpiryDate;
            exItem.CostPrice = item.CostPrice;
            exItem.Description = item.Description;
            exItem.CoordinateX = item.CoordinateX;
            exItem.CoordinateY = item.CoordinateY;
            exItem.CoordinateZ = item.CoordinateZ;

            return exItem;
        }
        private static ProductItems Convert(ProductItemModel item)
        {
            if (item == null) return null;
            var exItem = new ProductItems();
            exItem.Id = item.Id;
            exItem.MemberId = item.MemberId;
            exItem.ProductId = item.ProductId;
            exItem.DeliveryInvoiceId = item.DeliveryInvoiceId;
            exItem.StockId = item.StockId;
            exItem.ReservedById = item.ReservedById;
            exItem.Quantity = item.Quantity;
            exItem.CostPrice = item.CostPrice;
            exItem.ExpiryDate = item.ExpiryDate;
            exItem.Description = item.Description;
            exItem.CoordinateX = item.CoordinateX;
            exItem.CoordinateY = item.CoordinateY;
            exItem.CoordinateZ = item.CoordinateZ;
            return exItem;
        }
        #endregion
        #region Product Private methods
        private static bool TryChangeProductCode(Guid id, string code, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var exItem = db.Products.FirstOrDefault(s => s.EsMemberId == memberId && s.Code == code);
                    if (exItem != null)
                    {
                        MessageBox.Show(code + " կոդը զբաղված է։", "Գործողության ընդհատում", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return false;
                    }
                    exItem = db.Products.SingleOrDefault(s => s.EsMemberId == memberId && s.Id == id);
                    if (exItem == null)
                    {
                        MessageBox.Show("Սխալ գործողություն։", "Գործողության ընդհատում", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    exItem.Code = code;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool TryChangeProductEnabled(Guid id, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var exItem = db.Products.FirstOrDefault(s => s.EsMemberId == memberId && s.Id == id);
                    if (exItem == null)
                    {
                        MessageBox.Show("Ապրանքը հայտնաբերված չէ։", "Գործողության ընդհատում", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return false;
                    }
                    exItem.IsEnable = !exItem.IsEnable;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static decimal TryGetProductItemCount(Guid? productId, List<long> fromStocks, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.ProductItems.Where(s => s.MemberId == memberId && s.StockId != null && fromStocks.Contains((long)s.StockId) && s.ProductId == productId).Sum(s => s.Quantity);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        private static decimal TryGetProductItemCount(Guid? productId, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.ProductItems.Where(s => s.MemberId == memberId && s.ProductId == productId).Sum(s => s.Quantity);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        private static decimal TryGetProductItemCountFromStock(Guid? productId, List<long> stockIds, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.ProductItems.Where(s => s.StockId != null && stockIds.Contains((long)s.StockId) && s.ProductId == productId && s.MemberId == memberId).Sum(s => s.Quantity);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        private static List<Products> TryGetProductsByMember(long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.Products.Where(s => s.EsMemberId == memberId && s.IsEnable).OrderBy(s => s.Code).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Products>();
            }

        }
        private static Products TryGetProductsByCodeOrBarcode(string code, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var product = db.Products.
                        Include(s => s.Brands).
                        Include(s => s.ProductCategories).
                        Include(s => s.ProductGroup).
                        Where(s => s.EsMemberId == memberId && s.IsEnable && (s.Code == code || s.ProductGroup.Any(t => t.Barcode == code) || s.Barcode == code))
                        .OrderBy(s => s.ProductGroup.Count).FirstOrDefault();
                    return product;
                }
            }
            catch (Exception)
            {
                return null;
            }

        }
        private static Products TryGetProduct(Guid id, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products.
                                        Include(s => s.Brands).Include(s => s.ProductCategories).Include(s => s.ProductGroup).
                                        SingleOrDefault(s => s.EsMemberId == memberId && s.Id == id);
                }
                catch (Exception)
                {
                    return new Products();
                }

            }
        }
        private static List<Products> TryGetProducts(long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products.Include(s => s.ProductCategories).Include(s => s.ProductGroup).Where(s => s.EsMemberId == memberId && s.IsEnable).ToList();
                }
                catch (Exception)
                {
                    return new List<Products>();
                }

            }
        }
        private static List<ProductModel> TryGetProductsShortData(long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products.Where(s => s.EsMemberId == memberId && s.IsEnable).Select(p => new ProductModel()
                            {
                                Id = p.Id,
                                Description = p.Description,
                                Code = p.Code,
                                Barcode = p.Barcode,
                                Price = p.Price,
                                EsMemberId = p.EsMemberId,
                                IsEnabled = p.IsEnable
                            }).ToList();

                }
                catch (Exception)
                {
                    return new List<ProductModel>();
                }

            }
        }
        private static List<Products> TryGetRegisteredProducts(long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products.Include(s => s.ProductCategories).Include(s => s.ProductCategories).Include(s => s.ProductGroup).Where(s => s.EsMemberId == memberId).ToList();
                }
                catch (Exception)
                {
                    return new List<Products>();
                }

            }
        }
        private List<ProductModel> TryGetProductsBy(ProductViewType type, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    switch (type)
                    {
                        case ProductViewType.All:
                            return db.Products.Include(s => s.ProductCategories).Include(s => s.ProductGroup).Where(s => s.EsMemberId == memberId).Select(Convert).ToList();
                        case ProductViewType.ByActive:
                            return db.Products.Include(s => s.ProductCategories).Include(s => s.ProductGroup).Where(s => s.EsMemberId == memberId && s.IsEnable).Select(Convert).ToList();
                        case ProductViewType.ByPasive:
                            return db.Products.Include(s => s.ProductCategories).Include(s => s.ProductGroup).Where(s => s.EsMemberId == memberId && !s.IsEnable).Select(Convert).ToList();
                        case ProductViewType.ByEmpty:
                            return db.Products.Include(s => s.ProductCategories).Include(s => s.ProductGroup).Where(s => s.EsMemberId == memberId).Select(Convert).ToList();
                        case ProductViewType.WeigthsOnly:
                            return db.Products.Include(s => s.ProductCategories).Include(s => s.ProductGroup).Where(s => s.EsMemberId == memberId && s.IsWeight == true).Select(Convert).ToList();
                        default:
                            return db.Products.Include(s => s.ProductCategories).Include(s => s.ProductGroup).Where(s => s.EsMemberId == memberId).Select(Convert).ToList();
                    }
                }
                catch (Exception)
                {
                    return new List<ProductModel>();
                }

            }
        }
        private static DataTable TryGetProductTabletDataFromServer(long memberId)
        {
            DataTable dtDataTablesList = new DataTable();
            string NewconnectionString = string.Format("Data Source={0}; Initial Catalog={1}; Integrated Security={2}; User ID={3}; Password={4};", "87.241.191.72", "EsStockDB", "False", "sa", "kinutigkirqop");
            SqlConnection spContentConn = new SqlConnection(NewconnectionString);
            string sqlselectQuery = string.Format("select * from {0} where {1} = {2} order by {3}", "Products", ProductProperties.EsMemberId, memberId, ProductProperties.Code);
            try
            {
                spContentConn.Open();
                SqlCommand sqlCmd = new SqlCommand(sqlselectQuery, spContentConn);
                sqlCmd.CommandTimeout = 0;
                //sqlCmd.CommandType = CommandType.Text;
                sqlCmd.ExecuteNonQuery();
                SqlDataAdapter adptr = new SqlDataAdapter(sqlCmd);
                adptr.Fill(dtDataTablesList);
                spContentConn.Close();
            }
            catch (Exception ex)
            {
                return dtDataTablesList;
            }
            finally
            {
                if (spContentConn != null)
                    spContentConn.Dispose();
            }
            return dtDataTablesList;
        }
        private static List<ProductModel> TryGetExistingProduct(long memberId)
        {
            using (var db = GetDataContext())
            {
                return
                    db.ProductItems
                    .Include(s => s.Products.Brands)
                    .Where(s => s.MemberId == memberId && s.Quantity > 0)
                        .GroupBy(s => s.Products.Code).Select(p => new ProductModel()
                        {
                            Id = p.FirstOrDefault().Products.Id,
                            Description = p.FirstOrDefault().Products.Description,
                            Code = p.FirstOrDefault().Products.Code,
                            Barcode = p.FirstOrDefault().Products.Barcode,
                            Price = p.FirstOrDefault().Products.Price,
                            EsMemberId = p.FirstOrDefault().Products.EsMemberId,
                            IsEnabled = p.FirstOrDefault().Products.IsEnable
                        }).OrderBy(s => s.Code).ToList();

            }
        }
        private static bool TryEditProducts(List<Products> items)
        {
            using (var db = GetDataContext())
            {
                var memberId = ApplicationManager.Instance.GetEsMember.Id;
                var userId = ApplicationManager.GetEsUser.UserId;
                try
                {
                    foreach (var item in items)
                    {
                        if (db.Products.Count(s =>s.EsMemberId==memberId && (s.Code == item.Code || (!string.IsNullOrEmpty(s.Barcode) && s.Barcode == item.Barcode) || (!string.IsNullOrEmpty(item.Barcode) && s.ProductGroup.Any(t => t.Barcode == item.Barcode)))) > 1)
                        {
                            ApplicationManager.MessageManager.OnNewMessage(new MessageModel(string.Format("Barcode-ի կրկնություն։ Ապրանքի խմբագրումը չի իրականացել։ \n Կոդ։ {0} \nԲարկոդ: {1}", item.Code, item.Barcode), MessageModel.MessageTypeEnum.Warning));
                        }
                        var exItem = db.Products.SingleOrDefault(s => s.Code == item.Code && s.EsMemberId == memberId);
                        if (exItem != null)
                        {
                            exItem.Code = item.Code;
                            exItem.Barcode = item.Barcode;
                            exItem.HCDCS = item.HCDCS;
                            exItem.Description = item.Description;
                            exItem.Mu = item.Mu;
                            exItem.IsWeight = item.IsWeight;
                            exItem.Note = item.Note;
                            exItem.CostPrice = item.CostPrice;
                            exItem.ExpiryDays = item.ExpiryDays;
                            if (exItem.Price != item.Price)
                            {
                                exItem.OldPrice = exItem.Price;
                            }
                            exItem.Price = item.Price;
                            exItem.Discount = item.Discount;
                            exItem.DealerPrice = item.DealerPrice;
                            exItem.DealerDiscount = item.DealerDiscount;
                            exItem.ImagePath = item.ImagePath;
                            exItem.IsEnable = item.IsEnable;
                            exItem.BrandId = item.BrandId;
                            exItem.LastModifierId = item.LastModifierId;
                        }
                        else
                        {
                            item.Id = Guid.NewGuid();
                            item.EsMemberId = memberId;
                            item.LastModifierId = userId;
                            db.Products.Add(item);
                        } 
                        db.SaveChanges();
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        private static bool TryInsertProduct(Products item)
        {
            using (var db = GetDataContext())
            {
                if (db.Products.Count(s => s.Code == item.Code || (!string.IsNullOrEmpty(s.Barcode) && s.Barcode == item.Barcode) || (!string.IsNullOrEmpty(item.Barcode) && s.ProductGroup.Any(t => t.Barcode == item.Barcode))) > 1)
                {
                    ApplicationManager.MessageManager.OnNewMessage(new MessageModel(string.Format("Barcode-ի կրկնություն։ Ապրանքի խմբագրումը չի իրականացել։ \n Կոդ։ {0} \nԲարկոդ: {1}", item.Code, item.Barcode), MessageModel.MessageTypeEnum.Warning));
                    return false;
                }
                var exItem = db.Products.SingleOrDefault(s => s.Code == item.Code && s.EsMemberId == item.EsMemberId);
                if (exItem != null)
                {
                    if (string.IsNullOrEmpty(item.Barcode) || !string.IsNullOrEmpty(exItem.Barcode)) return false;
                    exItem.Barcode = item.Barcode;
                    exItem.Price = null;
                }
                else
                {
                    db.Products.Add(item);
                }
                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        private static Products TryEditProduct(Products item)
        {
            if (item == null) return null;
            try
            {
                using (var db = GetDataContext())
                {
                    if (db.Products.Count(s => (s.Code == item.Code || (!string.IsNullOrEmpty(item.Barcode) && s.Barcode == item.Barcode) || s.Id == item.Id) && s.EsMemberId == item.EsMemberId) > 1)
                    {
                        MessageBox.Show("Code-ը կամ Barcode-ը կրկնվում է։ Գործողությունը հնարավոր չէ շարունակել։");
                        return null;
                    }
                    var exItem = db.Products.SingleOrDefault(s => s.Code == item.Code && s.EsMemberId == item.EsMemberId);
                    if (exItem != null && exItem.Id != item.Id)
                    {
                        MessageBox.Show(
                            "Գործողությունը դադարեցված է։ \nԱպրանքի կոդն արդեն գրանցված է։ Խնդրում ենք փոխել կոդը և նորից փորձել։",
                            "Թերի տվյալներ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return null;
                    }
                    var exItemsByBarcode = db.Products.Where(s => s.EsMemberId == item.EsMemberId && (s.Code == item.Code || (!string.IsNullOrEmpty(item.Barcode) && s.Barcode == item.Barcode))).ToList();
                    if (exItemsByBarcode != null &&
                        (exItemsByBarcode.Count > 1 || (exItem == null && exItemsByBarcode.Count == 1)))
                    {
                        MessageBox.Show(
                           "Գործողությունը դադարեցված է։ \nԱպրանքի բարկոդը կրկնվում է։ Խնդրում ենք փոխել բարկոդը և նորից փորձել։",
                           "Թերի տվյալներ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return null;
                    }
                    if (exItem != null)
                    {
                        if (exItem.Code != item.Code) return null;
                        exItem.Barcode = item.Barcode;
                        exItem.HCDCS = item.HCDCS;
                        exItem.Description = item.Description;
                        exItem.Mu = item.Mu;
                        exItem.IsWeight = item.IsWeight;
                        exItem.Note = item.Note;
                        exItem.CostPrice = item.CostPrice;
                        if (exItem.Price != item.Price)
                        {
                            exItem.OldPrice = exItem.Price;
                        }
                        exItem.Price = item.Price;
                        exItem.Discount = item.Discount;
                        exItem.DealerPrice = item.DealerPrice;
                        exItem.DealerDiscount = item.DealerDiscount;
                        exItem.MinQuantity = item.MinQuantity;
                        exItem.ExpiryDays = item.ExpiryDays;
                        exItem.ImagePath = item.ImagePath;
                        exItem.IsEnable = item.IsEnable;
                        exItem.BrandId = item.BrandId;
                        exItem.LastModifierId = item.LastModifierId;
                    }
                    else
                    {
                        //item.Id = Guid.NewGuid();
                        db.Products.Add(item);
                    }
                    db.SaveChanges();
                    var exProductGrups = db.ProductGroup.Where(s => s.ProductId == item.Id && s.MemberId == item.EsMemberId).ToList();
                    foreach (var exProductGrup in exProductGrups)
                    {
                        db.ProductGroup.Remove(exProductGrup);
                    }
                    foreach (var productItem in item.ProductGroup)
                    {
                        db.ProductGroup.Add(productItem);
                    }
                    db.SaveChanges();
                    return item;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        private static bool TryDeleteProduct(Products item)
        {
            return item != null && TryDeleteProduct(item.Id, item.EsMemberId);
        }
        private static bool TryDeleteProduct(Guid id, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var exProduct = db.Products.SingleOrDefault(s => s.Id == id && s.EsMemberId == memberId);
                    if (exProduct == null) return false;
                    exProduct.IsEnable = false;
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }

            }
            return true;
        }
        private static List<ProductItems> TryGetAllProductItems(long memberId)
        {
            using (var db = GetDataContext())
            {
                return db.ProductItems
                    .Include(s => s.Products)
                    .Where(s => s.MemberId == memberId).ToList();
            }
        }
        private static List<ProductItems> TryGetProductItems(long memberId)
        {
            var db = GetDataContext();
            try
            {
                return db.ProductItems.Include(s => s.Products).Include(s => s.Products.ProductGroup).Include(s => s.Products.ProductCategories).Where(s => s.MemberId == memberId && s.Quantity != 0).ToList();
            }
            catch (Exception)
            {
                return new List<ProductItems>();
            }

        }
        private static List<ProductItems> TryGetProductItemsByStock(long stockId, long memberId)
        {
            var db = GetDataContext();
            try
            {
                return db.ProductItems
                    .Include(s => s.Products)
                    .Include(s => s.Products.ProductGroup)
                    .Include(s => s.Products.ProductCategories)
                    .Where(s => s.MemberId == memberId && s.StockId == stockId && s.Quantity != 0).ToList();
            }
            catch (Exception)
            {
                return new List<ProductItems>();
            }

        }
        private static List<ProductItems> TryGetProductItemsForInvocies(IEnumerable<Guid> invoiceIds, long memberId)
        {
            using (var db = GetDataContext())
            {
                return db.InvoiceItems
                    .Where(s => s.Invoices.MemberId == memberId && invoiceIds.Contains(s.InvoiceId)).Select(s => s.ProductItems).ToList();
            }
        }
        private static List<ProductItems> TryGetProductItemsFromStocks(List<long> stockIds, long memberId)
        {
            using (var db = GetDataContext())
            {
                return db.ProductItems
                    .Include(s => s.Products)
                    .Where(s => s.MemberId == memberId && s.Quantity != 0 && s.StockId != null && stockIds.Contains((long)s.StockId)).ToList();
            }
        }
        private static List<ProductItems> TryGetUnavilableProductItems(List<Guid> productIds, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.ProductItems.Include(s => s.Products).Where(s => s.MemberId == memberId && s.Quantity != 0 && !productIds.Contains(s.ProductId)).ToList();
                }
                catch (Exception)
                {
                    MessageBox.Show("Կապի խափանում", "Հարցման սխալ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return new List<ProductItems>();
                }
            }
        }
        private static List<ProductResidue> TryGetProductResidue(long memberId)
        {
            var db = GetDataContext();
            try
            {
                return db.ProductItems.Where(s => s.MemberId == memberId && s.Quantity != 0).GroupBy(pi => pi.ProductId).
                    Select(pi => new ProductResidue { ProductId = pi.FirstOrDefault().ProductId, Quantity = pi.Sum(s => s.Quantity) }).ToList();
            }
            catch (Exception)
            {
                return new List<ProductResidue>();
            }

        }
        private static long TryGetProductCount(long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products.Count(s => s.EsMemberId == memberId);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        private static List<ProductProvider> TryGetProductsProviders(List<Guid> productIds)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var invoceiItems = db.InvoiceItems.Where(s => s.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice && productIds.Contains(s.ProductId) && s.Invoices.PartnerId != null)
                        .OrderByDescending(s => s.Invoices.CreateDate).GroupBy(s => s.ProductId);
                    var items = new List<ProductProvider>();
                    foreach (var invoceiItem in invoceiItems)
                    {
                        if (invoceiItem.FirstOrDefault() == null || invoceiItem.First().Invoices == null) { continue; }
                        items.Add(new ProductProvider { ProductId = invoceiItem.First().ProductId, ProviderId = ((Guid)invoceiItem.First().Invoices.PartnerId) });
                    }
                    return items;
                }
                catch (Exception)
                {
                    MessageBox.Show("Կապի խափանում", "Հարցման սխալ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return new List<ProductProvider>();
                }
            }
        }
        private static int TryGetNextProductCode(long memberId)
        {
            var nextcode = 10000;
            using (var db = GetDataContext())
            {
                try
                {
                    var items = db.Products.Where(s => s.EsMemberId == memberId);
                    nextcode += items.Count();
                    while (items.Any(s => s.Code == nextcode.ToString()))
                    {
                        nextcode--;
                    }
                }
                catch (Exception)
                {
                }
                return nextcode;
            }
        }
        #endregion

        #endregion

        #region Categories
        public static List<EsCategoriesModel> GetEsCategories()
        {
            return Convert(TryGetEsCategories());
        }
        private static List<EsCategoriesModel> Convert(List<EsCategories> list)
        {
            if (list == null) return null;
            var categories = new List<EsCategoriesModel>();
            foreach (var item in list.Where(s => s.ParentId == null).ToList())
            {
                var subCategory = Convert(item);
                Convert(subCategory, list.Where(s => s.ParentId != null).ToList());
                categories.Add(subCategory);
            }
            return categories;
        }
        private static EsCategories Convert(EsCategoriesModel category)
        {
            if (category == null) return null;
            return new EsCategories()
            {
                Id = category.Id,
                ParentId = category.ParentId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                HCDCS = category.HcDcs,
                LastModificationDate = category.LastModificationDate,
                Logo = category.Logo,
                LastModifierId = category.LastModifierId
            };
        }

        private static void Convert(EsCategoriesModel parent, List<EsCategories> list)
        {
            foreach (var item in list.Where(s => s.ParentId == parent.Id).ToList())
            {
                var subCategory = Convert(item);
                Convert(subCategory, list.Where(s => s.ParentId != item.Id).ToList());
                item.ParentId = parent.Id;
                parent.Children.Add(subCategory);
            }
        }
        private static EsCategoriesModel Convert(EsCategories item)
        {
            if (item == null) return null;
            return new EsCategoriesModel
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Name = item.Name,
                Description = item.Description,
                IsActive = item.IsActive,
                HcDcs = item.HCDCS,
                LastModificationDate = item.LastModificationDate,
                Logo = item.Logo,
                LastModifierId = item.LastModifierId
            };
        } 
        private static List<EsCategories> TryGetEsCategories()
        {
            using (var db = GetServerDataContext())
            {
                try
                {
                    return db.EsCategories.Include("EsCategories1").Include("EsCategories2").OrderBy(s => s.HCDCS).ToList();
                }
                catch (Exception)
                {
                    return new List<EsCategories>();
                }
            }
        }
        #endregion Categories
    }
}
