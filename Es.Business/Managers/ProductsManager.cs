using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using CashReg.Helper;
using EsMarket.SharedData.Models;
using ES.Business.Helpers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Enumerations;
using ES.Data.Models;
using ES.Data.Models.EsModels;
using ES.Data.Models.Products;
using ES.Data.Models.Reports;
using ES.DataAccess.Models;
using DataTable = System.Data.DataTable;
using MessageManager = ES.Common.Managers.MessageManager;
using ProductModel = ES.Data.Models.Products.ProductModel;
using System.Security.Cryptography;

namespace ES.Business.Managers
{
    public class ProductsManager : BaseManager
    {

        private static int MemberId { get { return ApplicationManager.Member.Id; } }


        #region Categories public methods
        //public static List<EsCategories> GetCategories()
        //{
        //    return TryGetCategories();
        //}

        #endregion

        #region Categories private methods
        //private static List<EsCategories> TryGetCategories()
        //{
        //    using (var db = GetDataContext())
        //    {
        //        return db.EsCategories.ToList();
        //    }
        //}
        #endregion


        #region Brand public methods
        public static List<Brands> GetServerBrands()
        {
            return TryGetServerBrands();
        }
        public static List<Brands> GetMemberBrands(int memberId)
        {
            return TryGetMemberBrands(memberId);
        }
        public static List<Brands> GetAllBrands(bool? isActive = null)
        {
            return TryGetAllBrands(isActive);
        }
        public static List<Brands> GetBrands(int memberId)
        {
            return TryGetBrands(memberId);
        }
        public static bool EditBrands(List<Brands> brands, int memberId)
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
        private static List<Brands> TryGetMemberBrands(int memberId)
        {
            using (var db = GetDataContext())
            {
                return db.MembersBrands.Where(s => s.MemberId == memberId).Select(s => s.Brands).OrderBy(s => s.Name).ToList();
            }
        }
        private static List<Brands> TryGetAllBrands(bool? isActive)
        {
            using (var db = GetDataContext())
            {
                return isActive == null ? db.Brands.OrderBy(s => s.Name).ToList() : db.Brands.Where(s => s.IsActive != null && s.IsActive == isActive).OrderBy(s => s.Name).ToList();
            }
        }
        private static List<Brands> TryGetBrands(int memberId)
        {
            using (var db = GetDataContext())
            {
                return db.MembersBrands.Where(s => s.MemberId == memberId).Select(s => s.Brands).OrderBy(s => s.Name).ToList();
            }
        }
        private static List<Brands> TryGetServerBrands()
        {
            using (var db = GetServerDataContext())
            {
                return db.Brands.OrderBy(s => s.Name).ToList();
            }
        }
        #endregion



        #region Products

        #region Internal cash
        //private Dictionary<Guid, ProductModel> _products = new Dictionary<Guid, ProductModel>();
        #endregion
        #region Convert Products and items
        //public static ProductModel CopyProduct(ProductModel item)
        //{
        //    if (item == null) return null;
        //    var exItem = new ProductModel(item.EsMemberId, item.LastModifierId, item.IsEnabled);
        //    exItem.Id = item.Id;
        //    exItem.Code = item.Code;
        //    exItem.Barcode = item.Barcode;
        //    exItem.HcdCs = item.HcdCs;
        //    exItem.Description = item.Description;
        //    exItem.Mu = item.Mu;
        //    exItem.IsWeight = item.IsWeight;
        //    exItem.Note = item.Note;
        //    exItem.CostPrice = item.CostPrice;
        //    exItem.OldPrice = item.OldPrice;
        //    exItem.Price = item.Price;
        //    exItem.Discount = item.Discount;
        //    exItem.DealerPrice = item.DealerProfitPercent != null ? item.DealerPrice : null;
        //    exItem.DealerDiscount = item.DealerDiscount;
        //    exItem.MinQuantity = item.MinQuantity;
        //    exItem.ExpiryDays = item.ExpiryDays;
        //    exItem.ImagePath = item.ImagePath;
        //    exItem.BrandId = item.BrandId;
        //    exItem.Brand = item.Brand;
        //    exItem.EsMemberId = item.EsMemberId;
        //    exItem.EsMember = item.EsMember;
        //    exItem.LastModifierId = item.LastModifierId;
        //    exItem.LastModifiedDate = item.LastModifiedDate;
        //    exItem.ProductKeyss = item.ProductKeyss;
        //    exItem.TypeOfTaxes = item.TypeOfTaxes;
        //    return exItem;
        //}
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
            product.MeasureUnit = item.MeasureUnit;
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
            product.ProductKeys = item.ProductKeys;
            //product.TypeOfTaxes = item.TypeOfTaxes;
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
                exItem.MeasureUnit = CashManager.Instance.MeasureOfUnits.SingleOrDefault(m => m.Id == item.MeasureOfUnitsId);
                exItem.Note = item.Note;
                exItem.CostPrice = item.CostPrice;
                exItem.OldPrice = item.OldPrice;
                exItem.Price = item.Price;
                exItem.Discount = item.Discount;
                exItem.DealerPrice = item.DealerPrice;
                exItem.DealerDiscount = item.DealerDiscount;
                exItem.MinQuantity = item.MinQuantity;
                exItem.ExpiryDays = item.ExpiryDays;
                exItem.IsEnabled = item.IsEnable;
                exItem.BrandId = item.BrandId;
                //exItem.Brand = item.Brand;
                exItem.EsMemberId = item.EsMemberId;
                //exItem.EsMember = item.EsMember;
                exItem.LastModifierId = item.LastModifierId;
                exItem.LastModifiedDate = item.LastModifiedDate;
                // todo
                //exItem.ProductCategories = Convert(item.ProductCategories);
                exItem.ProductKeys = Convert(item.ProductKeys.ToList());
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
            exItem.MeasureUnit = CashManager.Instance.MeasureOfUnits.SingleOrDefault(s => s.Id == item.MeasureOfUnitsId);
            exItem.Note = item.Note;
            exItem.CostPrice = item.CostPrice;
            exItem.OldPrice = item.OldPrice;
            exItem.Price = item.Price;
            exItem.Discount = item.Discount;
            exItem.DealerPrice = item.DealerPrice;
            exItem.DealerDiscount = item.DealerDiscount;
            exItem.MinQuantity = item.MinQuantity;
            exItem.ExpiryDays = item.ExpiryDays;
            exItem.IsEnabled = item.IsEnable;
            exItem.BrandId = item.BrandId;
            //exItem.Brand = item.Brand;
            exItem.EsMemberId = item.EsMemberId;
            //exItem.EsMember = item.EsMember;
            exItem.LastModifierId = item.LastModifierId;
            exItem.LastModifier = ApplicationManager.CashManager.GetUser(item.LastModifierId);
            exItem.LastModifiedDate = item.LastModifiedDate;
            //_products.Add(exItem.Id, exItem);

            var typeOfTaxes = item.ProductsAdditionalData;
            exItem.TypeOfTaxes = typeOfTaxes != null && typeOfTaxes.TypeOfTaxes != null ? (TypeOfTaxes)typeOfTaxes.TypeOfTaxes : default(TypeOfTaxes);

            return exItem;
        }
        public static ProductModel Convert(EsGood item)
        {
            var exProducts = ApplicationManager.CashManager.GetProducts().Where(s => s.Barcode == item.Barcode || s.Code == item.Code).ToList();
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
                //Mu = item.Unit,
                CostPrice = item.CostPrice,
                OldPrice = exProducts.Any() ? exProducts.First().Price : null,
                Price = item.Price,
                DealerPrice = item.DealerPrice
            };

