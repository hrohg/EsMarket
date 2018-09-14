using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using CashReg.Helper;
using CashReg.Managers;
using EsMarket.SharedData.Models;
using ES.Business.Helpers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Data.Models;
using ES.Data.Models.EsModels;
using ES.Data.Models.Reports;
using ES.DataAccess.Models;
using DataTable = System.Data.DataTable;
using ProductModel = ES.Data.Models.ProductModel;

namespace ES.Business.Managers
{
    public class ProductsManager : BaseManager
    {

        private static long MemberId { get { return ApplicationManager.Member.Id; } }
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
            exItem.LastModifiedDate = item.LastModifiedDate;
            exItem.ProductGroups = item.ProductGroups;
            exItem.TypeOfTaxes = item.TypeOfTaxes;
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
            product.LastModifiedDate = item.LastModifiedDate;
            product.ProductGroups = item.ProductGroups;
            product.TypeOfTaxes = item.TypeOfTaxes;
            product.IsEnabled = item.IsEnabled;

            //Additional data
            product.TypeOfTaxes = item.TypeOfTaxes;
        }
        public List<ProductModel> Convert(List<Products> items)
        {
            return items.Select(Convert).ToList();
        }
        public static ProductModel Convert(Products item)
        {
            try
            {
                if (item == null) return null;
                var exItem = new ProductModel(item.EsMemberId, item.LastModifierId, item.IsEnable);
                exItem.Id = item.Id;
                exItem.Code = item.Code;
                exItem.Barcode = item.Barcode;
                exItem.HcdCs = item.HCDCS;
                exItem.Description = item.Description;
                exItem.Mu = item.Mu;
                exItem.IsWeight = item.IsWeight ?? false;
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
                exItem.LastModifiedDate = item.LastModifiedDate;
                exItem.ProductCategories = Convert(item.ProductCategories);
                exItem.ProductGroups = Convert(item.ProductGroup.ToList());
                //_products.Add(exItem.Id, exItem);

                //Additional data
                exItem.TypeOfTaxes = item.ProductsAdditionalData != null && item.ProductsAdditionalData.TypeOfTaxes != null ? (TypeOfTaxes)item.ProductsAdditionalData.TypeOfTaxes : default(TypeOfTaxes);
                return exItem;
            }
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.Message, MessageTypeEnum.Warning);
                return null;
            }
        }
        public static ProductModel ConvertLight(Products item)
        {
            if (item == null) return null;
            var exItem = new ProductModel(item.EsMemberId, item.LastModifierId, item.IsEnable);
            exItem.Id = item.Id;
            exItem.Code = item.Code;
            exItem.Barcode = item.Barcode;
            exItem.HcdCs = item.HCDCS;
            exItem.Description = item.Description;
            exItem.Mu = item.Mu;
            exItem.IsWeight = item.IsWeight ?? false;
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
            exItem.LastModifiedDate = item.LastModifiedDate;
            //_products.Add(exItem.Id, exItem);

            exItem.TypeOfTaxes = item.ProductsAdditionalData != null && item.ProductsAdditionalData.TypeOfTaxes != null ? (TypeOfTaxes)item.ProductsAdditionalData.TypeOfTaxes : default(TypeOfTaxes);

            return exItem;
        }
        public static ProductModel Convert(EsGood item)
        {
            var exProducts = ApplicationManager.Instance.CashManager.Products.Where(s => s.Barcode == item.Barcode || s.Code == item.Code).ToList();
            if (exProducts.Count > 0)
            {
                return null;
            }
            if (item == null) return null;
            var newItem = new ProductModel(ApplicationManager.Member.Id, ApplicationManager.GetEsUser.UserId, true)
            {
                Id = exProducts.Any() ? exProducts.First().Id : Guid.NewGuid(),
                Code = item.Code,
                Barcode = item.Barcode,
                HcdCs = item.HcdCs,
                Description = item.Description,
                Mu = item.Unit,
                CostPrice = item.CostPrice,
                OldPrice = exProducts.Any() ? exProducts.First().Price : null,
                Price = item.Price,
                DealerPrice = item.DealerPrice
            };

            return newItem;
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
            exItem.LastModifiedDate = item.LastModifiedDate;
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
        public ProductModel GetProductsByCodeOrBarcode(string code)
        {
            return Convert(TryGetProductsByCodeOrBarcode(code));
        }
        public static ProductModel GetProduct(Guid id)
        {
            return Convert(TryGetProduct(id));
        }

        public static bool IsProductExists(Guid id)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products.SingleOrDefault(s => s.EsMemberId == ApplicationManager.Member.Id && s.Id == id) != null;
                }
                catch (Exception)
                {
                    return false;
                }

            }
        }
        public List<ProductModel> GetExistingProduct(long memberId)
        {
            return TryGetExistingProduct(memberId);
        }
        public static List<ProductModel> GetProducts()
        {
            return TryGetProducts().Select(Convert).ToList();
        }
        public static List<ProductModel> GetChangedProducts(int days)
        {
            return TryGetChangedProducts(days).Select(ConvertLight).ToList();
        }
        public List<ProductModel> GetProductsShortData(long memberId)
        {
            return TryGetProductsShortData(memberId);
        }
        public List<ProductModel> GetRegisteredProducts(long memberId)
        {
            return TryGetRegisteredProducts(memberId).Select(Convert).ToList();
        }
        public List<ProductModel> GetProductsBy(ProductViewType type)
        {
            return TryGetProductsBy(type);

        }
        public static ProductModel EditProduct(ProductModel product)
        {
            var exProduct = Convert(product);
            var productAdditionalData = new ProductsAdditionalData { ProductId = product.Id };
            productAdditionalData.TypeOfTaxes = product.TypeOfTaxes > 0 ? (int)product.TypeOfTaxes : (int?)null;

            return Convert(TryEditProduct(exProduct, productAdditionalData));
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
        public static decimal GetProductItemQuantity(Guid? productId, List<long> fromStocks)
        {
            return TryGetProductItemCount(productId, fromStocks);
        }
        public static decimal GetProductItemCount(Guid? productId, long memberId)
        {
            return TryGetProductItemCount(productId, memberId);
        }
        public static decimal GetProductItemCountFromStock(Guid? productId, List<long> stockIds, long memeberId)
        {
            return TryGetProductItemCountFromStock(productId, stockIds, memeberId);
        }
        public static List<ProductItemModel> GetProductItems()
        {
            try
            {
                var productItems = TryGetProductItems();
                var products = productItems.GroupBy(s => s.Products).Select(s => Convert(s.Key)).ToList();
                return productItems.Select(s => Convert(s, products)).ToList();
            }
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error);
                return new List<ProductItemModel>();
            }

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
            var products = GetProducts();
            return TryGetProductItemsForInvocies(invoiceIds, memberId).Select(s => Convert(s, products)).ToList();
        }
        public List<ProductItemModel> GetAllProductItems(long memberId)
        {
            var products = GetProducts();
            return TryGetAllProductItems(memberId).Select(s => Convert(s, products)).ToList();
        }
        public static List<ProductItemModel> GetProductItemsFromStocks(List<long> stockIds)
        {
            var products = GetProducts();
            return TryGetProductItemsFromStocks(stockIds).Select(s => Convert(s, products)).ToList();
        }
        public List<ProductItemModel> GetUnavailableProductItems(List<Guid> productItemIds, List<long> stockIds)
        {
            var products = GetProducts();
            return TryGetUnavilableProductItems(productItemIds, stockIds).Select(s => Convert(s, products)).ToList();
        }
        public static long GetProductCount(long memberId)
        {
            return TryGetProductCount(memberId);
        }
        public static List<ProductOrderModel> GetProductOrderByBrands(List<Brands> brands, long memberId)
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
                        products.Select(s => new ProductOrderModel
                    {
                        Code = s.Code,
                        Description = s.Description,
                        Price = s.Price,
                        MinQuantity = s.MinQuantity,
                        ExistingQuantity = productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity),
                        Provider = s.InvoiceItems.First().Invoices.ProviderName

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
        public static List<ProductOrderModel> GetProductOrderByProduct()
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    var products = db.Products.Where(s => s.EsMemberId == memberId && s.IsEnable && s.MinQuantity != null);

                    var productIds = products.Select(t => t.Id);
                    var productItems = db.ProductItems.Where(s => s.MemberId == memberId && productIds.Contains(s.ProductId));
                    products = products.Where(s => s.MinQuantity > productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity));

                    var productOrderItems = products
                        .Select(p => new
                        {
                            p,
                            ii = db.InvoiceItems
                                .Where(ii =>
                                ii.Invoices.MemberId == memberId &&
                                ii.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice &&
                                ii.Invoices.ApproveDate != null &&
                                ii.ProductId == p.Id)
                                .OrderByDescending(ii => ii.Invoices.CreateDate).FirstOrDefault()
                        })
                        .Select(s => new ProductOrderModel
                        {
                            Index = 0,
                            ProductId = s.p.Id,
                            Code = s.p.Code,
                            Description = s.p.Description,
                            Mu = s.p.Mu,
                            CostPrice = s.p.CostPrice,
                            Price = s.p.Price,
                            MinQuantity = s.p.MinQuantity,
                            ExistingQuantity = productItems.Where(t => t.ProductId == s.p.Id).Sum(t => t.Quantity),
                            Provider = s.ii != null ? s.ii.Invoices.ProviderName : string.Empty
                            //Notes = s.Note
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
            if (products != null)
            {
                exItem.Product = products.SingleOrDefault(s => s.Id == item.ProductId);
            }
            if (exItem.Product == null)
            {
                exItem.Product = ConvertLight(item.Products);
            }
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
        public static ProductItemModel Convert(ProductItems item)
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
        private static decimal TryGetProductItemCount(Guid? productId, List<long> fromStocks)
        {
            if (!productId.HasValue || fromStocks == null) return 0;
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    var items = db.ProductItems.Where(s =>
                            s.MemberId == memberId && s.ProductId == productId.Value && s.StockId != null && fromStocks.Contains(s.StockId.Value));
                    return items.Sum(s => s.Quantity);
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
        private static Products TryGetProductsByCodeOrBarcode(string code)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var product = db.Products.
                        Include(s => s.Brands).
                        Include(s => s.ProductCategories).
                        Include(s => s.ProductGroup).
                        Include(s => s.ProductsAdditionalData).
                        Where(s => s.EsMemberId == ApplicationManager.Member.Id && s.IsEnable && (s.Code == code || s.ProductGroup.Any(t => t.Barcode == code) || s.Barcode == code))
                        .OrderBy(s => s.ProductGroup.Count).FirstOrDefault();
                    return product;
                }
            }
            catch (Exception)
            {
                return null;
            }

        }
        private static Products TryGetProduct(Guid id)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products
                        .Include(s => s.Brands)
                                        .Include(s => s.ProductCategories)
                                        .Include(s => s.ProductGroup)
                                        .Include(s => s.ProductsAdditionalData).
                                        SingleOrDefault(s => s.EsMemberId == ApplicationManager.Member.Id && s.Id == id);
                }
                catch (Exception)
                {
                    return new Products();
                }

            }
        }
        private static List<Products> TryGetProducts()
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products
                        .Include(s => s.ProductCategories)
                        .Include(s => s.ProductGroup)
                        .Include(s => s.ProductsAdditionalData)
                        .Where(s => s.EsMemberId == memberId && s.IsEnable).ToList();
                }
                catch (Exception)
                {
                    return new List<Products>();
                }

            }
        }
        private static List<Products> TryGetChangedProducts(int days)
        {
            var memberId = ApplicationManager.Member.Id;
            var date = DateTime.Today.AddDays(-days);
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products
                        .Include(s => s.ProductCategories)
                        .Include(s => s.ProductGroup)
                        .Include(s => s.ProductsAdditionalData)
                        .Where(s => s.EsMemberId == memberId && s.IsEnable && s.LastModifiedDate >= date).ToList();
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
                    return db.Products
                        .Include(s => s.ProductCategories)
                        .Include(s => s.ProductCategories)
                        .Include(s => s.ProductGroup)
                        .Include(s => s.ProductsAdditionalData)
                        .Where(s => s.EsMemberId == memberId).ToList();
                }
                catch (Exception)
                {
                    return new List<Products>();
                }

            }
        }
        private List<ProductModel> TryGetProductsBy(ProductViewType type)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var memberId = ApplicationManager.Member.Id;
                    switch (type)
                    {
                        case ProductViewType.All:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductGroup)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId).Select(Convert).ToList();
                        case ProductViewType.ByActive:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductGroup)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId && s.IsEnable).Select(Convert).ToList();
                        case ProductViewType.ByPasive:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductGroup)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId && !s.IsEnable).Select(Convert).ToList();
                        case ProductViewType.ByEmpty:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductGroup)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId).Select(Convert).ToList();
                        case ProductViewType.WeigthsOnly:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductGroup)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId && s.IsWeight == true).Select(Convert).ToList();
                        default:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductGroup)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId).Select(Convert).ToList();
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
                var memberId = ApplicationManager.Instance.GetMember.Id;
                var userId = ApplicationManager.GetEsUser.UserId;
                try
                {
                    foreach (var item in items)
                    {
                        if (db.Products.Count(s => s.EsMemberId == memberId && (s.Code == item.Code || (!string.IsNullOrEmpty(s.Barcode) && s.Barcode == item.Barcode) || (!string.IsNullOrEmpty(item.Barcode) && s.ProductGroup.Any(t => t.Barcode == item.Barcode)))) > 1)
                        {
                            MessageManager.OnMessage(string.Format("Barcode-ի կրկնություն։ Ապրանքի խմբագրումը չի իրականացել։ \n Կոդ։ {0} \nԲարկոդ: {1}", item.Code, item.Barcode), MessageTypeEnum.Warning);
                        }
                        var exItem = db.Products.SingleOrDefault(s => s.Code == item.Code && s.EsMemberId == memberId);


                        if (exItem != null)
                        {
                            exItem.LastModifiedDate = exItem.LastModifiedDate.AddMilliseconds(-exItem.LastModifiedDate.Millisecond);
                            if (exItem.LastModifiedDate > item.LastModifiedDate)
                            {
                                MessageManager.OnMessage(string.Format("Ապրանքն ավելի վաղ արդեն խմբագրվել է։ Ապրանքի խմբագրումը չի իրականացել։ \n Կոդ։ {0} \nԲարկոդ: {1}", item.Code, item.Barcode), MessageTypeEnum.Warning);
                                continue;
                            }
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
                            exItem.LastModifierId = userId;
                            exItem.LastModifiedDate = item.LastModifiedDate = DateTime.Now;
                        }
                        else
                        {
                            item.Id = Guid.NewGuid();
                            item.EsMemberId = memberId;
                            item.LastModifierId = userId;
                            item.LastModifiedDate = DateTime.Now;
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
                    MessageManager.OnMessage(string.Format("Barcode-ի կրկնություն։ Ապրանքի խմբագրումը չի իրականացել։ \n Կոդ։ {0} \nԲարկոդ: {1}", item.Code, item.Barcode), MessageTypeEnum.Warning);
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
                    item.LastModifiedDate = DateTime.Now;
                    if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
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
        private static Products TryEditProduct(Products item, ProductsAdditionalData productAdditionalData)
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
                    var exItem = db.Products.Include(s => s.ProductsAdditionalData).SingleOrDefault(s => s.Code == item.Code && s.EsMemberId == item.EsMemberId);
                    if (exItem != null)
                    {
                        if (exItem.Id != item.Id && item.Id != Guid.Empty)
                        {
                            MessageBox.Show("Գործողությունը դադարեցված է։ \nԱպրանքի կոդն արդեն գրանցված է։ Խնդրում ենք փոխել կոդը և նորից փորձել։",
                                "Թերի տվյալներ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null;
                        }
                        exItem.LastModifiedDate = exItem.LastModifiedDate.AddMilliseconds(-exItem.LastModifiedDate.Millisecond);
                        if (exItem.LastModifiedDate > item.LastModifiedDate)
                        {
                            MessageManager.OnMessage("Գործողությունը դադարեցված է։ \nԱպրանքն ավելի վաղ արդեն խմբագրվել է։", MessageTypeEnum.Warning);
                            return null;
                        }
                    }

                    var exItemsByBarcode = db.Products.Where(s => s.EsMemberId == item.EsMemberId && (s.Code == item.Code || (!string.IsNullOrEmpty(item.Barcode) && s.Barcode == item.Barcode))).ToList();
                    if ((exItemsByBarcode.Count > 1 || (exItem == null && exItemsByBarcode.Count == 1)))
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
                        exItem.LastModifiedDate = item.LastModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        //item.Id = Guid.NewGuid();
                        item.LastModifiedDate = DateTime.Now;
                        if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
                        db.Products.Add(item);
                    }
                    if (productAdditionalData != null)
                    {

                        if (exItem != null && exItem.ProductsAdditionalData != null)
                        {
                            exItem.ProductsAdditionalData.TypeOfTaxes = productAdditionalData.TypeOfTaxes;
                        }
                        else
                        {
                            if (productAdditionalData.ProductId == item.Id && productAdditionalData.TypeOfTaxes > 0)
                            {
                                db.ProductsAdditionalData.Add(productAdditionalData);
                                item.ProductsAdditionalData = productAdditionalData;
                            }
                        }
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
        private static List<ProductItems> TryGetProductItems()
        {
            var db = GetDataContext();
            try
            {
                return db.ProductItems.Include(s => s.Products)
                    .Include(s => s.Products.ProductGroup)
                    .Include(s => s.Products.ProductCategories)
                    .Where(s => s.MemberId == ApplicationManager.Instance.GetMember.Id && s.Quantity != 0).ToList();
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
                    .Include(s => s.Products.ProductsAdditionalData)
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
        private static List<ProductItems> TryGetProductItemsFromStocks(List<long> stockIds)
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                return db.ProductItems
                    .Include(p => p.Products)
                    .Where(pi => pi.MemberId == memberId && pi.Quantity != 0 && pi.StockId != null && stockIds.Contains((long)pi.StockId))
                    .Join(db.Invoices.Where(i => i.MemberId == memberId), pi => pi.DeliveryInvoiceId, ii => ii.Id, (pi, ii) => new { pi, ii })
                    .OrderBy(s => s.ii.CreateDate)
                    .Select(s => s.pi).ToList();
            }
        }
        private static List<ProductItems> TryGetUnavilableProductItems(List<Guid> productIds, List<long> stockIds)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.ProductItems.Include(s => s.Products).Where(s => s.MemberId == ApplicationManager.Member.Id && s.Quantity != 0 && !productIds.Contains(s.ProductId) && stockIds.Contains(s.StockId ?? 0)).ToList();
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
                    var items = db.Products.Where(s => s.EsMemberId == memberId).Select(s => s.Code).ToList();
                    nextcode += items.Count;
                    var code = string.Format("{0}{1}", ApplicationManager.Settings.SettingsContainer.MemberSettings.UseShortCode ? "" : ApplicationManager.Member.Id.ToString("D2"), nextcode);
                    while (items.Any(s => s == code))
                    {
                        nextcode--;
                        code = string.Format("{0}{1}", ApplicationManager.Settings.SettingsContainer.MemberSettings.UseShortCode ? "" : ApplicationManager.Member.Id.ToString("D2"), nextcode);

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
                Convert(subCategory, list.Where(s => s.ParentId != parent.Id).ToList());
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

        public static List<ProductModel> GetFallowProductItems(int days)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    DateTime date = DateTime.Today.AddDays(-days);
                    var items = db.ProductItems.Where(s => s.MemberId == MemberId && s.Quantity > 0).Include(s => s.Products)
                        .Join(db.Invoices.Where(i => i.MemberId == MemberId), s => s.DeliveryInvoiceId, i => i.Id, (s, i) => new { s, i })
                        .Where(t => t.i.InvoiceTypeId != (int)InvoiceType.SaleInvoice);
                    var groupItems = items
                        .Include(t => t.s.Products)
                    .Include(t => t.s.Products.ProductGroup)
                    .Include(t => t.s.Products.ProductCategories)
                    .Include(t => t.s.Products
                        .ProductsAdditionalData).GroupBy(t => t.s.ProductId).Where(t => t.All(x => x.i.ApproveDate < date)).ToList();
                    var products = new List<ProductModel>();
                    foreach (var productItems in groupItems)
                    {
                        var product = Convert(productItems.First().s.Products);
                        product.ExistingQuantity = productItems.Sum(t => t.s.Quantity);
                        products.Add(product);
                    }
                    return products;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static List<ProductModel> GetMissingProductItems(long stockId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var items = db.ProductItems.Where(s => s.MemberId == MemberId)
                        .Include(s => s.Products)
                        .Join(db.Invoices.Where(i => i.MemberId == MemberId), s => s.DeliveryInvoiceId, pi => pi.Id, (pi, i) => new { pi, i });

                    var groupItems = items
                        .Include(t => t.pi.Products)
                        .Include(t => t.pi.Products.ProductGroup)
                        .Include(t => t.pi.Products.ProductCategories)
                        .Include(t => t.pi.Products
                        .ProductsAdditionalData)
                        .GroupBy(t => t.pi.ProductId)
                        .Where(t => t.GroupBy(x => x.pi.StockId).Any(x => x.Any(s => s.i.InvoiceTypeId == (int)InvoiceType.SaleInvoice) && x.Sum(s => s.pi.Quantity) == 0)).ToList();
                    var products = new List<ProductModel>();
                    foreach (var productItems in groupItems)
                    {
                        var product = Convert(productItems.First().pi.Products);

                        products.Add(product);
                    }
                    return products;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public static List<ProductModel> CheckProductRemainderByStockItems(long stockId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var items = db.ProductItems.Where(s => s.MemberId == MemberId)
                        .Include(s => s.Products)
                        .Join(db.Invoices.Where(i => i.MemberId == MemberId), s => s.DeliveryInvoiceId, pi => pi.Id, (pi, i) => new { pi, i });

                    //items = items.Where(s => items.Where(t => t.pi.StockId == stockId && t.pi.ProductId == s.pi.ProductId).Sum(t => t.pi.Quantity) == 0 && items.Sum(t => t.pi.Quantity) > 0);

                    //var items1 = items
                    //    .Include(t => t.pi.Products.ProductGroup)
                    //    .Include(t => t.pi.Products.ProductCategories)
                    //    .Include(t => t.pi.Products
                    //    .ProductsAdditionalData).ToList();

                    //var groupItems = items.Where(s => s.i.ApproveDate > date).GroupBy(t => t.pi.ProductId).Where(s => s.Sum(t => t.pi.Quantity) > 0).ToList();

                    var list = items.GroupBy(s => s.pi.ProductId).Where(s => s.Any(t => t.pi.StockId == stockId)).ToList();
                    list = list.Where(s => s.Where(t => t.pi.StockId == stockId).Sum(t => t.pi.Quantity) == 0 &&
                                           s.Where(t => t.pi.StockId != stockId).Sum(t => t.pi.Quantity) > 0).ToList();

                    var products = list.Select(s => s.First().pi.Products).Select(s => new ProductModel
                    {
                        Code = s.Code,
                        Description = s.Description,
                        Mu = s.Mu,
                        ExistingQuantity = list.First(t => t.First().pi.ProductId == s.Id).Sum(t => t.pi.Quantity)
                    }).ToList();
                    return products;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public static List<ProductModel> CheckProductRemainderItems(long stockId, int days = 120)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var items = db.ProductItems.Where(s => s.MemberId == MemberId)
                        .Include(s => s.Products)
                        .Join(db.Invoices.Where(i => i.MemberId == MemberId), s => s.DeliveryInvoiceId, pi => pi.Id, (pi, i) => new { pi, i });

                    //items = items.Where(s => items.Where(t => t.pi.StockId == stockId && t.pi.ProductId == s.pi.ProductId).Sum(t => t.pi.Quantity) == 0 && items.Sum(t => t.pi.Quantity) > 0);

                    //var items1 = items
                    //    .Include(t => t.pi.Products.ProductGroup)
                    //    .Include(t => t.pi.Products.ProductCategories)
                    //    .Include(t => t.pi.Products
                    //    .ProductsAdditionalData).ToList();

                    var date = DateTime.Today.AddDays(-days);
                    //var groupItems = items.Where(s => s.i.ApproveDate > date).GroupBy(t => t.pi.ProductId).Where(s => s.Sum(t => t.pi.Quantity) > 0).ToList();

                    var list = items.GroupBy(s => s.pi.ProductId).Where(s => s.Any(t => t.pi.StockId == stockId)).ToList();
                    list = list.Where(s => s.Any(t => t.pi.StockId == stockId && t.i.ApproveDate >= date) && s.Where(t => t.pi.StockId == stockId).Sum(t => t.pi.Quantity) == 0 &&
                                          s.Where(t => t.pi.StockId != stockId).Sum(t => t.pi.Quantity) > 0).ToList();

                    var products = list.Select(s => s.First().pi.Products).Select(s => new ProductModel
                    {
                        Code = s.Code,
                        Description = s.Description,
                        Mu = s.Mu,
                        ExistingQuantity = list.First(t => t.First().pi.ProductId == s.Id).Sum(t => t.pi.Quantity)
                    }).ToList();
                    return products;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static List<IInvoiceReport> GetProductsBalance(DateTime? date)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var pi = db.ProductItems.Include(s => s.Products).Where(s => s.Quantity > 0);
                    return new List<IInvoiceReport>(){ new InvoiceReport
                    {
                        Description = "Stock",
                        Count = pi.GroupBy(t => t.ProductId).Count(),
                        Quantity = pi.Sum(t=>t.Quantity),
                        Cost = pi.Sum(t => t.Quantity*t.CostPrice),
                        Price = pi.Sum(t => t.Quantity*(t.Products.Price ?? 0)),
                        Sale = pi.Sum(t => t.Quantity*((t.Products.Price ?? 0)-t.CostPrice)),
                    }};
                }
                catch (Exception)
                {
                    return new List<IInvoiceReport>();
                }
            }
        }

        public static List<IInvoiceReport> GetProductsByStock(DateTime? date, List<StockModel> stocks)
        {
            using (var db = GetDataContext())
            {
                var result = new List<IInvoiceReport>();
                try
                {

                    var pI = db.ProductItems.Include(s => s.Products).
                        Join(db.Invoices.Where(i => i.ApproveDate > date), pi => pi.DeliveryInvoiceId, i => i.Id, (pi, i) => new { pi, i }).
                        Where(s => s.pi.Quantity > 0 && s.i != null);

                    var invoiceItems = db.InvoiceItems.Where(ii => ii.Invoices.ApproveDate > date).Include(ii => ii.Invoices);


                    foreach (var productItem in pI.GroupBy(t => t.pi.ProductId))
                    {
                        var invoiceItemsCur = invoiceItems.Where(ii => productItem.Select(t => t.i.Id).Contains(ii.InvoiceId));
                        result.Add(new InvoiceReport
                    {
                        Description = productItem.First().pi.Products.Description,
                        Count = productItem.Count(),
                        Quantity = productItem.Sum(t => t.pi.Quantity) +
                        invoiceItemsCur.Where(t => t.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice).Sum(t => t.Quantity ?? 0),
                        //Cost = pI.Sum(t => t.pi.Quantity*t.pi.CostPrice),
                        Price = productItem.Sum(t => t.pi.Quantity * (t.pi.Products.Price ?? 0)),
                        Sale = productItem.Sum(t => t.pi.Quantity * ((t.pi.Products.Price ?? 0) - t.pi.CostPrice)),
                    });
                    }

                }
                catch (Exception ex)
                {

                }
                return result;
            }
        }
    }
}