            return newItem;
        }

        public static ProductModel Update(Products item)
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
                exItem.MeasureUnit = CashManager.Instance.MeasureOfUnits.SingleOrDefault(m => m.Id == item.MeasureOfUnitsId);
                exItem.Note = item.Note;
                exItem.CostPrice = item.CostPrice;
                exItem.OldPrice = item.OldPrice;
                exItem.Price = item.Price;
                exItem.Discount = item.Discount;
                exItem.DealerPrice = item.DealerPrice;
                exItem.DealerDiscount = item.DealerDiscount;
                exItem.MinQuantity = item.MinQuantity;
                exItem.ExpiryDays = item.ExpiryDays;
                exItem.IsEnabled = item.IsEnable;
                exItem.BrandId = item.BrandId;
                //exItem.Brand = item.Brand;
                exItem.EsMemberId = item.EsMemberId;
                //exItem.EsMember = item.EsMember;
                exItem.LastModifierId = item.LastModifierId;
                exItem.LastModifiedDate = item.LastModifiedDate;
                // todo
                //exItem.ProductCategories = Convert(item.ProductCategories);
                exItem.ProductKeys = Convert(item.ProductKeys.ToList());
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

        public static List<ProductOrderModelBase> GetProductItemsByPartners(List<Guid> partnerIds)
        {
            //var reports = new List<IInvoiceReport>();

            try
            {
                using (var db = GetDataContext())
                {

                    var productItems = db.ProductItems.Where(pi => pi.Quantity > 0 && pi.MemberId == MemberId)
                               .Join(db.Invoices, pi => pi.CreatedInvoiceId, i => i.Id, (productItem, invoice) => new { productItem, invoice })
                               .Where(i => i.invoice.PartnerId.HasValue && partnerIds.Contains((Guid)i.invoice.PartnerId)).ToList();
                    return productItems.Select(pi => new ProductOrderModelBase()
                    {
                        ProductId = pi.productItem.ProductId,
                        ProviderId = pi.invoice.PartnerId,
                        SaleQuantity = (double)pi.productItem.Quantity,
                        ProductItem = pi.productItem,
                        CreateInvoice = pi.invoice
                    }).ToList();
                    ////var saleInvoiceItems =
                    ////    db.InvoiceItems.Where(
                    ////        s => s.Invoices.MemberId == ApplicationManager.Member.Id && s.Invoices.ApproveDate > _dateIntermediate.Item1 &&
                    ////             (s.Invoices.InvoiceTypeId == (int)InvoiceType.SaleInvoice ||
                    ////              s.Invoices.InvoiceTypeId == (int)InvoiceType.InventoryWriteOff ||
                    ////              s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnTo)).ToList();
                    ////var salePrice = saleInvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0) + (s.Discount ?? 0));
                    ////var saleCostPrice = saleInvoiceItems.Sum(s => (s.CostPrice ?? 0) * (s.Quantity ?? 0));

                    ////var purchaseInvoiceItems = db.InvoiceItems.Where(s => s.Invoices.MemberId == ApplicationManager.Member.Id && s.Invoices.ApproveDate > _dateIntermediate.Item1 &&
                    ////    (s.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice ||
                    ////    s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnFrom)).ToList();
                    ////var purchasePrice = purchaseInvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0) + (s.Discount ?? 0));
                    ////var purchaseCostPrice = purchaseInvoiceItems.Sum(s => (s.CostPrice ?? 0) * (s.Quantity ?? 0));

                    //var stocks = ApplicationManager.CashManager.GetStocks;

                    //var field = new DynamicModel<string, object>();
                    //field.Fields.Add("Մատակարար", typeof(string));
                    //field.Fields.Add("Ապրանքագիր", typeof(string));
                    //field.Fields.Add("Ամսաթիվ", typeof(DateTime));
                    //field.Fields.Add("Կոդ", typeof(string));
                    //field.Fields.Add("Անվանում", typeof(string));
                    //field.Fields.Add("Ինքնարժեք", typeof(double));
                    //field.Fields.Add("Վաճառք", typeof(double));
                    //field.Fields.Add("Քանակ", typeof(double));
                    //foreach (var stock in stocks)
                    //    field.Fields.Add(stock.FullName, typeof(double));


                    //var items = new List<object>();

                    //foreach (var item in productItems)
                    //{

                    //    var newItem = new field
                    //    {
                    //        Մատակարար = item.invoice.ProviderName,
                    //        Ապրանքագիր = item.invoice.InvoiceNumber,
                    //        Ամսաթիվ = item.invoice.CreateDate,

                    //        Կոդ = item.productItem.Products.Code,
                    //        Անվանում = item.productItem.Products.Description,
                    //        Ինքնարժեք = item.productItem.CostPrice,
                    //        Վաճառք = item.productItem.Products.Price,
                    //        Քանակ = item.productItem.Quantity,
                    //        stocks.Select(s=>new)
                    //    };

                    //    items.Add(newItem);
                    //    //var productItems = pi.Where(s => s.StockId == stock.Id).ToList();
                    //    //reports.Add(new InvoiceReport
                    //    //{
                    //    //    Description = string.Format("{0}", stock.FullName),
                    //    //    Count = productItems.GroupBy(t => t.ProductId).Count(),
                    //    //    Quantity = productItems.Sum(t => t.Quantity),
                    //    //    Cost = productItems.Sum(t => t.Quantity * t.CostPrice) + saleCostPrice - purchaseCostPrice,
                    //    //    Price = productItems.Sum(t => t.Quantity * (t.Products.Price ?? 0)) + salePrice - purchasePrice,
                    //    //    Sale = productItems.Sum(t => t.Quantity * ((t.Products.Price ?? 0) - t.CostPrice)),
                    //    //});
                    //}
                }
            }
            catch (Exception)
            {
                MessageManager.OnMessage("Գործողությունը ձախողվել է:");
                return null;
            }
            //return reports;
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
            exItem.MeasureOfUnitsId = item.MeasureUnit != null ? item.MeasureUnit.Id : (short?)null;
            exItem.Note = item.Note;
            exItem.CostPrice = item.CostPrice;
            exItem.OldPrice = item.OldPrice;
            exItem.Price = item.Price;
            exItem.Discount = item.Discount;
            exItem.DealerPrice = item.DealerProfitPercent != null ? item.DealerPrice : null;
            exItem.DealerDiscount = item.DealerDiscount;
            exItem.MinQuantity = item.MinQuantity;
            exItem.ExpiryDays = item.ExpiryDays;
            exItem.IsEnable = item.IsEnabled;
            exItem.BrandId = item.BrandId;
            //exItem.Brand = item.Brand;
            exItem.EsMemberId = item.EsMemberId;
            //exItem.EsMember = item.EsMember;
            exItem.LastModifierId = item.LastModifierId;
            exItem.LastModifiedDate = item.LastModifiedDate;
            exItem.ProductCategories = Convert(item.ProductCategories);
            exItem.ProductKeys = Convert(item.ProductKeys);

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
            return items.Select(s => new ProductCategoriesModel() { Id = s.Id, CategoriesId = (short)s.CategoryId, ProductId = s.ProductId }).ToList();
        }
        public static ICollection<ProductKeys> Convert(List<ProductKeysModel> items)
        {
            if (items == null || items.Count == 0) return new List<ProductKeys>();
            return items.Select(s => new ProductKeys { Id = s.Id, ProductKey = s.ProductKey, ProductId = s.ProductId, MemberId = s.MemberId }).ToList();

        }
        public static List<ProductKeysModel> Convert(List<ProductKeys> items)
        {
            if (items == null || items.Count == 0) return new List<ProductKeysModel>();
            return items.Select(s => new ProductKeysModel(s.ProductKey, s.ProductId, s.MemberId) { Id = s.Id }).ToList();
        }
        #endregion
        #region Product Public methods
        public static bool ChangeProductCode(Guid id, string productCode, int memberId)
        {
            return TryChangeProductCode(id, productCode, memberId);
        }
        public static bool ChangeProductEnabled(Guid id, int memberId)
        {
            return TryChangeProductEnabled(id, memberId);
        }
        //public static List<ProductModel> GetProductsByMember(int memberId)
        //{
        //    return TryGetProductsByMember(memberId).Select(ConvertProduct).OrderByDescending(s => s.Code).ToList();
        //}
        public static List<ProductModel> GetProductsByCodeOrBarcode(string code)
        {
            var products = TryGetProductsByCodeOrBarcode(code);
            return products != null ? products.Select(Convert).ToList() : new List<ProductModel>();
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
        public List<ProductModel> GetExistingProduct(int memberId)
        {
            return TryGetExistingProduct(memberId);
        }
        public static List<ProductModel> GetProducts(bool isActive = true)
        {
            return TryGetProducts(isActive).Select(Convert).ToList();
        }
        public static List<Products> GetActiveProducts()
        {
            return TryGetProducts(true);
        }
        public static List<ProductModel> GetProductsForView()
        {
            return TryGetProducts(true).Select(s => new ProductModel
            {
                Id = s.Id,
                Code = s.Code,
                Description = s.Description,
                MeasureUnit = CashManager.Instance.MeasureOfUnits.SingleOrDefault(mu => mu.Id == s.MeasureOfUnitsId),
                ExistingQuantity = 0,
                Price = s.Price,
                OldPrice = s.OldPrice,
                CostPrice = s.CostPrice,
                LastModifier = CashManager.Instance.GetUser(s.LastModifierId)
            }).OrderBy(s => s.Description).ToList();
        }
        public static List<ProductModel> GetChangedProducts(Tuple<DateTime, DateTime> dateIntermediate)
        {
            return TryGetChangedProducts(dateIntermediate).Select(ConvertLight).ToList();
        }
        public static List<LogForProductsModel> GetModifiedProducts(Tuple<DateTime, DateTime> dateIntermediate, bool showPriceChangedOnly)
        {
            var items = TryGetModifiedProducts(dateIntermediate, showPriceChangedOnly);
            return items;
        }
        public List<ProductModel> GetProductsShortData(int memberId)
        {
            return TryGetProductsShortData(memberId);
        }
        public List<ProductModel> GetRegisteredProducts(int memberId)
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
            productAdditionalData.TypeOfTaxes = product.TypeOfTaxes > 0 ? (short)product.TypeOfTaxes : (short?)null;

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
        public static bool DeleteProduct(Guid id, int memebrId)
        {
            return TryDeleteProduct(id, memebrId);
        }
        public static decimal GetProductItemQuantity(Guid? productId, List<short> fromStocks)
        {
            return TryGetProductItemCount(productId, fromStocks);
        }
        public static decimal GetProductItemCount(Guid? productId, int memberId)
        {
            return TryGetProductItemCount(productId, memberId);
        }
        public static List<ProductsByStockModel> GetProductsQuantity()
        {
            return TryGetProductsQuantity();
        }
        public static decimal GetProductItemCountFromStock(Guid? productId, List<short> stockIds, int memeberId)
        {
            return TryGetProductItemCountFromStock(productId, stockIds, memeberId);
        }
        public static List<ProductItemModel> GetProductItems(string productKey = null)
        {
            try
            {
                var productItems = TryGetProductItems(productKey);
                var products = ApplicationManager.CashManager.GetProducts();
                return productItems.Select(s => new ProductItemModel
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    Product = products.SingleOrDefault(p => p.Id == s.ProductId),
                    StockId = s.StockId,
                    Quantity = s.Quantity,
                    MemberId = s.MemberId
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error);
                return new List<ProductItemModel>();
            }

        }
        public static List<ProductItemModel> GetProductItemsByStocks(List<short> stockIds, string productkey = null)
        {
            var productItems = TryGetProductItemsByStocks(stockIds, productkey);
            var productIds = productItems.Select(s => s.ProductId).GroupBy(s => s).Select(s => s.Key).ToList();
            var products = ApplicationManager.CashManager.GetProducts().Where(s => productIds.Contains(s.Id)).ToList();
            //convertAsProduct
            return productItems.Select(s => new ProductItemModel
            {
                Id = s.Id,
                ProductId = s.ProductId,
                Product = products.FirstOrDefault(t => t.Id == s.ProductId),
                Quantity = s.Quantity,
                StockId = s.StockId,
                MemberId = s.MemberId
            }).ToList();
        }

        public static List<ProductItemModel> GetProductItemsByStock(short stockId)
        {
            return GetProductItemsByStocks(new List<short> { stockId });
        }

        public static List<ProductResidue> GeProductResidues(int memberId)
        {
            return TryGetProductResidue(memberId);
        }
        public List<ProductItemModel> GetProductItemsForInvoices(IEnumerable<Guid> invoiceIds, int memberId)
        {
            var products = GetProducts();
            return TryGetProductItemsForInvoices(invoiceIds, memberId).Select(s => Convert(s, products)).ToList();
        }
        public List<ProductItemModel> GetAllProductItems(int memberId)
        {
            var products = GetProducts();
            return TryGetAllProductItems(memberId).Select(s => Convert(s, products)).ToList();
        }
        public static List<ProductItemModel> GetProductItemsFromStocks(List<short> stockIds)
        {
            var products = GetProducts();
            return TryGetProductItemsFromStocks(stockIds).Select(s => Convert(s, products)).ToList();
        }
        public static List<ProductItemModel> GetUnavailableProductItems(List<Guid> productItemIds, List<short> stockIds)
        {
            var products = CashManager.Instance.GetProducts();
            return TryGetUnavilableProductItems(productItemIds, stockIds).Select(s => Convert(s, products.SingleOrDefault(p => p.Id == s.ProductId))).ToList();
        }
        public static long GetProductCount(int memberId)
        {
            return TryGetProductCount(memberId);
        }
        public static List<ProductOrderModel> GetProductOrderByBrands(List<Brands> brands, int memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var productItems = db.ProductItems.Where(s => s.Products.IsEnable && s.Products.MinQuantity != null && s.MemberId == memberId).ToList();
                    var brandIds = brands.Select(t => t.Id).ToList();
                    var products = db.Products.Where(s => s.BrandId != null && brandIds.Contains((int)s.BrandId) && s.MinQuantity != null && s.IsEnable && s.EsMemberId == memberId).ToList();
                    var partners = CashManager.Instance.GetPartners;
                    products = products.Where(s => s.MinQuantity > productItems.Where(t => t.ProductId == s.Id)
                        .Sum(t => t.Quantity)).OrderByDescending(s => s.InvoiceItems.First().Invoices.CreateDate).ToList();
                    var productOrderItems =
                        products.Select(s => new ProductOrderModel
                        {
                            Code = s.Code,
                            HCDCS = s.HCDCS,
                            Description = s.Description,
                            Price = s.Price,
                            MinQuantity = s.MinQuantity,
                            ExistingQuantity = productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity),
                            Provider = partners.SingleOrDefault(p => p.Id == s.InvoiceItems.First().Invoices.PartnerId)

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

            try
            {
                List<ProductItems> productItems;
                //List<InvoiceItems> invoiceItems;
                var products = CashManager.Instance.GetProducts().Where(s => s.IsEnabled && s.MinQuantity != null).ToList();
                var productIds = products.Select(t => t.Id).ToList();
                var dateFrom = DateTime.Today.AddYears(-1);
                using (var db = GetDataContext())
                {
                    //var products = db.Products.Where(s => s.IsEnable && s.MinQuantity != null && s.EsMemberId == memberId).ToList();

                    productItems = db.ProductItems.Where(s => s.Quantity > 0 && productIds.Contains(s.ProductId) && s.MemberId == memberId).ToList();
                    db.Database.CommandTimeout = 300;
                    var invoices = db.Invoices.Where(i => i.ApproveDate > dateFrom && i.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice && i.MemberId == memberId).Select(i => i.Id).ToList();
                    var invoiceItemsEx = db.InvoiceItems.Where(ii => invoices.Contains(ii.InvoiceId)).Select(ii => new
                    {
                        CreateDate = ii.Invoices.CreateDate,
                        ProductId = ii.ProductId,
                        PartnerId = ii.Invoices.PartnerId
                    }).ToList();
                    invoiceItemsEx = invoiceItemsEx.Where(ii =>

                                     //            .Where(ii => ii.Invoices.ApproveDate > dateFrom).ToList();
                                     //invoiceItems = invoiceItems.Where(ii=>
                                     //                ii.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice &&
                                     //                ii.Invoices.MemberId == memberId &&
                                     productIds.Contains(ii.ProductId)).ToList();

                    db.Dispose();


                    invoiceItemsEx = invoiceItemsEx.OrderByDescending(ii => ii.CreateDate).GroupBy(ii => ii.ProductId).Select(ii => ii.FirstOrDefault()).ToList();
                    products = products.Where(s => productItems.All(t => t.ProductId != s.Id) || s.MinQuantity > productItems.Where(t => t.ProductId == s.Id).Sum(t => t.Quantity)).ToList();

                    //var productOrderItems = products
                    //    .Select(p => new
                    //    {
                    //        p,
                    //        ii = invoiceItems.SingleOrDefault(i => i.ProductId == p.Id)
                    //        //ii = db.InvoiceItems
                    //        //    .Where(ii =>
                    //        //        ii.ProductId == p.Id && 
                    //        //        ii.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice &&
                    //        //        ii.Invoices.ApproveDate != null &&
                    //        //        ii.Invoices.MemberId == memberId)
                    //        //    .OrderByDescending(ii => ii.Invoices.CreateDate).FirstOrDefault()
                    //    }).ToList();
                    var partners = CashManager.Instance.GetPartners;
                    return products.Select(p => new ProductOrderModel
                    {
                        Index = 0,
                        ProductId = p.Id,
                        Code = p.Code,
                        Description = p.Description,
                        //Mu = s.p.Mu,
                        CostPrice = p.CostPrice,
                        Price = p.Price,
                        MinQuantity = p.MinQuantity,
                        ExistingQuantity = productItems.Where(t => t.ProductId == p.Id).Sum(t => t.Quantity),
                        Provider = CashManager.GetPartner(invoiceItemsEx.Where(s => s.ProductId == p.Id).Select(ii => ii.PartnerId).FirstOrDefault())
                        //partners.SingleOrDefault(p => p.Id == s.ii.Invoices.PartnerId) : null
                        //Notes = s.Note
                    }).ToList();
                }
                //return productO021rderItems;




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
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.Message);
                return new List<ProductOrderModel>();
            }
        }
        public static PartnerModel GetProductsProvider(Guid productId)
        {
            return PartnersManager.Convert(TryGetProductsProvider(productId));
        }
        public static List<ProductProvider> GetProductsProviders(List<Guid> productIds)
        {
            return TryGetProductsProviders(productIds);
        }

        public static int GetNextProductCode(int memberId)
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
            if (exItem.Product == null && item.Products != null)
            {
                exItem.Product = new ProductModel()
                {
                    Id = item.ProductId,
                    Code = item.Products.Code,
                    Barcode = item.Products.Barcode,
                    Description = item.Products.Description,
                    Price = item.Products.Price,
                    Note = item.Products.Note
                };
            }
            exItem.CreateInvoiceId = item.CreatedInvoiceId;
            exItem.CreatedDate = item.CreatedDate;
            exItem.DeliveryInvoiceId = item.DeliveryInvoiceId;
            exItem.DeliveryDate = item.DeliveryDate;
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
        public static ProductItemModel Convert(ProductItems item, ProductModel product = null)
        {
            if (item == null) return null;
            var exItem = new ProductItemModel();
            exItem.Id = item.Id;
            exItem.MemberId = item.MemberId;
            exItem.ProductId = item.ProductId;
            exItem.Product = product; // Convert(item.Products);
            exItem.CreateInvoiceId = item.CreatedInvoiceId;
            exItem.CreatedDate = item.CreatedDate;
            exItem.DeliveryInvoiceId = item.DeliveryInvoiceId;
            exItem.DeliveryDate = item.DeliveryDate;
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
        private static bool TryChangeProductCode(Guid id, string code, int memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var exItem = db.Products.FirstOrDefault(s => s.EsMemberId == memberId && s.Code == code);
                    if (exItem != null)
                    {
                        MessageManager.ShowMessage(code + " կոդը զբաղված է։", "Գործողության ընդհատում");
                        return false;
                    }
                    exItem = db.Products.SingleOrDefault(s => s.EsMemberId == memberId && s.Id == id);
                    if (exItem == null)
                    {
                        MessageManager.ShowMessage("Սխալ գործողություն։", "Գործողության ընդհատում");
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
        private static bool TryChangeProductEnabled(Guid id, int memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var exItem = db.Products.FirstOrDefault(s => s.EsMemberId == memberId && s.Id == id);
                    if (exItem == null)
                    {
                        MessageManager.ShowMessage("Ապրանքը հայտնաբերված չէ։", "Գործողության ընդհատում");
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
        private static decimal TryGetProductItemCount(Guid? productId, List<short> fromStocks)
        {
            if (!productId.HasValue || fromStocks == null) return 0;
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    if (!fromStocks.Any()) return 0;
                    var items = db.ProductItems.Where(s => s.MemberId == memberId && s.Quantity > 0 && s.StockId.HasValue && fromStocks.Contains(s.StockId.Value) && s.ProductId == productId.Value).ToList();
                    var value = items.Sum(s => s.Quantity);
                    return value;
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error);
                    return 0;
                }
            }
        }
        private static List<ProductsByStockModel> TryGetProductsQuantity()
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var items = db.ProductItems.Where(s => s.MemberId == ApplicationManager.Member.Id && s.Quantity > 0)
                        .GroupBy(s => new { s.StockId, s.ProductId })
                        .Select(pi => new ProductsByStockModel { ProductId = pi.Select(t => t.ProductId).FirstOrDefault(), StockId = pi.Select(t => t.StockId).FirstOrDefault(), Quantity = pi.Sum(s => s.Quantity) }).ToList();
                    return items;
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error);
                    return null;
                }
            }
        }
        private static decimal TryGetProductItemCount(Guid? productId, int memberId)
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
        private static decimal TryGetProductItemCountFromStock(Guid? productId, List<short> stockIds, int memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.ProductItems.Where(s => s.StockId != null && stockIds.Contains((short)s.StockId) && s.ProductId == productId && s.MemberId == memberId).Sum(s => s.Quantity);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        private static List<Products> TryGetProductsByMember(int memberId)
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
        private static List<Products> TryGetProductsByCodeOrBarcode(string code)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var product = db.Products.
                        Include(s => s.Brands).
                        Include(s => s.ProductCategories).
                        Include(s => s.ProductKeys).
                        Include(s => s.ProductsAdditionalData).
                        Where(s => s.EsMemberId == ApplicationManager.Member.Id && s.IsEnable && (s.Code == code || s.ProductKeys.Any(t => t.ProductKey == code) || s.Barcode == code)).ToList();
                    //.OrderBy(s => s.ProductKeys.Count)
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
                                        .Include(s => s.ProductKeys)
                                        .Include(s => s.ProductsAdditionalData).
                                        SingleOrDefault(s => s.EsMemberId == ApplicationManager.Member.Id && s.Id == id);
                }
                catch (Exception)
                {
                    return new Products();
                }

            }
        }
        private static List<Products> TryGetProducts(bool isActive)
        {
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products
                        .Include(s => s.ProductCategories)
                        .Include(s => s.ProductKeys)
                        .Include(s => s.ProductsAdditionalData)
                        .Where(s => s.EsMemberId == memberId && s.IsEnable == isActive).ToList();
                }
                catch (Exception)
                {
                    return new List<Products>();
                }

            }
        }
        private static List<Products> TryGetChangedProducts(Tuple<DateTime, DateTime> dateIntermediate)
        {
            if (dateIntermediate == null) return null;
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products
                        .Include(s => s.ProductCategories)
                        .Include(s => s.ProductKeys)
                        .Include(s => s.ProductsAdditionalData)
                        .Where(s => s.EsMemberId == memberId && s.IsEnable && s.LastModifiedDate >= dateIntermediate.Item1 && s.LastModifiedDate <= dateIntermediate.Item2)
                        .ToList();
                }
                catch (Exception)
                {
                    return new List<Products>();
                }

            }
        }
        private static List<LogForProductsModel> TryGetModifiedProducts(Tuple<DateTime, DateTime> dateIntermediate, bool showPriceChangedOnly)
        {
            if (dateIntermediate == null) return null;
            var memberId = ApplicationManager.Member.Id;
            using (var db = GetDataContext())
            {
                try
                {
                    var items = db.LogForProducts
                        .Where(log => log.MemberId == memberId && log.Date >= dateIntermediate.Item1 && log.Date <= dateIntermediate.Item2)
                        .Select(log => new LogForProductsModel
                        {
                            Id = log.Id,
                            Action = (ModificationTypeEnum)log.Action,
                            Date = log.Date,
                            ProductId = log.ProductId,
                            //Product = CashManager.Instance.GetProduct(i.log.ProductId),
                            Code = log.Code,
                            Description = log.Description,
                            CostPrice = log.CostPrice,
                            Price = log.Price,
                            //OldPrice = db.LogForProducts.Where(s=>s.Id==i.log.Id && s.Date<i.log.Date).OrderBy(s=>s.Date).Select(s=>s.Price).LastOrDefault(),
                            IsEmpty = log.IsEmpty,
                            ModifierId = log.ModifierId,
                            //Modifier = CashManager.Instance.GetUser(i.l.ModifierId),
                            MemberId = log.MemberId
                        }).OrderBy(s => s.Date).ToList();

                    foreach (var item in items)
                    {
                        item.Product = CashManager.GetProduct(item.ProductId);
                        item.Modifier = CashManager.Instance.GetUser(item.ModifierId);
                        var oldPrice = db.LogForProducts.Where(log => log.MemberId == memberId && log.ProductId == item.ProductId && log.Date < item.Date).OrderByDescending(s => s.Date).FirstOrDefault();
                        item.OldPrice = oldPrice != null ? oldPrice.Price : null;
                        item.Log = items;
                    }
                    if (showPriceChangedOnly)
                    {
                        items = items.Where(s => s.Action != ModificationTypeEnum.Added && !s.IsEmpty).ToList();
                    }
                    items = items.Where(s => s.OldPrice != s.Price).ToList();
                    return items;
                }
                catch (Exception)
                {
                    return new List<LogForProductsModel>();
                }

            }
        }
        private static List<ProductModel> TryGetProductsShortData(int memberId)
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
        private static List<Products> TryGetRegisteredProducts(int memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Products
                        .Include(s => s.ProductCategories)
                        .Include(s => s.ProductCategories)
                        .Include(s => s.ProductKeys)
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
                                .Include(s => s.ProductKeys)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId).Select(Convert).ToList();
                        case ProductViewType.ByActive:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductKeys)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId && s.IsEnable).Select(Convert).ToList();
                        case ProductViewType.ByPasive:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductKeys)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId && !s.IsEnable).Select(Convert).ToList();
                        case ProductViewType.ByEmpty:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductKeys)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId).Select(Convert).ToList();
                        case ProductViewType.WeigthsOnly:

                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductKeys)
                                .Include(s => s.ProductsAdditionalData)
                                .Where(s => s.EsMemberId == memberId).AsEnumerable().Select(Convert).Where(s => s.IsWeight).ToList();
                        default:
                            return db.Products
                                .Include(s => s.ProductCategories)
                                .Include(s => s.ProductKeys)
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
        private static DataTable TryGetProductTabletDataFromServer(int memberId)
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
        private static List<ProductModel> TryGetExistingProduct(int memberId)
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
                        if (db.Products.Count(s => s.EsMemberId == memberId && (s.Code == item.Code || (!string.IsNullOrEmpty(s.Barcode) && s.Barcode == item.Barcode) || (!string.IsNullOrEmpty(item.Barcode) && s.ProductKeys.Any(t => t.ProductKey == item.Barcode)))) > 1)
                        {
                            MessageManager.OnMessage(string.Format("Barcode-ի կրկնություն։ Ապրանքի խմբագրումը չի իրականացել։ \n Կոդ։ {0} \nԲարկոդ: {1}", item.Code, item.Barcode), MessageTypeEnum.Warning);
                        }
                        var exItem = db.Products.SingleOrDefault(s => s.Code == item.Code && s.EsMemberId == memberId);


                        if (exItem != null)
                        {
                            //exItem.LastModifiedDate = exItem.LastModifiedDate.AddMilliseconds(-exItem.LastModifiedDate.Millisecond);
                            if (exItem.LastModifiedDate > item.LastModifiedDate)
                            {
                                MessageManager.OnMessage(
                                    string.Format(
                                        "Ապրանքն ավելի վաղ արդեն խմբագրվել է։ Ապրանքի խմբագրումը չի իրականացել։ \n Կոդ։ {0} \nԲարկոդ: {1}",
                                        item.Code, item.Barcode), MessageTypeEnum.Warning);
                                continue;
                            }
                        }

                        CheckIsProductPriceWasChanged(item, exItem, db);
                        if (exItem != null)
                        {
                            exItem.LastModifiedDate = item.LastModifiedDate = DateTime.Now;

                            exItem.Code = item.Code;
                            exItem.Barcode = item.Barcode;
                            exItem.HCDCS = item.HCDCS;
                            exItem.Description = item.Description;
                            var mu = CashManager.Instance.MeasureOfUnits.SingleOrDefault(m => m.Id == item.MeasureOfUnitsId);
                            exItem.MeasureOfUnitsId = mu != null ? mu.Id : (short?)null;
                            exItem.Note = item.Note;
                            exItem.CostPrice = item.CostPrice;
                            exItem.ExpiryDays = item.ExpiryDays;
                            exItem.Price = item.Price;
                            exItem.Discount = item.Discount;
                            exItem.DealerPrice = item.DealerPrice;
                            exItem.DealerDiscount = item.DealerDiscount;
                            exItem.IsEnable = item.IsEnable;
                            exItem.BrandId = item.BrandId;
                            exItem.LastModifierId = userId;

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
                if (db.Products.Count(s => s.Code == item.Code || (!string.IsNullOrEmpty(s.Barcode) && s.Barcode == item.Barcode) || (!string.IsNullOrEmpty(item.Barcode) && s.ProductKeys.Any(t => t.ProductKey == item.Barcode))) > 1)
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

                var productKeys = item.ProductKeys.ToList();
                using (var db = GetDataContext())
                {
                    if (db.Products.Count(s => (s.Code == item.Code || (!string.IsNullOrEmpty(item.Barcode) && s.Barcode == item.Barcode) || s.Id == item.Id) && s.EsMemberId == item.EsMemberId) > 1)
                    {
                        MessageManager.OnMessage("Code-ը կամ Barcode-ը կրկնվում է։ Գործողությունը հնարավոր չէ շարունակել։");
                        return null;
                    }
                    var exItem = db.Products.Include(s => s.ProductsAdditionalData).SingleOrDefault(s => s.Code == item.Code && s.EsMemberId == item.EsMemberId);
                    if (exItem != null)
                    {
                        if (exItem.Id != item.Id && item.Id != Guid.Empty)
                        {
                            MessageManager.OnMessage("Գործողությունը դադարեցված է։ \nԱպրանքի կոդն արդեն գրանցված է։ Խնդրում ենք փոխել կոդը և նորից փորձել։");
                            return null;
                        }
                        exItem.LastModifiedDate = exItem.LastModifiedDate.AddMilliseconds(-exItem.LastModifiedDate.Millisecond);
                        if (exItem.LastModifiedDate > item.LastModifiedDate)
                        {
                            MessageManager.OnMessage("Գործողությունը դադարեցված է։ \nԱպրանքն ավելի վաղ արդեն խմբագրվել է։", MessageTypeEnum.Warning);
                            return null;
                        }
                    }
                    else
                    {
                        item.LastModifiedDate = DateTime.Now;
                    }

                    var exItemsByBarcode = db.Products.Where(s => s.EsMemberId == item.EsMemberId && (s.Code == item.Code || (!string.IsNullOrEmpty(item.Barcode) && s.Barcode == item.Barcode))).ToList();
                    if ((exItemsByBarcode.Count > 1 || (exItem == null && exItemsByBarcode.Count == 1)))
                    {
                        MessageManager.OnMessage("Գործողությունը դադարեցված է։ \nԱպրանքի բարկոդը կրկնվում է։ Խնդրում ենք փոխել բարկոդը և նորից փորձել։");
                        return null;
                    }
                    CheckIsProductPriceWasChanged(item, exItem, db);
                    if (exItem != null)
                    {
                        if (exItem.Code != item.Code) return null;
                        exItem.LastModifiedDate = item.LastModifiedDate = DateTime.Now;

                        exItem.Barcode = item.Barcode;
                        exItem.HCDCS = item.HCDCS;
                        exItem.Description = item.Description;
                        var mu = CashManager.Instance.MeasureOfUnits.SingleOrDefault(m => m.Id == item.MeasureOfUnitsId);
                        exItem.MeasureOfUnitsId = mu != null ? mu.Id : (short?)null;
                        exItem.Note = item.Note;
                        exItem.CostPrice = item.CostPrice;
                        exItem.Price = item.Price;
                        exItem.Discount = item.Discount;
                        exItem.DealerPrice = item.DealerPrice;
                        exItem.DealerDiscount = item.DealerDiscount;
                        exItem.MinQuantity = item.MinQuantity;
                        exItem.ExpiryDays = item.ExpiryDays;
                        exItem.IsEnable = item.IsEnable;
                        exItem.BrandId = item.BrandId;
                        exItem.LastModifierId = item.LastModifierId;
                        exItem.LastModifiedDate = item.LastModifiedDate;
                        item.Id = exItem.Id;
                    }
                    else
                    {
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
                    var exProductKeys = db.ProductKeys.Where(s => s.ProductId == item.Id && MemberId == item.EsMemberId).ToList();
                    foreach (var value in exProductKeys)
                    {
                        var exProductKey = productKeys.FirstOrDefault(s => s.ProductKey == value.ProductKey);
                        if (exProductKey != null)
                        {
                            productKeys.Remove(exProductKey);
                        }
                        else
                        {
                            db.ProductKeys.Remove(value);
                        }
                    }
                    foreach (var productItem in productKeys)
                    {
                        if (exProductKeys.Any(s => s.ProductKey == productItem.ProductKey)) continue;
                        productItem.ProductId = item.Id;
                        db.ProductKeys.Add(productItem);
                    }
                    db.SaveChanges();
                    item.ProductKeys = db.ProductKeys.Where(s => s.ProductId == item.Id).ToList();
                    return item;
                }
            }
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.ToString());
                return null;
            }

        }

        private static void CheckIsProductPriceWasChanged(Products product, Products existingProduct, EsStockDBEntities db)
        {
            if (existingProduct == null || existingProduct.Price != product.Price)
            {
                ModificationTypeEnum action = ModificationTypeEnum.Modified;
                if (existingProduct != null)
                {
                    existingProduct.OldPrice = existingProduct.Price;
                    if (existingProduct.Code != product.Code || existingProduct.Barcode != product.Barcode) { action = ModificationTypeEnum.ReCreated; }
                }
                else
                {
                    action = ModificationTypeEnum.Added;
                }


                var log = new LogForProducts
                {
                    Action = (short)action,
                    ProductId = product.Id,
                    Code = product.Code,
                    Description = product.Description,
                    CostPrice = product.CostPrice,
                    Price = product.Price,
                    Date = product.LastModifiedDate,
                    IsEmpty = GetProductQuantity(product.Id) == 0,
                    ModifierId = ApplicationManager.GetEsUser.UserId,
                    MemberId = product.EsMemberId
                };
                AddProductChangedLog(log, db);
            }
        }

        private static void AddProductChangedLog(LogForProducts log, EsStockDBEntities db)
        {
            if (log != null && db != null)
            {
                db.LogForProducts.Add(log);
            }

        }
        private class LogOfProducts
        {
            public Guid Id;
            public DateTime Date;
            public Guid ProductId;
            public string Code;
            public string Description;
            public double CostPrice;
            public double Price;
            public ModificationTypeEnum Action;
            public bool IsEmpty;
            public int MemberId;
            public int ModifierId;
            public LogOfProducts()
            {
                Id = Guid.NewGuid();
                Date = DateTime.Now;
            }
        }
        private static bool TryDeleteProduct(Products item)
        {
            return item != null && TryDeleteProduct(item.Id, item.EsMemberId);
        }
        private static bool TryDeleteProduct(Guid id, int memberId)
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
        private static List<ProductItems> TryGetAllProductItems(int memberId)
        {
            using (var db = GetDataContext())
            {
                return db.ProductItems
                    .Include(s => s.Products)
                    .Where(s => s.MemberId == memberId).ToList();
            }
        }
        private static List<ProductItems> TryGetProductItems(string productKey = null)
        {
            var db = GetDataContext();
            try
            {
                productKey = productKey != null ? productKey.ToLower() : "";

                var productItems = db.ProductItems

                    .Where(s => s.MemberId == ApplicationManager.Member.Id && s.Quantity != 0);
                if (!string.IsNullOrWhiteSpace(productKey))
                {
                    return productItems.Where(s => s.Products.Code.ToLower().Contains(productKey) || s.Products.Description.ToLower().Contains(productKey)).ToList();
                }
                else
                {
                    return productItems.ToList();
                }
            }
            catch (Exception)
            {
                return new List<ProductItems>();
            }

        }
        private static List<ProductItems> TryGetProductItemsByStocks(List<short> stockIds, string productKey = null)
        {
            var db = GetDataContext();
            try
            {
                var productIds = db.Products.Where(s => s.EsMemberId == ApplicationManager.Member.Id && (string.IsNullOrEmpty(productKey) || s.Code.Contains(productKey) || s.Description.Contains(productKey))).Select(s => s.Id);
                var productItems = db.ProductItems.Where(s => s.MemberId == ApplicationManager.Member.Id &&
                                    s.StockId.HasValue && stockIds.Contains((short)s.StockId) && s.Quantity != 0 &&
                                   productIds.Contains(s.ProductId))
                    //.Include(s => s.Products)
                    //.Include(s => s.Products.ProductKeys)
                    //.Include(s => s.Products.ProductCategories)
                    //.Include(s => s.Products.ProductsAdditionalData)
                    .ToList();
                return productItems;
            }
            catch (Exception)
            {
                return new List<ProductItems>();
            }

        }
        private static List<ProductItems> TryGetProductItemsForInvoices(IEnumerable<Guid> invoiceIds, int memberId)
        {
            using (var db = GetDataContext())
            {
                var productIds = db.InvoiceItems
                    .Where(s => s.Invoices.MemberId == memberId && invoiceIds.Contains(s.InvoiceId)).Select(s => s.ProductItemId);
                return db.ProductItems.Where(pi => productIds.Contains(pi.Id)).ToList();
            }
        }
        private static List<ProductItems> TryGetProductItemsFromStocks(List<short> stockIds)
        {
            using (var db = GetDataContext())
            {
                return db.ProductItems
                    .Include(p => p.Products)
                    .Where(pi => pi.Quantity != 0 && pi.StockId != null && stockIds.Contains((short)pi.StockId) && pi.MemberId == MemberId)
                    .Join(db.Invoices, pi => pi.DeliveryInvoiceId, ii => ii.Id, (pi, ii) => new { pi, ii })
                    .OrderBy(s => s.ii.CreateDate)
                    .Select(s => s.pi).ToList();
            }
        }
        private static List<ProductItems> TryGetUnavilableProductItems(List<Guid> productIds, List<short> stockIds)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.ProductItems.Include(s => s.Products).Where(s => s.MemberId == ApplicationManager.Member.Id && s.Quantity != 0 && !productIds.Contains(s.ProductId) && stockIds.Contains(s.StockId ?? 0)).ToList();
                }
                catch (Exception)
                {
                    MessageManager.ShowMessage("Կապի խափանում", "Հարցման սխալ");
                    return new List<ProductItems>();
                }
            }
        }
        private static List<ProductResidue> TryGetProductResidue(int memberId)
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
        private static long TryGetProductCount(int memberId)
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
        private static Partners TryGetProductsProvider(Guid productId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var partnerId = db.InvoiceItems
                        .Where(s => s.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice && s.ProductId == productId && s.Invoices.PartnerId != null && s.Invoices.MemberId == ApplicationManager.Member.Id)
                        .OrderByDescending(s => s.Invoices.CreateDate).Select(s => s.Invoices.PartnerId).FirstOrDefault();


                    if (partnerId == null) { return null; }

                    return db.Partners.Include(s => s.EsPartnersTypes).FirstOrDefault(s => s.EsMemberId == ApplicationManager.Member.Id && s.Id == partnerId);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static List<ProductProvider> TryGetProductsProviders(List<Guid> productIds)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var invoceiItems = db.InvoiceItems
                        .Where(s => s.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice && productIds.Contains(s.ProductId) && s.Invoices.PartnerId != null)
                        .OrderByDescending(s => s.Invoices.CreateDate).GroupBy(s => s.ProductId);

                    var items = new List<ProductProvider>();
                    foreach (var invoceItem in invoceiItems)
                    {
                        if (!invoceItem.Any()) { continue; }
                        var invoice = invoceItem.First();
                        items.Add(new ProductProvider
                        {
                            ProductId = invoice.ProductId,
                            ProviderId = invoice.Invoices != null && invoice.Invoices.PartnerId != null ? ((Guid)invoice.Invoices.PartnerId) : Guid.Empty
                        });
                    }
                    return items;
                }
                catch (Exception)
                {
                    MessageManager.ShowMessage("Կապի խափանում", "Հարցման սխալ");
                    return new List<ProductProvider>();
                }
            }
        }

        private static int TryGetNextProductCode(int memberId)
        {
            //Reserve 1000 codes
            var nextcode = 11000;
            using (var db = GetDataContext())
            {
                try
                {
                    nextcode += db.Products.Where(s => s.EsMemberId == memberId).Count();
                    //var items = db.Products.Where(s => s.EsMemberId == memberId).Select(s => s.Code).ToList();
                    //nextcode += items.Count;
                    //var code = string.Format("{0}{1}", !ApplicationManager.Settings.SettingsContainer.MemberSettings.UseUnicCode ? "" : ApplicationManager.Member.Id.ToString("D2"), nextcode);
                    //while (items.Any(s => s == code))
                    //{
                    //    nextcode--;
                    //    code = string.Format("{0}{1}", !ApplicationManager.Settings.SettingsContainer.MemberSettings.UseUnicCode ? "" : ApplicationManager.Member.Id.ToString("D2"), nextcode);
                    //}
                }
                catch (Exception e)
                {
                    nextcode += 1;
                    //MessageManager.OnMessage(e.ToString());
                }
                return nextcode;
            }
        }
        #endregion

        public static List<EsmMeasureUnitModel> GetMeasureOfUnits()
        {
            using (var db = GetDataContext())
            {
                return db.EsmMeasureOfUnits.Select(s => new EsmMeasureUnitModel() { Id = s.Id, Key = s.Code, Name = s.Name, IsWeight = s.IsWeight, GroupId = s.GroupId, DisplayOrder = s.DisplayOrder, Ratio = s.Ratio }).OrderBy(s => s.DisplayOrder).ToList();
            }
        }
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
                    var items = db.ProductItems
                        .Where(s => s.MemberId == MemberId && s.Quantity > 0 && s.CreatedDate <= date)
                        //.Join(db.Invoices.Where(i => i.MemberId == MemberId), s => s.DeliveryInvoiceId, i => i.Id, (s, i) => new { s, i })
                        //.Where(t => t.i.InvoiceTypeId != (int)InvoiceType.SaleInvoice);
                        .Select(s => s.ProductId).ToList();
                    var invoiceItems = db.InvoiceItems.Where(i => i.Invoices.MemberId == MemberId && i.Invoices.InvoiceTypeId == (short)InvoiceType.SaleInvoice && i.Invoices.CreateDate > date && items.Contains(i.ProductId)).GroupBy(s => s.ProductId).Select(s => s.Key).ToList();
                    //var groupItems = items

                    //.Include(t => t.s.Products.ProductKeys)
                    //.Include(t => t.s.Products.ProductCategories)
                    //.Include(t => t.s.Products.ProductsAdditionalData)
                    //.Where(t => t.i.ApproveDate < date).Select(t => t.s.ProductId).ToList();
                    items = items.Where(s => !invoiceItems.Contains(s)).ToList();
                    var products = CashManager.Instance.GetProducts().Where(s => items.Contains(s.Id)).ToList();
                    //foreach (var productItems in groupItems)
                    //{
                    //    var product = Convert(productItems.First().s.Products);
                    //    product.ExistingQuantity = productItems.Sum(t => t.s.Quantity);
                    //    product.Provider = GetProductsProvider(product.Id);
                    //    products.Add(product);
                    //}
                    return products;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static List<ProductModel> GetMissingProductItems(short stockId)
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
                        .Include(t => t.pi.Products.ProductKeys)
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
        public static List<ProductModel> CheckProductRemainderByStockItems(int stockId)
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
                    //    .Include(t => t.pi.Products.ProductKeys)
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
                        MeasureUnit = CashManager.Instance.MeasureOfUnits.SingleOrDefault(m => m.Id == s.MeasureOfUnitsId),
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
        public static List<ProductModel> CheckProductRemainderItems(int stockId, int days = 120)
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
                    //    .Include(t => t.pi.Products.ProductKeys)
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
                        MeasureUnit = CashManager.Instance.MeasureOfUnits.SingleOrDefault(m => m.Id == s.MeasureOfUnitsId),
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
            var reports = new List<IInvoiceReport>();

            try
            {
                using (var db = GetDataContext())
                {
                    var pi = db.ProductItems.Where(s => s.MemberId == ApplicationManager.Member.Id).Include(s => s.Products).Where(s => s.Quantity > 0);
                    var saleInvoiceItems =
                        db.InvoiceItems.Where(
                            s => s.Invoices.MemberId == ApplicationManager.Member.Id && s.Invoices.ApproveDate > date &&
                                 (s.Invoices.InvoiceTypeId == (int)InvoiceType.SaleInvoice ||
                                  s.Invoices.InvoiceTypeId == (int)InvoiceType.InventoryWriteOff ||
                                  s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnTo)).ToList();
                    var salePrice = saleInvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0) + (s.Discount ?? 0));
                    var saleCostPrice = saleInvoiceItems.Sum(s => (s.CostPrice ?? 0) * (s.Quantity ?? 0));

                    var purchaseInvoiceItems = db.InvoiceItems.Where(s => s.Invoices.MemberId == ApplicationManager.Member.Id && s.Invoices.ApproveDate > date &&
                        (s.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice ||
                        s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnFrom)).ToList();
                    var purchasePrice = purchaseInvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0) + (s.Discount ?? 0));
                    var purchaseCostPrice = purchaseInvoiceItems.Sum(s => (s.CostPrice ?? 0) * (s.Quantity ?? 0));

                    var stocks = ApplicationManager.CashManager.GetStocks;
                    foreach (var stock in stocks)
                    {

                        var productItems = pi.Where(s => s.StockId == stock.Id).ToList();
                        reports.Add(new InvoiceReport
                        {
                            Description = string.Format("{0}", stock.FullName),
                            Count = productItems.GroupBy(t => t.ProductId).Count(),
                            Quantity = productItems.Sum(t => t.Quantity),
                            Cost = productItems.Sum(t => t.Quantity * t.CostPrice) + saleCostPrice - purchaseCostPrice,
                            Price = productItems.Sum(t => t.Quantity * (t.Products.Price ?? 0)) + salePrice - purchasePrice,
                            Sale = productItems.Sum(t => t.Quantity * ((t.Products.Price ?? 0) - t.CostPrice)),
                        });
                    }
                }
            }
            catch (Exception)
            {
                MessageManager.OnMessage("Գործողությունը ձախողվել է:");
            }
            return reports;
        }

        public static List<IInvoiceReport> GetProductsByStock(DateTime? date, List<StockModel> stocks)
        {
            using (var db = GetDataContext())
            {
                var result = new List<IInvoiceReport>();
                try
                {
                    if (date == null || stocks == null) return null;
                    var stockIds = stocks.Select(s => s.Id).ToList();
                    var products = db.Products.Where(s => s.EsMemberId == MemberId).ToList();
                    var pI = db.ProductItems.Where(s => s.MemberId == ApplicationManager.Member.Id && s.Quantity > 0 && s.StockId.HasValue && stockIds.Contains((short)s.StockId)).Include(s => s.Products)
                        .ToList();
                    //.Join(db.Invoices, pi => pi.DeliveryInvoiceId, i => i.Id, (pi, i) => new { pi, i }).
                    //Where(s =>s.i.CreateDate > date).GroupBy(t => t.pi.ProductId).ToList();

                    var invoiceItems = db.InvoiceItems.Where(ii => ii.Invoices.ApproveDate > date).Include(ii => ii.Invoices).ToList();

                    foreach (var product in products)
                    {
                        var invoiceItemsCur = invoiceItems.Where(ii => ii.ProductId == product.Id).ToList();
                        var item = new InvoiceReport
                        {
                            Code = product.Code,
                            Description = product.Description,
                            Price = product.Price ?? 0,

                            Quantity = pI.Where(s => s.ProductId == product.Id).Sum(t => t.Quantity)// + invoiceItemsCur.Sum(t => GetInvoiceCoefficient(t.Invoices, t.StockId, stockIds) * (t.Quantity ?? 0))
                        };
                        if (item.Quantity != 0) result.Add(item);
                    }
                }
                catch (Exception ex)
                {

                }
                return result;
            }
        }
        private static short GetInvoiceCoefficient(Invoices invoice, short? stockId, List<short> stocks)
        {
            short coefficient = 0;
            if (!stockId.HasValue) return coefficient;
            switch ((InvoiceType)invoice.InvoiceTypeId)
            {
                case InvoiceType.PurchaseInvoice:
                case InvoiceType.ReturnFrom:
                    if (stocks.Any(s => s == stockId)) coefficient++;
                    break;
                case InvoiceType.SaleInvoice:
                case InvoiceType.ReturnTo:
                case InvoiceType.InventoryWriteOff:
                    if (stocks.Any(s => s == stockId)) coefficient++;
                    break;
                case InvoiceType.MoveInvoice:
                    if (stocks.Any(s => s == invoice.ToStockId)) coefficient--;
                    if (stocks.Any(s => s == invoice.FromStockId)) coefficient++;
                    break;
                case InvoiceType.ProductOrder:
                    break;
            }
            return coefficient;
        }

        public static decimal? GetProductQuantity(Guid id)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var productItems = db.ProductItems.Where(s => s.MemberId == ApplicationManager.Member.Id && s.ProductId == id);
                    return productItems.Any() ? productItems.Sum(s => s.Quantity) : 0;
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage(ex.Message);
                    return null;
                }
            }
        }
        public static decimal GetProductQuantityFromInvoices(Guid id, DateTime? date)
        {
            using (var db = GetDataContext())
            {
                try
                {

                    var exCount = db.ProductItems.Where(s => s.MemberId == ApplicationManager.Member.Id && s.ProductId == id).ToList().Sum(s => s.Quantity);
                    exCount +=
                        db.InvoiceItems.Where(s =>
                                s.Invoices.MemberId == ApplicationManager.Member.Id &&
                                s.Invoices.ApproveDate >= date &&
                                (s.Invoices.InvoiceTypeId == (int)InvoiceType.SaleInvoice ||
                                s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnTo ||
                                s.Invoices.InvoiceTypeId == (int)InvoiceType.InventoryWriteOff) &&
                                s.ProductId == id).ToList().Sum(s => s.Quantity ?? 0);

                    exCount -=
                        db.InvoiceItems.Where(s =>
                                s.Invoices.MemberId == ApplicationManager.Member.Id &&
                                s.Invoices.ApproveDate >= date &&
                                (s.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice ||
                                s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnFrom) &&
                                s.ProductId == id).ToList().Sum(s => s.Quantity ?? 0);
                    return exCount;
                }
                catch (Exception ex)
                {
                    MessageManager.OnMessage(ex.ToString());
                    return 0;
                }

            }
        }

        public static void RemoveProducts(List<ProductModel> products)
        {
            if (products.Any(s => TryRemoveProduct(s)))
            {

            }
        }

        private static bool TryRemoveProduct(ProductModel productModel)
        {
            using (var db = GetDataContext())
            {
                var exProduct = db.Products.SingleOrDefault(s => s.Id == productModel.Id && s.EsMemberId == productModel.EsMemberId);
                if (exProduct == null || db.ProductItems.Any(s => s.ProductId == productModel.Id)) return false;
                using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 3, 0)))
                {
                    try
                    {
                        db.Products.Remove(exProduct);
                        var log = new LogForProducts
                        {
                            Action = (short)ModificationTypeEnum.Removed,
                            ProductId = exProduct.Id,
                            Code = exProduct.Code,
                            Description = exProduct.Description,
                            CostPrice = exProduct.CostPrice,
                            Price = exProduct.Price,
                            IsEmpty = true,
                            ModifierId = ApplicationManager.ActiveUserId,
                            MemberId = exProduct.EsMemberId
                        };
                        AddProductChangedLog(log, db);
                        transaction.Complete();
                        MessageManager.OnMessage(string.Format("'{0} ({1})' ապրանքի հեռացումը հաջողվել է", productModel.Description, productModel.Code), MessageTypeEnum.Success);
                        return true;
                    }
                    catch (Exception e)
                    {
                        MessageManager.OnMessage(string.Format("'{0} ({1})' ապրանքի հեռացումը ձախողվել է", productModel.Description, productModel.Code), MessageTypeEnum.Warning);
                        return false;
                    }

                }
            }
        }

        public static Guid? GetLastProvider(Guid productModelId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var invoiceId = db.ProductItems.Where(s => s.MemberId == ApplicationManager.Member.Id && s.Quantity > 0 && s.ProductId == productModelId).OrderByDescending(s => s.CreatedDate).Select(s => s.CreatedInvoiceId).FirstOrDefault();
                    return db.Invoices.Where(s => s.Id == invoiceId).Select(s => s.PartnerId).SingleOrDefault();
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public static List<LogForProducts> GetProductsLog(Tuple<DateTime?, DateTime?> startEndDate)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var productsLog = db.LogForProducts.Where(s => s.MemberId == ApplicationManager.Member.Id &&
                                                                   (!startEndDate.Item1.HasValue || s.Date >= startEndDate.Item1) &&
                                                                   (!startEndDate.Item2.HasValue || s.Date <= startEndDate.Item2))
                        .OrderBy(s => s.Date).ToList();
                    return productsLog;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        public static DateTime? GetProductsCreationDate(Guid pId, DateTime? startDate)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    var productsLogs = db.LogForProducts
                                         .Where(s => s.MemberId == ApplicationManager.Member.Id
                                                   && (!startDate.HasValue || startDate <= s.Date)
                                                   && s.IsEmpty && s.ProductId == pId)
                                         .OrderByDescending(s => s.Date).ToList();

                    if (!productsLogs.Any())
                    {
                        return db.InvoiceItems.Where(s => s.ProductId == pId).Select(s => s.Invoices.CreateDate).OrderBy(s => s).FirstOrDefault();
                    }
                    else if (productsLogs.Any(s => s.Action == (short)ModificationTypeEnum.Added || s.Action == (short)ModificationTypeEnum.ReCreated))
                    {
                        return productsLogs.Last(s => s.Action == (short)ModificationTypeEnum.Added || s.Action == (short)ModificationTypeEnum.ReCreated).Date;
                    }
                    else
                    {
                        DateTime dateTime = productsLogs[0].Date;
                        for (int i = 0; i < productsLogs.Count - 1; i++)
                        {
                            if (productsLogs[i].Code != productsLogs[i + 1].Code)
                                return productsLogs[i].Date;
                            else if (productsLogs[i].Description != productsLogs[i + 1].Description && productsLogs[i].Price != productsLogs[i + 1].Price)
                                return productsLogs[i].Date;
                        }
                        return productsLogs.Last().Date;
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
    }
}
