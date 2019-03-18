using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Windows;
using AccountingTools.Enums;
using EsMarket.SharedData.Interfaces;
using EsMarket.SharedData.Models;
using ES.Business.Helpers;
using ES.Business.Models;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Data.Enumerations;
using ES.Data.Helper;
using ES.Data.Models;
using ES.Data.Models.Reports;
using ES.DataAccess.Models;
using Microsoft.Win32;
using BankAccount = EsMarket.SharedData.Models.BankAccount;
using ProductModel = ES.Data.Models.ProductModel;


namespace ES.Business.Managers
{
    public enum InvoiceType
    {
        PurchaseInvoice = 1,
        SaleInvoice = 2,
        ProductOrder = 3,
        MoveInvoice = 4,
        InventoryWriteOff = 5,
        ReturnFrom = 6,
        ReturnTo = 7
    }
    [Flags]
    public enum InvoiceTypeEnum
    {
        None = 0,
        PurchaseInvoice = 1,
        SaleInvoice = 2,
        ProductOrder = 4,
        MoveInvoice = 8,
        InventoryWriteOff = 16,
        ReturnFrom = 32,
        ReturnTo = 64,
        Statements = InventoryWriteOff | ReturnFrom | ReturnTo
    }
    public enum InvoiceState
    {
        All,
        New,
        Draft,
        Accepted,
        Approved
    }

    public enum OrderInvoiceBy
    {
        Approver,
        CreatedDate,
        ApprovedDate
    }
    public enum MaxInvocieCount
    {
        SmallCount = 100,
        BigCount = 1000,
        All
    }

    public class InvoicesManager : BaseManager
    {
        private static long MemberId
        {
            get { return ApplicationManager.Member.Id; }
        }

        #region Invoice enumerables

        public static List<int> GetInvoiceTypes(InvoiceTypeEnum type)
        {
            var types = new List<int>();
            if ((type & InvoiceTypeEnum.SaleInvoice) == InvoiceTypeEnum.SaleInvoice)
                types.Add((int)InvoiceType.SaleInvoice);
            if ((type & InvoiceTypeEnum.PurchaseInvoice) == InvoiceTypeEnum.PurchaseInvoice)
                types.Add((int)InvoiceType.PurchaseInvoice);
            if ((type & InvoiceTypeEnum.ProductOrder) == InvoiceTypeEnum.ProductOrder)
                types.Add((int)InvoiceType.ProductOrder);
            if ((type & InvoiceTypeEnum.MoveInvoice) == InvoiceTypeEnum.MoveInvoice)
                types.Add((int)InvoiceType.MoveInvoice);
            if ((type & InvoiceTypeEnum.InventoryWriteOff) == InvoiceTypeEnum.InventoryWriteOff)
                types.Add((int)InvoiceType.InventoryWriteOff);
            if ((type & InvoiceTypeEnum.ReturnFrom) == InvoiceTypeEnum.ReturnFrom)
                types.Add((int)InvoiceType.ReturnFrom);
            if ((type & InvoiceTypeEnum.ReturnTo) == InvoiceTypeEnum.ReturnTo) types.Add((int)InvoiceType.ReturnTo);
            return types;
        }

        #endregion

        #region Invoice converters

        private static Invoices ConvertInvoice(InvoiceModel invoice)
        {
            if (invoice == null) return null;
            return new Invoices
            {
                Id = invoice.Id,
                MemberId = invoice.MemberId,
                InvoiceTypeId = invoice.InvoiceTypeId,
                InvoiceNumber = invoice.InvoiceNumber,
                FromStockId = invoice.FromStockId,
                ToStockId = invoice.ToStockId,
                CreatorId = invoice.CreatorId,
                Creator = invoice.Creator,
                CreateDate = invoice.CreateDate,
                ApproveDate = invoice.ApproveDate,
                ApproverId = invoice.ApproverId,
                Approver = invoice.Approver,
                AcceptDate = invoice.AcceptDate,
                AccepterId = invoice.AccepterId,
                PartnerId = invoice.PartnerId,
                ProviderName = invoice.ProviderName,
                ProviderJuridicalAddress = invoice.ProviderJuridicalAddress,
                ProviderAddress = invoice.ProviderAddress,
                ProviderBank = invoice.ProviderBank,
                ProviderBankAccount = invoice.ProviderBankAccount,
                ProviderTaxRegistration = invoice.ProviderTaxRegistration,
                RecipientName = invoice.RecipientName,
                RecipientJuridicalAddress = invoice.RecipientJuridicalAddress,
                RecipientAddress = invoice.RecipientAddress,
                RecipientBank = invoice.RecipientBank,
                RecipientBankAccount = invoice.RecipientBankAccount,
                RecipientTaxRegistration = invoice.RecipientTaxRegistration,
                Discount = invoice.Discount,
                Summ = invoice.Total,
                Notes = invoice.Notes
            };
        }

        private static InvoiceModel ConvertInvoice(Invoices invoice, PartnerModel partner)
        {
            if (invoice == null) return null;

            return new InvoiceModel
            {
                Id = invoice.Id,
                MemberId = invoice.MemberId,
                InvoiceTypeId = invoice.InvoiceTypeId,
                InvoiceNumber = invoice.InvoiceNumber,
                FromStockId = invoice.FromStockId,
                ToStockId = invoice.ToStockId,
                CreatorId = invoice.CreatorId,
                Creator = invoice.Creator,
                CreateDate = invoice.CreateDate,
                ApproveDate = invoice.ApproveDate,
                ApproverId = invoice.ApproverId,
                Approver = invoice.Approver,
                AcceptDate = invoice.AcceptDate,
                AccepterId = invoice.AccepterId,
                PartnerId = invoice.PartnerId,
                Partner = partner,
                ProviderName = invoice.ProviderName,
                ProviderJuridicalAddress = invoice.ProviderJuridicalAddress,
                ProviderAddress = invoice.ProviderAddress,
                ProviderBank = invoice.ProviderBank,
                ProviderBankAccount = invoice.ProviderBankAccount,
                ProviderTaxRegistration = invoice.ProviderTaxRegistration,
                RecipientName = invoice.RecipientName,
                RecipientJuridicalAddress = invoice.RecipientJuridicalAddress,
                RecipientAddress = invoice.RecipientAddress,
                RecipientBank = invoice.RecipientBank,
                RecipientBankAccount = invoice.RecipientBankAccount,
                RecipientTaxRegistration = invoice.RecipientTaxRegistration,
                Discount = invoice.Discount,
                Total = invoice.Summ,
                Notes = invoice.Notes
            };
        }

        private static InvoiceItemsModel ConvertInvoiceItem(InvoiceItems item, List<ProductModel> products,
            List<ProductItemModel> productItems)
        {
            if (item == null) return null;
            return new InvoiceItemsModel
            {
                Id = item.Id,
                InvoiceId = item.InvoiceId,
                ProductId = item.ProductId,
                Index = item.Index ?? 0,
                Product = products.SingleOrDefault(s => s.Id == item.ProductId),
                ProductItemId = item.ProductItemId,
                ProductItem =
                    item.ProductItemId != null ? productItems.FirstOrDefault(s => s.Id == item.ProductItemId) : null,
                Code = item.Code,
                Description = item.Description,
                Mu = item.Mu,
                Quantity = item.Quantity,
                Price = item.Price,
                CostPrice = item.CostPrice,
                Discount = item.Discount,
                Note = item.Note
            };
        }

        private static InvoiceItemsModel Convert(InvoiceItems item)
        {
            if (item == null) return null;
            return new InvoiceItemsModel
            {
                Id = item.Id,
                Index = item.Index ?? 0,
                InvoiceId = item.InvoiceId,
                Invoice = ConvertInvoice(item.Invoices, new PartnerModel()),
                ProductId = item.ProductId,
                Product = ProductsManager.Convert(item.Products),
                ProductItemId = item.ProductItemId,
                ProductItem = ProductsManager.Convert(item.ProductItems),
                Code = item.Code,
                Description = item.Description,
                Mu = item.Mu,
                Quantity = item.Quantity,
                Price = item.Price,
                CostPrice = item.CostPrice,
                Discount = item.Discount,
                Note = item.Note
            };
        }

        private static InvoiceItemsModel ConvertLite(InvoiceItems item)
        {
            if (item == null) return null;
            return new InvoiceItemsModel
            {
                Id = item.Id,
                Index = item.Index ?? 0,
                InvoiceId = item.InvoiceId,
                //Invoice = new InvoiceModel(),
                ProductId = item.ProductId,
                //Product = ProductsManager.Convert(item.Products),
                ProductItemId = item.ProductItemId,
                //ProductItem = new ProductsManager().Convert(item.ProductItems),
                Code = item.Code,
                Description = item.Description,
                Mu = item.Mu,
                Quantity = item.Quantity,
                Price = item.Price,
                CostPrice = item.CostPrice,
                Discount = item.Discount,
                Note = item.Note
            };
        }

        private static InvoiceItems Convert(InvoiceItemsModel item)
        {
            if (item == null) return null;
            return new InvoiceItems
            {
                Id = item.Id,
                InvoiceId = item.InvoiceId,
                ProductId = item.ProductId,
                Index = item.Index,
                ProductItemId = item.ProductItemId,
                Code = item.Code,
                Description = item.Description,
                Mu = item.Mu,
                Quantity = item.Quantity,
                Price = item.Price,
                CostPrice = item.CostPrice,
                Discount = item.Discount,
                Note = item.Note
            };
        }

        #endregion

        #region Invoices Internal methods

        private static Int64 GetNextInvoiceIndex(long invoiceTypeId, EsStockDBEntities db)
        {
            try
            {
                var memberId = MemberId;
                //var invoices = db.Invoices.Where(s => s.MemberId == memberId && s.InvoiceTypeId == invoiceTypeId);
                //var index = invoices.Max(s => s.InvoiceIndex);
                return
                    db.Invoices.Where(s => s.MemberId == memberId && s.InvoiceTypeId == invoiceTypeId)
                        .Max(s => s.InvoiceIndex) + 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private static Invoices TryGetInvoice(Guid? id)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.Invoices.SingleOrDefault(s => s.Id == id && s.MemberId == ApplicationManager.Member.Id);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string TryGetInvoiceNumber(Guid? id, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return
                        db.Invoices.Where(s => s.Id == id && s.MemberId == memberId)
                            .Select(s => s.InvoiceNumber)
                            .FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<Invoices> TryGetInvoices(IEnumerable<Guid> ids)
        {
            var memberId = ApplicationManager.Member.Id;
            try
            {
                using (var db = GetDataContext())
                {
                    return db.Invoices.Where(s => ids.Contains(s.Id) && s.MemberId == memberId).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }

        private static List<Invoices> TryGetUnaccepedInvoices(InvoiceType invoiceType, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return
                        db.Invoices.Where(
                            s =>
                                s.InvoiceTypeId == (long)invoiceType && s.MemberId == memberId && s.ApproveDate == null)
                            .ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }

        private static List<Invoices> TryGetInvoices(InvoiceType invoiceType)
        {
            var memberId = ApplicationManager.Member.Id;
            try
            {
                using (var db = GetDataContext())
                {
                    return
                        db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.MemberId == memberId).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }

        private static List<Invoices> TryGetUnApprovedInvoices(InvoiceType invoiceType, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return
                        db.Invoices.Where(
                            s =>
                                s.InvoiceTypeId == (long)invoiceType && s.ApproveDate == null && s.MemberId == memberId)
                            .ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }

        private static List<Invoices> TryGetInvoices(InvoiceTypeEnum invoiceType, int? maxCount)
        {
            try
            {
                var types = GetInvoiceTypes(invoiceType);
                using (var db = GetDataContext())
                {
                    var items = db.Invoices.Where(s =>
                        types.Contains((int)s.InvoiceTypeId) && s.MemberId == ApplicationManager.Member.Id)
                        .OrderByDescending(s => s.CreateDate)
                        .ThenBy(s => s.ApproveDate);
                    return maxCount != null ? items.Take((int)maxCount).ToList() : items.ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }

        private static List<Invoices> TryGetApprovedInvoices(InvoiceTypeEnum invoiceType, int? maxCount)
        {
            try
            {
                var types = GetInvoiceTypes(invoiceType);
                using (var db = GetDataContext())
                {
                    var items = db.Invoices.Where(s =>
                        types.Contains((int)s.InvoiceTypeId) && s.ApproveDate != null &&
                        s.MemberId == ApplicationManager.Member.Id).OrderByDescending(s => s.ApproveDate);
                    return maxCount != null ? items.Take((int)maxCount).ToList() : items.ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }

        private static List<Invoices> TryGetInvoices(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var memberId = MemberId;
                using (var db = GetDataContext())
                {
                    if (startDate == null) startDate = DateTime.Today;
                    if (endDate == null) endDate = DateTime.Today.AddDays(1);

                    return db.Invoices.Where(s =>
                        s.ApproveDate.HasValue &&
                        s.ApproveDate >= startDate &&
                        s.ApproveDate < endDate &&
                        s.MemberId == memberId)
                        .OrderByDescending(s => s.InvoiceIndex)
                        .Include(s => s.InvoiceItems)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<Invoices>();
            }
        }

        private static List<InvoiceItems> TryGetInvoiceItems(Guid invoiceId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.InvoiceItems
                        .Include(s => s.Invoices)
                        .Include(s => s.Products)
                        .Include(s => s.ProductItems)
                        .Include(s => s.Products.ProductCategories)
                        .Include(s => s.Products.ProductGroup)
                        .Include(s => s.Products.ProductsAdditionalData)
                        .Where(s => s.InvoiceId == invoiceId)
                        .OrderBy(s => s.Code)
                        .ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }

        private static List<InvoiceItems> TryGetInvoiceItems(IEnumerable<Guid> invoiceIds)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.InvoiceItems
                        .Include(s => s.Invoices)
                        .Include(s => s.Products).Include(s => s.ProductItems)
                        .Include(s => s.Products.ProductCategories)
                        .Include(s => s.Products.ProductGroup)
                        .Include(s => s.Products.ProductsAdditionalData)
                        .Where(s => invoiceIds.Contains(s.InvoiceId)).OrderBy(s => s.Code).ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }

        private static List<InvoiceItems> TryGetInvoiceItemsByStocks(List<Guid> invoiceIds, List<StockModel> stocks)
        {
            var stockids = stocks.Select(st => st.Id).ToList();
            try
            {
                using (var db = GetDataContext())
                {
                    return db.InvoiceItems
                        .Include(s => s.Invoices)
                        .Include(s => s.Products)
                        .Include(s => s.ProductItems)
                        .Include(s => s.Products.ProductGroup)
                        .Include(s => s.Products.ProductCategories)
                        .Where(s => invoiceIds.Contains(s.InvoiceId) && stockids.Contains(s.ProductItems.StockId ?? 0))
                        .OrderBy(s => s.Code)
                        .ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invoiceIndex"></param>
        /// <param name="invoiceTypeId"></param>
        /// <returns></returns>
        private static string GetInvocieNumber(long invoiceIndex, long invoiceTypeId)
        {
            switch ((InvoiceType)invoiceTypeId)
            {
                case InvoiceType.SaleInvoice:
                    return string.Format("{0}{1}", "SI", invoiceIndex);
                    break;
                case InvoiceType.PurchaseInvoice:
                    return string.Format("{0}{1}", "PI", invoiceIndex);
                    break;
                case InvoiceType.MoveInvoice:
                    return string.Format("{0}{1}", "MI", invoiceIndex);
                    break;
                case InvoiceType.ProductOrder:
                    return string.Format("{0}{1}", "PO", invoiceIndex);
                    break;
                case InvoiceType.InventoryWriteOff:
                    return string.Format("{0}{1}", "WO", invoiceIndex);
                    break;
                case InvoiceType.ReturnFrom:
                    return string.Format("{0}{1}", "RF", invoiceIndex);
                    break;
                case InvoiceType.ReturnTo:
                    return string.Format("{0}{1}", "RT", invoiceIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("invoiceTypeId", invoiceTypeId, null);
            }
            return null;
        }

        private static string GetInvoiceNumber(long invoiceTypeId, EsStockDBEntities db)
        {
            var invoiceIndex = GetNextInvoiceIndex(invoiceTypeId, db);
            return GetInvocieNumber(invoiceIndex, invoiceTypeId);
        }

        /// <summary>
        /// Save invoice
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="invoiceItems"></param>
        /// <returns></returns>
        private static bool TrySaveInvoice(Invoices invoice, List<InvoiceItems> invoiceItems)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 5, 0)))
            {
                using (var db = GetDataContext())
                {
                    #region Edit invoice

                    var exInvoice =
                        db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);
                    if (exInvoice == null)
                    {
                        invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, db);
                        invoice.InvoiceNumber = GetInvocieNumber(invoice.InvoiceIndex, invoice.InvoiceTypeId);
                        db.Invoices.Add(invoice);
                    }
                    else
                    {
                        if (exInvoice.ApproveDate != null)
                        {
                            MessageManager.OnMessage(
                                string.Format("Հաստատված ապրանքագիր ({0})", exInvoice.InvoiceNumber),
                                MessageTypeEnum.Information);
                            return false;
                        }

                        exInvoice.InvoiceTypeId = invoice.InvoiceTypeId;
                        exInvoice.FromStockId = invoice.FromStockId;
                        exInvoice.ToStockId = invoice.ToStockId;
                        exInvoice.Creator = invoice.Creator;
                        exInvoice.CreateDate = invoice.CreateDate;
                        exInvoice.AcceptDate = invoice.AcceptDate;
                        exInvoice.AccepterId = invoice.AccepterId;
                        exInvoice.PartnerId = invoice.PartnerId;
                        exInvoice.ProviderName = invoice.ProviderName;
                        exInvoice.ProviderJuridicalAddress = invoice.ProviderJuridicalAddress;
                        exInvoice.ProviderAddress = invoice.ProviderAddress;
                        exInvoice.ProviderBank = invoice.ProviderBank;
                        exInvoice.ProviderBankAccount = invoice.ProviderBankAccount;
                        exInvoice.ProviderTaxRegistration = invoice.ProviderTaxRegistration;
                        exInvoice.RecipientName = invoice.RecipientName;
                        exInvoice.RecipientJuridicalAddress = invoice.RecipientJuridicalAddress;
                        exInvoice.RecipientAddress = invoice.RecipientAddress;
                        exInvoice.RecipientBank = invoice.RecipientBank;
                        exInvoice.RecipientBankAccount = invoice.RecipientBankAccount;
                        exInvoice.RecipientTaxRegistration = invoice.RecipientTaxRegistration;
                        exInvoice.Discount = invoice.Discount;
                        exInvoice.Summ = invoice.Summ;
                        exInvoice.Notes = invoice.Notes;
                    }

                    #endregion Edit Invoice

                    #region Edit InvoiceItems

                    var exInvoiceItems = db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).ToList();
                    foreach (var exInvoiceItem in exInvoiceItems)
                    {
                        var exItem = invoiceItems.FirstOrDefault(s => s.Id == exInvoiceItem.Id);

                        if (exItem != null)
                        {
                            exItem.InvoiceId = invoice.Id;
                            CopyInvoiceItem(exItem, exInvoiceItem);
                            invoiceItems.RemoveAt(invoiceItems.IndexOf(exItem));
                        }
                        else
                        {
                            db.InvoiceItems.Remove(exInvoiceItem);
                        }
                    }
                    foreach (var invoiceItem in invoiceItems)
                    {
                        //todo check posibility if exItem.InvoiceId != invoice.Id
                        invoiceItem.InvoiceId = invoice.Id;
                        db.InvoiceItems.Add(invoiceItem);
                    }

                    #endregion Edit InvoiceItems

                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        invoice.ApproveDate = null;
                        MessageManager.OnMessage(ex.ToString());
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Approve purchase invoice
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="invoiceItems"></param>
        /// <param name="stockId"></param>
        /// <param name="invoicePaid"></param>
        /// <returns></returns>
        private static Invoices TryApprovePurchaseInvoice(Invoices invoice, List<InvoiceItems> invoiceItems,
            long stockId, InvoicePaid invoicePaid)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 3, 0)))
            {
                using (var db = GetDataContext())
                {
                    invoice.ApproveDate = DateTime.Now;

                    #region Update purchase invoice

                    var exInvoice =
                        db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);

                    if (exInvoice == null)
                    {
                        invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, db);
                        switch (invoice.InvoiceTypeId)
                        {
                            case (long)InvoiceType.SaleInvoice:
                                return null;
                            case (long)InvoiceType.PurchaseInvoice:
                                invoice.InvoiceNumber = string.Format("{0}{1}", "PI", invoice.InvoiceIndex);
                                break;
                        }
                        invoice.ApproveDate = invoice.CreateDate = DateTime.Now;
                        db.Invoices.Add(invoice);
                    }
                    else
                    {
                        if (exInvoice.ApproveDate != null)
                        {
                            return null;
                        }
                        exInvoice.FromStockId = invoice.FromStockId;
                        exInvoice.ToStockId = invoice.ToStockId;
                        exInvoice.ApproveDate = DateTime.Now;
                        exInvoice.ApproverId = invoice.ApproverId;
                        exInvoice.Approver = invoice.Approver;
                        exInvoice.AcceptDate = invoice.AcceptDate;
                        exInvoice.AccepterId = invoice.AccepterId;
                        exInvoice.PartnerId = invoice.PartnerId;
                        exInvoice.ProviderName = invoice.ProviderName;
                        exInvoice.ProviderJuridicalAddress = invoice.ProviderJuridicalAddress;
                        exInvoice.ProviderAddress = invoice.ProviderAddress;
                        exInvoice.ProviderBank = invoice.ProviderBank;
                        exInvoice.ProviderBankAccount = invoice.ProviderBankAccount;
                        exInvoice.ProviderTaxRegistration = invoice.ProviderTaxRegistration;
                        exInvoice.RecipientName = invoice.RecipientName;
                        exInvoice.RecipientJuridicalAddress = invoice.RecipientJuridicalAddress;
                        exInvoice.RecipientAddress = invoice.RecipientAddress;
                        exInvoice.RecipientBank = invoice.RecipientBank;
                        exInvoice.RecipientBankAccount = invoice.RecipientBankAccount;
                        exInvoice.RecipientTaxRegistration = invoice.RecipientTaxRegistration;
                        exInvoice.Discount = invoice.Discount;
                        exInvoice.Summ = invoice.Summ;
                        exInvoice.Notes = invoice.Notes;
                    }

                    #region Edit Invoice items

                    var exInvoiceItems =
                        db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).OrderBy(s => s.Index).ToList();

                    foreach (var exInvoiceItem in exInvoiceItems)
                    {
                        var exItem = invoiceItems.FirstOrDefault(s => s.Id == exInvoiceItem.Id);

                        if (exItem != null && exItem.Quantity != null && exItem.Quantity != 0)
                        {
                            exInvoiceItem.Code = exItem.Code;
                            exInvoiceItem.CostPrice = exItem.CostPrice;
                            exInvoiceItem.Description = exItem.Description;
                            exInvoiceItem.Discount = exItem.Discount;
                            exInvoiceItem.ExpiryDate = exItem.ExpiryDate;
                            exInvoiceItem.Index = exItem.Index;
                            //todo check posibility if exItem.InvoiceId != invoice.Id
                            exInvoiceItem.InvoiceId = exItem.InvoiceId = invoice.Id;
                            exInvoiceItem.Mu = exItem.Mu;
                            exInvoiceItem.Note = exItem.Note;
                            exInvoiceItem.Price = exItem.Price;
                            exInvoiceItem.ProductId = exItem.ProductId;
                            exInvoiceItem.Quantity = exItem.Quantity;
                            var productItem = CreateProductItem(exInvoiceItem, stockId);
                            exInvoiceItem.ProductItemId = productItem.Id;
                            if (exInvoiceItem.Quantity == null || exInvoiceItem.Quantity == 0) continue;
                            db.ProductItems.Add(productItem);

                            invoiceItems.RemoveAt(invoiceItems.IndexOf(exItem));
                        }
                        else
                        {
                            db.InvoiceItems.Remove(exInvoiceItem);
                        }
                    }
                    foreach (var invoiceItem in invoiceItems.OrderBy(s => s.Index))
                    {
                        //todo check posibility if exItem.InvoiceId != invoice.Id
                        invoiceItem.InvoiceId = invoice.Id;
                        db.InvoiceItems.Add(invoiceItem);
                        if (invoiceItem.Quantity == null || invoiceItem.Quantity == 0) continue;
                        var productItem = CreateProductItem(invoiceItem, stockId);
                        invoiceItem.ProductItemId = productItem.Id;
                        db.ProductItems.Add(productItem);
                    }

                    #endregion Edit invoice items

                    #endregion

                    var exPartner = db.Partners.Single(s => s.Id == invoice.PartnerId);
                    if (exPartner.Debit == null) exPartner.Debit = 0;
                    if (exPartner.Credit == null) exPartner.Credit = 0;

                    #region Add Purchase 216 - 521

                    // 216 - 521 Register in AccountingRecoords Accounting Receivable
                    //decimal amount = invoice.Summ;
                    var apAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = invoice.Summ,
                        Debit = (int)AccountingPlanEnum.Purchase,
                        DebitLongId = invoice.ToStockId,
                        Credit = (int)AccountingPlanEnum.PurchasePayables,
                        CreditGuidId = exPartner.Id,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                    };
                    db.AccountingRecords.Add(apAccountingRecords);
                    exPartner.Credit += invoice.Summ;

                    #endregion

                    #region Paid 521 - 251

                    decimal amount = invoicePaid.ByCash;
                    var exCashDesk =
                        db.CashDesk.SingleOrDefault(
                            s => s.Id == invoicePaid.CashDeskId && s.MemberId == invoice.MemberId);

                    if (amount > 0)
                    {
                        if (exCashDesk == null) return null;
                        if (invoicePaid.CashDeskId == null) return null;

                        // 521 - 251 Register in AccountingRecoords
                        var cpAccountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingPlanEnum.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingPlanEnum.CashDesk,
                            CreditGuidId = exCashDesk.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(cpAccountingRecords);
                        exCashDesk.Total -= amount;
                        exPartner.Credit -= amount;


                        //Change in CashDesk add to SaleInCash
                    }

                    #endregion

                    #region Paid by ticket 521 - 251

                    amount = invoicePaid.ByCheck ?? 0;
                    if (amount > 0)
                    {
                        if (invoicePaid.CashDeskForTicketId == null) return null;
                        var exCashDeskByCheck =
                            db.CashDesk.SingleOrDefault(
                                s => s.Id == invoicePaid.CashDeskForTicketId && s.MemberId == invoice.MemberId);
                        if (exCashDeskByCheck == null)
                        {
                            return null;
                        }
                        // 521 - 251 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingPlanEnum.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingPlanEnum.CashDesk,
                            CreditGuidId = exCashDeskByCheck.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exCashDeskByCheck.Total -= amount;
                        exPartner.Credit -= amount;
                    }

                    #endregion

                    #region Prepayment 521 - 224

                    amount = invoicePaid.Prepayment ?? 0;
                    if (amount > 0)
                    {
                        if (exCashDesk == null) return null;
                        // 521 - 224 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingPlanEnum.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingPlanEnum.Prepayments,
                            CreditGuidId = exPartner.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exCashDesk.Total -= amount;
                        exPartner.Credit += amount;
                    }

                    #endregion

                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return invoice;
                    }
                    catch (Exception ex)
                    {
                        invoice.ApproveDate = null;
                        MessageManager.ShowMessage(string.Format("{0} \n {1}", ex.Message,
                            ex.InnerException != null ? ex.InnerException.Message : string.Empty));
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// TryApproveReturnFromInvoice
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="invoiceItems"></param>
        /// <param name="stockId"></param>
        /// <param name="invoicePaid"></param>
        /// <returns></returns>
        private static Invoices TryApproveReturnFromInvoice(Invoices invoice, List<InvoiceItems> invoiceItems,
            long stockId, InvoicePaid invoicePaid)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 3, 0)))
            {
                using (var db = GetDataContext())
                {


                    #region Update purchase invoice

                    var exInvoice = db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);

                    if (exInvoice == null)
                    {
                        invoice.ApproveDate = invoice.CreateDate = DateTime.Now;
                        db.Invoices.Add(invoice);
                    }
                    else
                    {
                        if (exInvoice.ApproveDate != null)
                        {
                            return null;
                        }
                        exInvoice.FromStockId = invoice.FromStockId;
                        exInvoice.ToStockId = invoice.ToStockId;
                        exInvoice.ApproveDate = invoice.ApproveDate = DateTime.Now;
                        exInvoice.ApproverId = invoice.ApproverId;
                        exInvoice.Approver = invoice.Approver;
                        exInvoice.AcceptDate = invoice.AcceptDate;
                        exInvoice.AccepterId = invoice.AccepterId;
                        exInvoice.PartnerId = invoice.PartnerId;
                        exInvoice.ProviderName = invoice.ProviderName;
                        exInvoice.ProviderJuridicalAddress = invoice.ProviderJuridicalAddress;
                        exInvoice.ProviderAddress = invoice.ProviderAddress;
                        exInvoice.ProviderBank = invoice.ProviderBank;
                        exInvoice.ProviderBankAccount = invoice.ProviderBankAccount;
                        exInvoice.ProviderTaxRegistration = invoice.ProviderTaxRegistration;
                        exInvoice.RecipientName = invoice.RecipientName;
                        exInvoice.RecipientJuridicalAddress = invoice.RecipientJuridicalAddress;
                        exInvoice.RecipientAddress = invoice.RecipientAddress;
                        exInvoice.RecipientBank = invoice.RecipientBank;
                        exInvoice.RecipientBankAccount = invoice.RecipientBankAccount;
                        exInvoice.RecipientTaxRegistration = invoice.RecipientTaxRegistration;
                        exInvoice.Discount = invoice.Discount;
                        exInvoice.Summ = invoice.Summ;
                        exInvoice.Notes = invoice.Notes;
                    }

                    #region Edit Invoice items

                    var exInvoiceItems =
                        db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).OrderBy(s => s.Index).ToList();

                    foreach (var exInvoiceItem in exInvoiceItems)
                    {
                        var exItem = invoiceItems.FirstOrDefault(s => s.Id == exInvoiceItem.Id);

                        if (exItem != null && exItem.Quantity != null && exItem.Quantity != 0)
                        {
                            exInvoiceItem.Code = exItem.Code;
                            exInvoiceItem.CostPrice = exItem.CostPrice;
                            exInvoiceItem.Description = exItem.Description;
                            exInvoiceItem.Discount = exItem.Discount;
                            exInvoiceItem.ExpiryDate = exItem.ExpiryDate;
                            exInvoiceItem.Index = exItem.Index;
                            //todo check posibility if exItem.InvoiceId != invoice.Id
                            exInvoiceItem.InvoiceId = exItem.InvoiceId = invoice.Id;
                            exInvoiceItem.Mu = exItem.Mu;
                            exInvoiceItem.Note = exItem.Note;
                            exInvoiceItem.Price = exItem.Price;
                            exInvoiceItem.ProductId = exItem.ProductId;
                            exInvoiceItem.Quantity = exItem.Quantity;
                            var productItem = CreateProductItem(exInvoiceItem, stockId);
                            exInvoiceItem.ProductItemId = productItem.Id;
                            if (exInvoiceItem.Quantity == null || exInvoiceItem.Quantity == 0) continue;
                            db.ProductItems.Add(productItem);

                            invoiceItems.RemoveAt(invoiceItems.IndexOf(exItem));
                        }
                        else
                        {
                            db.InvoiceItems.Remove(exInvoiceItem);
                        }
                    }
                    foreach (var invoiceItem in invoiceItems.OrderBy(s => s.Index))
                    {
                        //todo check posibility if exItem.InvoiceId != invoice.Id
                        invoiceItem.InvoiceId = invoice.Id;
                        db.InvoiceItems.Add(invoiceItem);
                        if (invoiceItem.Quantity == null || invoiceItem.Quantity == 0) continue;
                        var productItem = CreateProductItem(invoiceItem, stockId);
                        invoiceItem.ProductItemId = productItem.Id;
                        db.ProductItems.Add(productItem);
                    }

                    #endregion Edit invoice items

                    #endregion

                    var exPartner = db.Partners.Single(s => s.Id == invoice.PartnerId);
                    if (exPartner.Debit == null) exPartner.Debit = 0;
                    if (exPartner.Credit == null) exPartner.Credit = 0;

                    #region Add Purchase 216 - 521

                    // 216 - 521 Register in AccountingRecoords Accounting Receivable
                    //decimal amount = invoice.Summ;
                    var apAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = invoice.Summ,
                        Debit = (int)AccountingPlanEnum.Purchase,
                        DebitLongId = invoice.ToStockId,
                        Credit = (int)AccountingPlanEnum.PurchasePayables,
                        CreditGuidId = exPartner.Id,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                    };
                    db.AccountingRecords.Add(apAccountingRecords);
                    exPartner.Credit += invoice.Summ;

                    #endregion

                    #region Paid 521 - 251

                    decimal amount = invoicePaid.ByCash;
                    if (amount > 0)
                    {
                        if (invoicePaid.CashDeskId == null) return null;
                        var exCashDesk =
                            db.CashDesk.SingleOrDefault(
                                s => s.Id == invoicePaid.CashDeskId && s.MemberId == invoice.MemberId);
                        if (exCashDesk == null) return null;
                        // 521 - 251 Register in AccountingRecoords
                        var cpAccountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingPlanEnum.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingPlanEnum.CashDesk,
                            CreditGuidId = exCashDesk.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(cpAccountingRecords);
                        exCashDesk.Total -= amount;
                        exPartner.Credit -= amount;


                        //Change in CashDesk add to SaleInCash
                    }

                    #endregion

                    #region Paid by ticket 521 - 251

                    amount = invoicePaid.ByCheck ?? 0;
                    if (amount > 0)
                    {
                        if (invoicePaid.CashDeskForTicketId == null) return null;
                        var exCashDeskByCheck =
                            db.CashDesk.SingleOrDefault(
                                s => s.Id == invoicePaid.CashDeskForTicketId && s.MemberId == invoice.MemberId);
                        if (exCashDeskByCheck == null)
                        {
                            return null;
                        }
                        // 521 - 251 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingPlanEnum.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingPlanEnum.CashDesk,
                            CreditGuidId = exCashDeskByCheck.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exCashDeskByCheck.Total -= amount;
                        exPartner.Credit -= amount;
                    }

                    #endregion

                    #region Prepayment 521 - 224

                    amount = invoicePaid.Prepayment ?? 0;
                    if (amount > 0)
                    {
                        // 521 - 224 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingPlanEnum.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingPlanEnum.Prepayments,
                            CreditGuidId = exPartner.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exPartner.Debit -= amount;
                        exPartner.Credit -= amount;
                    }

                    #endregion

                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return invoice;
                    }
                    catch (Exception ex)
                    {
                        invoice.ApproveDate = null;
                        MessageManager.ShowMessage(string.Format("{0} \n {1}", ex.Message,
                            ex.InnerException != null ? ex.InnerException.Message : string.Empty));
                        return null;
                    }
                }
            }
        }

        private static Invoices TryApproveReturnToInvoice(Invoices invoice, List<InvoiceItems> invoiceItems,
            List<long> fromStockIds, InvoicePaid invoicePaid)
        {
            if (!invoiceItems.Any() || !fromStockIds.Any() || invoicePaid == null) return null;
            using (var transaction = new TransactionScope())
            {
                using (var db = GetDataContext())
                {
                    #region Edit Invoices

                    var exInvoice = db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);
                    if (exInvoice == null)
                    {
                        invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, db);
                        switch ((InvoiceType)invoice.InvoiceTypeId)
                        {
                            case InvoiceType.SaleInvoice:
                                invoice.InvoiceNumber = string.Format("{0}{1}", "SI", invoice.InvoiceIndex);
                                break;
                            case InvoiceType.PurchaseInvoice:
                                return null;

                                break;
                            case InvoiceType.ProductOrder:
                                break;
                            case InvoiceType.MoveInvoice:
                                break;
                            case InvoiceType.InventoryWriteOff:
                                break;
                            case InvoiceType.ReturnFrom:
                                break;
                            case InvoiceType.ReturnTo:
                                invoice.InvoiceNumber = string.Format("{0}{1}", "RTI", invoice.InvoiceIndex);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        invoice.ApproveDate = invoice.CreateDate = DateTime.Now;
                        db.Invoices.Add(invoice);
                    }
                    else
                    {
                        if (exInvoice.ApproveDate != null)
                        {
                            return null;
                        }
                        exInvoice.FromStockId = invoice.FromStockId;
                        exInvoice.ToStockId = invoice.ToStockId;
                        exInvoice.ApproveDate = invoice.ApproveDate = DateTime.Now;
                        exInvoice.ApproverId = invoice.ApproverId;
                        exInvoice.Approver = invoice.Approver;
                        exInvoice.AcceptDate = invoice.AcceptDate;
                        exInvoice.AccepterId = invoice.AccepterId;
                        exInvoice.PartnerId = invoice.PartnerId;
                        exInvoice.ProviderName = invoice.ProviderName;
                        exInvoice.ProviderJuridicalAddress = invoice.ProviderJuridicalAddress;
                        exInvoice.ProviderAddress = invoice.ProviderAddress;
                        exInvoice.ProviderBank = invoice.ProviderBank;
                        exInvoice.ProviderBankAccount = invoice.ProviderBankAccount;
                        exInvoice.ProviderTaxRegistration = invoice.ProviderTaxRegistration;
                        exInvoice.RecipientName = invoice.RecipientName;
                        exInvoice.RecipientJuridicalAddress = invoice.RecipientJuridicalAddress;
                        exInvoice.RecipientAddress = invoice.RecipientAddress;
                        exInvoice.RecipientBank = invoice.RecipientBank;
                        exInvoice.RecipientBankAccount = invoice.RecipientBankAccount;
                        exInvoice.RecipientTaxRegistration = invoice.RecipientTaxRegistration;
                        exInvoice.Discount = invoice.Discount;
                        exInvoice.Summ = invoice.Summ;
                        exInvoice.Notes = invoice.Notes;
                    }

                    #endregion

                    #region Add InvoiceItems and edit ProductItems quantity

                    var productsIds = invoiceItems.Select(t => t.ProductId).ToList();
                    List<ProductItems> productItemsFifo = GetProductItemsFifo(db, productsIds, fromStockIds).ToList();
                    decimal costPrice = 0;

                    var exInvoiceItems = db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).OrderBy(s => s.Index).ToList();
                    foreach (var exInvoiceItem in exInvoiceItems)
                    {
                        var exItem = invoiceItems.FirstOrDefault(s => s.Id == exInvoiceItem.Id);

                        if (exItem != null)
                        {
                            exInvoiceItem.Code = exItem.Code;
                            exInvoiceItem.CostPrice = exItem.CostPrice;
                            exInvoiceItem.Description = exItem.Description;
                            exInvoiceItem.Discount = exItem.Discount;
                            exInvoiceItem.ExpiryDate = exItem.ExpiryDate;
                            exInvoiceItem.Index = exItem.Index;
                            //todo check posibility if exItem.InvoiceId != invoice.Id
                            exInvoiceItem.InvoiceId = exItem.InvoiceId = invoice.Id;
                            exInvoiceItem.Mu = exItem.Mu;
                            exInvoiceItem.Note = exItem.Note;
                            exInvoiceItem.Price = exItem.Price;
                            exInvoiceItem.ProductId = exItem.ProductId;
                            exInvoiceItem.Quantity = exItem.Quantity;

                            var quantity = exInvoiceItem.Quantity ?? 0;
                            exInvoiceItem.Quantity = 0;
                            while (quantity > 0)
                            {
                                var productItemFifo = productItemsFifo.FirstOrDefault(s => s.StockId != null && fromStockIds.Contains((long)s.StockId) && s.ProductId == exInvoiceItem.ProductId && s.Quantity > 0);
                                if (productItemFifo == null)
                                {
                                    MessageManager.ShowMessage("Անբավարար միջոցներ " + exInvoiceItem.Code + " " + exInvoiceItem.Description + " " + exItem.Quantity, "Գործողության ընդհատոմ");
                                    return null;
                                }
                                var newInvoiceItem = GetExistingInvoiceItem(quantity, exInvoiceItem, productItemFifo);
                                if (!newInvoiceItem.Quantity.HasValue || newInvoiceItem.Quantity == 0) continue;
                                quantity -= (decimal)newInvoiceItem.Quantity;
                                if (exInvoiceItem.Quantity == 0)
                                {
                                    CopyInvoiceItem(newInvoiceItem, exInvoiceItem);
                                }
                                else
                                {
                                    db.InvoiceItems.Add(newInvoiceItem);
                                }
                                costPrice += (newInvoiceItem.CostPrice ?? 0) * (decimal)newInvoiceItem.Quantity;
                            }
                            invoiceItems.RemoveAt(invoiceItems.IndexOf(exItem));
                        }
                        else
                        {
                            db.InvoiceItems.Remove(exInvoiceItem);
                        }
                    }

                    foreach (var invoiceItem in invoiceItems.OrderBy(s => s.Index))
                    {
                        var quantity = invoiceItem.Quantity ?? 0;
                        while (quantity > 0)
                        {
                            var productItemFifo = productItemsFifo.FirstOrDefault(s => s.StockId != null && fromStockIds.Contains((long)s.StockId) && s.ProductId == invoiceItem.ProductId && s.Quantity > 0);
                            if (productItemFifo == null)
                            {
                                MessageManager.ShowMessage("Անբավարար միջոցներ " + invoiceItem.Code + " " + invoiceItem.Description + " " + invoiceItem.Quantity, "Գործողության ընդհատոմ", MessageBoxImage.Warning);
                                return null;
                            }

                            var newInvoiceItem = GetExistingInvoiceItem(quantity, invoiceItem, productItemFifo);
                            if (!newInvoiceItem.Quantity.HasValue || newInvoiceItem.Quantity == 0) continue;
                            quantity -= (decimal)newInvoiceItem.Quantity;

                            //todo check posibility if exItem.InvoiceId != invoice.Id   
                            invoiceItem.InvoiceId = invoice.Id;
                            db.InvoiceItems.Add(newInvoiceItem);
                            costPrice += (newInvoiceItem.CostPrice ?? 0) * (decimal)newInvoiceItem.Quantity;
                        }
                    }

                    // 711 - 216 Register cost price in AccountingRecoords

                    var pcAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = costPrice,
                        Debit = (long)AccountingPlanEnum.CostPrice,
                        Credit = (long)AccountingPlanEnum.Purchase,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                        DebitLongId = invoice.FromStockId,
                    };
                    db.AccountingRecords.Add(pcAccountingRecords);

                    #endregion

                    //Accounting records
                    var exPartner = db.Partners.SingleOrDefault(s => s.EsMemberId == invoice.MemberId && s.Id == invoicePaid.PartnerId);
                    if (exPartner == null)
                    {
                        MessageManager.OnMessage("Պատվիրատու ընտրված չէ։ Ընտրեք պատվիրատու և փորձեք կրկին։", MessageTypeEnum.Warning);
                        return null;
                    }
                    if (exPartner.Credit == null) exPartner.Credit = 0;
                    if (exPartner.Debit == null) exPartner.Debit = 0;

                    #region Accounting Receivable

                    // 221 - 611 Register in AccountingRecoords Accounting Receivable
                    var apAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = invoice.Summ,
                        Debit = (int)AccountingPlanEnum.AccountingReceivable,
                        DebitGuidId = exPartner.Id,
                        Credit = (int)AccountingPlanEnum.Proceeds,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                    };
                    db.AccountingRecords.Add(apAccountingRecords);
                    exPartner.Debit += invoice.Summ;

                    #endregion

                    #region Cash

                    if (invoicePaid.ByCash > 0)
                    {
                        var exCashDesk = invoicePaid != null && invoicePaid.CashDeskId != null ? db.CashDesk.SingleOrDefault(s => s.Id == invoicePaid.CashDeskId && s.MemberId == invoice.MemberId) : null;
                        if (exCashDesk == null)
                        {
                            MessageManager.OnMessage("Դրամարկղ ընտրված չէ։ Ընտրեք դրամարկղ և փորձեք կրկին։", MessageTypeEnum.Warning);
                            return null;
                        }

                        // 251 - 221 Register in AccountingRecoords
                        var cpAccountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.ByCash,
                            Debit = (int)AccountingPlanEnum.CashDesk,
                            DebitGuidId = exCashDesk.Id,
                            Credit = (int)AccountingPlanEnum.AccountingReceivable,
                            CreditGuidId = invoice.PartnerId,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(cpAccountingRecords);
                        exCashDesk.Total += invoicePaid.ByCash;
                        exPartner.Debit -= invoicePaid.ByCash;
                        //Change in CashDesk add to SaleInCash

                        var newSaleInCash = new SaleInCash
                        {
                            Id = Guid.NewGuid(),
                            CashDeskId = exCashDesk.Id,
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.ByCash,
                            Notes = invoice.Notes,
                            CashierId = (long)invoice.ApproverId,
                            AccountingRecordsId = cpAccountingRecords.Id
                        };
                        db.SaleInCash.Add(newSaleInCash);
                    }

                    #endregion

                    #region ByCheck

                    if ((invoicePaid.ByCheck ?? 0) > 0)
                    {
                        var exCashDeskByCheck = (invoicePaid.CashDeskForTicketId != null) ? db.CashDesk.SingleOrDefault(s => s.Id == invoicePaid.CashDeskForTicketId && s.MemberId == invoice.MemberId) : null;
                        if (exCashDeskByCheck == null)
                        {
                            MessageManager.OnMessage("Անկանխիկ դրամարկղ ընտրված չէ։", MessageTypeEnum.Warning);
                            return null;
                        }
                        // 251 - 221 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = (invoicePaid.ByCheck ?? 0),
                            Debit = (int)AccountingPlanEnum.CashDesk,
                            DebitGuidId = (Guid)exCashDeskByCheck.Id,
                            Credit = (int)AccountingPlanEnum.AccountingReceivable,
                            CreditGuidId = invoice.PartnerId,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exCashDeskByCheck.Total += (invoicePaid.ByCheck ?? 0);
                        exPartner.Debit -= (invoicePaid.ByCheck ?? 0);
                        //Change in CashDesk add to SaleInCash
                        var newSaleInCashByCheck = new SaleInCash
                        {
                            Id = Guid.NewGuid(),
                            CashDeskId = exCashDeskByCheck.Id,
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = (invoicePaid.ByCheck ?? 0),
                            Notes = invoice.Notes,
                            CashierId = (long)invoice.ApproverId
                        };
                        db.SaleInCash.Add(newSaleInCashByCheck);
                    }

                    #endregion

                    #region Add ReceivedPrepayment

                    if ((invoicePaid.ReceivedPrepayment ?? 0) > 0)
                    {
                        // 221 - 523 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.ReceivedPrepayment ?? 0,
                            Debit = (int)AccountingPlanEnum.AccountingReceivable,
                            Credit = (int)AccountingPlanEnum.ReceivedInAdvance,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exPartner.Debit -= invoicePaid.ReceivedPrepayment ?? 0;
                        exPartner.Credit -= invoicePaid.ReceivedPrepayment ?? 0;
                    }

                    #endregion

                    #region Add AccountsReceivable

                    if ((invoicePaid.AccountsReceivable ?? 0) > 0)
                    {
                        //Change in CashDesk add to SaleInCash
                        var newAccountsReceivable = new AccountsReceivable
                        {
                            Id = Guid.NewGuid(),
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.AccountsReceivable ?? 0,
                            Notes = invoice.Notes,
                            CashierId = (long)invoice.ApproverId,
                            MemberId = invoice.MemberId,
                            PartnerId = exPartner.Id,
                            ExpairyDate = ((DateTime)invoice.ApproveDate).AddMonths(1),
                            AccountingRecordsId = apAccountingRecords.Id
                        };
                        db.AccountsReceivable.Add(newAccountsReceivable);
                        exPartner.Debit += (invoicePaid.AccountsReceivable ?? 0);
                        if (exPartner.Debit > exPartner.MaxDebit)
                        {
                            MessageManager.ShowMessage("Դեպիտորական պարտքը սահմանվածից ավել է։", "Անբավարար միջոցներ");
                            return null;
                        }
                    }

                    #endregion

                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return invoice;
                    }
                    catch (Exception ex)
                    {
                        MessageManager.OnMessage(string.Format("Exception on approving sale invoice: {0}", ex.Message));
                        invoice.ApproveDate = null;
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Approve sale invoice
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="invoiceItems"></param>
        /// <param name="fromStockIds"></param>
        /// <param name="invoicePaid"></param>
        /// <returns></returns>
        private static Invoices TryApproveSaleInvoice(Invoices invoice, List<InvoiceItems> invoiceItems, List<long> fromStockIds, InvoicePaid invoicePaid)
        {
            if (!invoiceItems.Any() || !fromStockIds.Any() || invoicePaid == null) return null;
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 5, 0)))
            {
                using (var db = GetDataContext())
                {
                    #region Edit Invoices

                    var exInvoice = db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);
                    if (exInvoice == null)
                    {
                        invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, db);
                        switch (invoice.InvoiceTypeId)
                        {
                            case (long)InvoiceType.SaleInvoice:
                                invoice.InvoiceNumber = string.Format("{0}{1}", "SI", invoice.InvoiceIndex);
                                break;
                            case (long)InvoiceType.PurchaseInvoice:
                                return null;
                                break;
                        }
                        invoice.ApproveDate = invoice.CreateDate = DateTime.Now;
                        db.Invoices.Add(invoice);
                    }
                    else
                    {
                        if (exInvoice.ApproveDate != null)
                        {
                            return null;
                        }
                        exInvoice.FromStockId = invoice.FromStockId;
                        exInvoice.ToStockId = invoice.ToStockId;
                        exInvoice.ApproveDate = invoice.ApproveDate = DateTime.Now;
                        exInvoice.ApproverId = invoice.ApproverId;
                        exInvoice.Approver = invoice.Approver;
                        exInvoice.AcceptDate = invoice.AcceptDate;
                        exInvoice.AccepterId = invoice.AccepterId;
                        exInvoice.PartnerId = invoice.PartnerId;
                        exInvoice.ProviderName = invoice.ProviderName;
                        exInvoice.ProviderJuridicalAddress = invoice.ProviderJuridicalAddress;
                        exInvoice.ProviderAddress = invoice.ProviderAddress;
                        exInvoice.ProviderBank = invoice.ProviderBank;
                        exInvoice.ProviderBankAccount = invoice.ProviderBankAccount;
                        exInvoice.ProviderTaxRegistration = invoice.ProviderTaxRegistration;
                        exInvoice.RecipientName = invoice.RecipientName;
                        exInvoice.RecipientJuridicalAddress = invoice.RecipientJuridicalAddress;
                        exInvoice.RecipientAddress = invoice.RecipientAddress;
                        exInvoice.RecipientBank = invoice.RecipientBank;
                        exInvoice.RecipientBankAccount = invoice.RecipientBankAccount;
                        exInvoice.RecipientTaxRegistration = invoice.RecipientTaxRegistration;
                        exInvoice.Discount = invoice.Discount;
                        exInvoice.Summ = invoice.Summ;
                        exInvoice.Notes = invoice.Notes;
                    }

                    #endregion

                    #region Add InvoiceItems and edit ProductItems quantity

                    var productItemsFifo = GetProductItemsFifo(db, invoiceItems.Select(t => t.ProductId).ToList(), fromStockIds);
                    decimal costPrice = 0;

                    var exInvoiceItems = db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).OrderBy(s => s.Index).ToList();
                    foreach (var exInvoiceItem in exInvoiceItems)
                    {
                        var exItem = invoiceItems.FirstOrDefault(s => s.Id == exInvoiceItem.Id);

                        if (exItem != null)
                        {
                            exInvoiceItem.Code = exItem.Code;
                            exInvoiceItem.CostPrice = exItem.CostPrice;
                            exInvoiceItem.Description = exItem.Description;
                            exInvoiceItem.Discount = exItem.Discount;
                            exInvoiceItem.ExpiryDate = exItem.ExpiryDate;
                            exInvoiceItem.Index = exItem.Index;
                            //todo check posibility if exItem.InvoiceId != invoice.Id
                            exInvoiceItem.InvoiceId = exItem.InvoiceId = invoice.Id;
                            exInvoiceItem.Mu = exItem.Mu;
                            exInvoiceItem.Note = exItem.Note;
                            exInvoiceItem.Price = exItem.Price;
                            exInvoiceItem.ProductId = exItem.ProductId;
                            exInvoiceItem.Quantity = exItem.Quantity;

                            var quantity = exInvoiceItem.Quantity ?? 0;
                            exInvoiceItem.Quantity = 0;
                            while (quantity > 0)
                            {
                                var productItemFifo = productItemsFifo.FirstOrDefault(s => s.StockId != null && fromStockIds.Contains((long)s.StockId) && s.ProductId == exInvoiceItem.ProductId && s.Quantity > 0);
                                if (productItemFifo == null)
                                {
                                    MessageManager.ShowMessage("Անբավարար միջոցներ " + exInvoiceItem.Code + " " + exInvoiceItem.Description + " " + exItem.Quantity, "Գործողության ընդհատոմ");
                                    return null;
                                }
                                var newInvoiceItem = GetExistingInvoiceItem(quantity, exInvoiceItem, productItemFifo);
                                if (!newInvoiceItem.Quantity.HasValue || newInvoiceItem.Quantity == 0) continue;
                                quantity -= (decimal)newInvoiceItem.Quantity;
                                if (exInvoiceItem.Quantity == 0)
                                {
                                    CopyInvoiceItem(newInvoiceItem, exInvoiceItem);
                                }
                                else
                                {
                                    db.InvoiceItems.Add(newInvoiceItem);
                                }
                                costPrice += (newInvoiceItem.CostPrice ?? 0) * (decimal)newInvoiceItem.Quantity;
                            }
                            invoiceItems.RemoveAt(invoiceItems.IndexOf(exItem));
                        }
                        else
                        {
                            db.InvoiceItems.Remove(exInvoiceItem);
                        }
                    }

                    foreach (var invoiceItem in invoiceItems.OrderBy(s => s.Index))
                    {
                        var quantity = invoiceItem.Quantity ?? 0;
                        while (quantity > 0)
                        {
                            var productItemFifo = productItemsFifo.FirstOrDefault(s => s.StockId != null && fromStockIds.Contains((long)s.StockId) && s.ProductId == invoiceItem.ProductId && s.Quantity > 0);
                            if (productItemFifo == null)
                            {
                                MessageManager.ShowMessage("Անբավարար միջոցներ " + invoiceItem.Code + " " + invoiceItem.Description + " " + invoiceItem.Quantity, "Գործողության ընդհատոմ", MessageBoxImage.Warning);
                                return null;
                            }

                            var newInvoiceItem = GetExistingInvoiceItem(quantity, invoiceItem, productItemFifo);
                            if (!newInvoiceItem.Quantity.HasValue || newInvoiceItem.Quantity == 0) continue;
                            quantity -= (decimal)newInvoiceItem.Quantity;

                            //todo check posibility if exItem.InvoiceId != invoice.Id   
                            invoiceItem.InvoiceId = invoice.Id;
                            db.InvoiceItems.Add(newInvoiceItem);
                            costPrice += (newInvoiceItem.CostPrice ?? 0) * (decimal)newInvoiceItem.Quantity;
                        }
                    }

                    #endregion

                    #region Cost of Purchase 711 - 216

                    // 711 - 216 Register cost price in AccountingRecoords

                    var pcAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = costPrice,
                        Debit = (long)AccountingPlanEnum.CostPrice,
                        Credit = (long)AccountingPlanEnum.Purchase,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                        DebitLongId = invoice.FromStockId,
                    };
                    db.AccountingRecords.Add(pcAccountingRecords);

                    #endregion

                    //Accounting records
                    var exPartner = db.Partners.SingleOrDefault(s => s.EsMemberId == invoice.MemberId && s.Id == invoicePaid.PartnerId);
                    if (exPartner == null)
                    {
                        MessageManager.OnMessage("Պատվիրատու ընտրված չէ։ Ընտրեք պատվիրատու և փորձեք կրկին։", MessageTypeEnum.Warning);
                        return null;
                    }
                    //Remove this
                    if (exPartner.Credit == null) exPartner.Credit = 0;
                    if (exPartner.Debit == null) exPartner.Debit = 0;

                    #region Accounting Receivable

                    // 221 - 611 Register in AccountingRecoords Accounting Receivable
                    var apAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = invoice.Summ,
                        Debit = (int)AccountingPlanEnum.AccountingReceivable,
                        DebitGuidId = exPartner.Id,
                        Credit = (int)AccountingPlanEnum.Proceeds,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                    };
                    db.AccountingRecords.Add(apAccountingRecords);
                    exPartner.Debit += invoice.Summ;

                    #endregion

                    #region Cash

                    var exCashDesk = invoicePaid != null && invoicePaid.CashDeskId != null ? db.CashDesk.SingleOrDefault(s => s.Id == invoicePaid.CashDeskId && s.MemberId == invoice.MemberId) : null;

                    if (invoicePaid.ByCash > 0)
                    {
                        if (exCashDesk == null)
                        {
                            MessageManager.OnMessage("Դրամարկղ ընտրված չէ։ Ընտրեք դրամարկղ և փորձեք կրկին։", MessageTypeEnum.Warning);
                            return null;
                        }
                        // 251 - 221 Register in AccountingRecoords
                        var cpAccountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.ByCash,
                            Debit = (int)AccountingPlanEnum.CashDesk,
                            DebitGuidId = exCashDesk.Id,
                            Credit = (int)AccountingPlanEnum.AccountingReceivable,
                            CreditGuidId = invoice.PartnerId,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(cpAccountingRecords);
                        exCashDesk.Total += invoicePaid.ByCash;
                        exPartner.Debit -= invoicePaid.ByCash;
                        //Change in CashDesk add to SaleInCash

                        var newSaleInCash = new SaleInCash
                        {
                            Id = Guid.NewGuid(),
                            CashDeskId = exCashDesk.Id,
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.ByCash,
                            Notes = invoice.Notes,
                            CashierId = (long)invoice.ApproverId,
                            AccountingRecordsId = cpAccountingRecords.Id
                        };
                        db.SaleInCash.Add(newSaleInCash);
                    }

                    #endregion

                    #region ByCheck

                    if ((invoicePaid.ByCheck ?? 0) > 0)
                    {
                        var exCashDeskByCheck = (invoicePaid.CashDeskForTicketId != null) ? db.CashDesk.SingleOrDefault(s => s.Id == invoicePaid.CashDeskForTicketId && s.MemberId == invoice.MemberId) : null;
                        if (exCashDeskByCheck == null)
                        {
                            MessageManager.OnMessage("Անկանխիկ դրամարկղ ընտրված չէ։", MessageTypeEnum.Warning);
                            return null;
                        }
                        // 251 - 221 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = (invoicePaid.ByCheck ?? 0),
                            Debit = (int)AccountingPlanEnum.CashDesk,
                            DebitGuidId = (Guid)exCashDeskByCheck.Id,
                            Credit = (int)AccountingPlanEnum.AccountingReceivable,
                            CreditGuidId = invoice.PartnerId,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exCashDeskByCheck.Total += (invoicePaid.ByCheck ?? 0);
                        exPartner.Debit -= (invoicePaid.ByCheck ?? 0);
                        //Change in CashDesk add to SaleInCash
                        var newSaleInCashByCheck = new SaleInCash
                        {
                            Id = Guid.NewGuid(),
                            CashDeskId = exCashDeskByCheck.Id,
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = (invoicePaid.ByCheck ?? 0),
                            Notes = invoice.Notes,
                            CashierId = (long)invoice.ApproverId
                        };
                        db.SaleInCash.Add(newSaleInCashByCheck);
                    }

                    #endregion

                    #region Add ReceivedPrepayment 221 - 523

                    if ((invoicePaid.ReceivedPrepayment ?? 0) > 0)
                    {
                        // 221 - 523 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.ReceivedPrepayment ?? 0,
                            Debit = (int)AccountingPlanEnum.AccountingReceivable,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingPlanEnum.ReceivedInAdvance,
                            CreditGuidId = exPartner.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exPartner.Debit -= invoicePaid.ReceivedPrepayment ?? 0;
                        exPartner.Credit -= invoicePaid.ReceivedPrepayment ?? 0;
                    }

                    #endregion

                    #region Add AccountsReceivable

                    if ((invoicePaid.AccountsReceivable ?? 0) > 0)
                    {
                        //Change in CashDesk add to SaleInCash
                        var newAccountsReceivable = new AccountsReceivable()
                        {
                            Id = Guid.NewGuid(),
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.AccountsReceivable ?? 0,
                            Notes = invoice.Notes,
                            CashierId = (long)invoice.ApproverId,
                            MemberId = invoice.MemberId,
                            PartnerId = exPartner.Id,
                            ExpairyDate = ((DateTime)invoice.ApproveDate).AddMonths(1),
                            AccountingRecordsId = apAccountingRecords.Id
                        };
                        db.AccountsReceivable.Add(newAccountsReceivable);
                        if (exPartner.Debit > exPartner.MaxDebit)
                        {
                            MessageManager.ShowMessage("Դեբիտորական պարտքը սահմանվածից ավել է։", "Անբավարար միջոցներ");
                            return null;
                        }
                    }

                    #endregion

                    #region Prepayment 521 - 224

                    if ((invoicePaid.Prepayment ?? 0) > 0)
                    {
                        if (exCashDesk == null)
                        {
                            MessageManager.OnMessage("Դրամարկղ ընտրված չէ։ Ընտրեք դրամարկղ և փորձեք կրկին։", MessageTypeEnum.Warning);
                            return null;
                        }
                        // 521 - 224 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.Prepayment ?? 0,
                            Debit = (int)AccountingPlanEnum.PurchasePayables,
                            DebitGuidId = exCashDesk.Id,
                            Credit = (int)AccountingPlanEnum.Prepayments,
                            CreditGuidId = exPartner.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exPartner.Credit += invoicePaid.Prepayment ?? 0;
                        exCashDesk.Total += invoicePaid.Prepayment ?? 0;
                    }

                    #endregion

                    #region Set Discount Bond

                    // 712 - 224 523 Register in AccountingRecoords
                    if (invoicePaid.DiscountBond > 0)
                    {
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = invoicePaid.DiscountBond,
                            Debit = (int)AccountingPlanEnum.ReceivedInAdvance,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingPlanEnum.CostOfSales,
                            //CreditGuidId = exPartner.Id,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exPartner.Debit += invoicePaid.DiscountBond;
                        //exCashDesk.Total += invoicePaid.Prepayment ?? 0;
                        return null;
                    }

                    #endregion Set Discount Bond

                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return invoice;
                    }
                    catch (Exception ex)
                    {
                        MessageManager.OnMessage(string.Format("Exception on approving sale invoice: {0}", ex));
                        invoice.ApproveDate = null;
                        return null;
                    }
                }
            }
        }

        private static IEnumerable<ProductItems> GetProductItemsFifo(EsStockDBEntities db, List<Guid> productsIds, IEnumerable<long> stockIds)
        {
            try
            {
                List<ProductItems> productItemsFifo = db.ProductItems.Where(s => s.MemberId == MemberId && s.StockId != null && stockIds.Contains(s.StockId.Value) && productsIds.Contains(s.ProductId) && s.Quantity > 0).ToList();
                var productItems = productItemsFifo.Join(db.Invoices.Where(s => s.MemberId == MemberId && s.ToStockId != null && stockIds.Contains(s.ToStockId.Value)), pi => pi.DeliveryInvoiceId, i => i.Id, (pi, i) => new { pi, i }).ToList();
                return productItems.OrderBy(t => t.i.ApproveDate).Select(t => t.pi);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void CheckForQuantityExisting(List<InvoiceItemsModel> invoiceItems, IEnumerable<long> fromStockIds)
        {
            int mismatchItemsCount = 0;
            using (var db = GetDataContext())
            {
                var productsIds = invoiceItems.Select(t => t.ProductId).ToList();
                List<ProductItems> productItemsFifo = db.ProductItems.Where(s => productsIds.Contains(s.ProductId) && fromStockIds.Contains(s.StockId ?? 0) && s.Quantity > 0).ToList();

                foreach (var items in invoiceItems.GroupBy(s => s.ProductId))
                {
                    var invoiceItem = items.First();
                    invoiceItem.Quantity = items.Sum(s => s.Quantity);
                    var existingQuantity = productItemsFifo.Where(s => s.ProductId == invoiceItem.ProductId).Sum(s => s.Quantity);
                    if (existingQuantity < invoiceItem.Quantity)
                    {
                        mismatchItemsCount++;
                        var message = string.Format("Անբավարար միջոցներ {0} ({1}) {2} առկա է {3}", invoiceItem.Description, invoiceItem.Code, invoiceItem.Quantity, existingQuantity);
                        MessageManager.OnMessage(message);
                        //if (MessageBox.Show(string.Format("{0} \nՑանկանու՞մ եք շարունակել:", message), "Անբավարար միջոցներ", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                    }
                }
                MessageManager.OnMessage(mismatchItemsCount == 0 ? "Բոլոր ապրանքների քանակությունն առկա է:" : string.Format("Բավարար քանակով առկա չէ թվով {0} ապրանքատեսակներ:", mismatchItemsCount));
            }
        }

        private static Invoices TryApproveInventoryWriteOffInvoice(Invoices invoice, List<InvoiceItems> invoiceItems, IEnumerable<long> fromStockIds)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 15, 0)))
            {
                using (var db = GetDataContext())
                {
                    invoice.ApproveDate = DateTime.Now;
                    try
                    {
                        #region Remove InvoiceItems

                        var exInvoiceItems = db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).ToList();
                        db.InvoiceItems.RemoveRange(exInvoiceItems);
                        //foreach (var exInvoiceItem in exInvoiceItems)
                        //{
                        //    if(invoiceItems.All(s => s.Id != exInvoiceItem.Id)) 
                        //}

                        db.SaveChanges();
                        #endregion

                        #region Edit Invoices

                        var exInvoice = db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);
                        if (exInvoice == null)
                        {
                            invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, db);
                            switch (invoice.InvoiceTypeId)
                            {
                                case (long)InvoiceType.InventoryWriteOff:
                                    invoice.InvoiceNumber = string.Format("{0}{1}", "WO", invoice.InvoiceIndex);
                                    break;
                                case (long)InvoiceType.PurchaseInvoice:
                                    return null;
                                    break;
                            }
                            invoice.ApproveDate = invoice.CreateDate = DateTime.Now;
                            db.Invoices.Add(invoice);
                        }
                        else
                        {
                            if (exInvoice.ApproveDate != null)
                            {
                                return null;
                            }
                            exInvoice.FromStockId = invoice.FromStockId;
                            exInvoice.ToStockId = invoice.ToStockId;
                            exInvoice.ApproveDate = DateTime.Now;
                            exInvoice.ApproverId = invoice.ApproverId;
                            exInvoice.Approver = invoice.Approver;
                            exInvoice.AcceptDate = invoice.AcceptDate;
                            exInvoice.AccepterId = invoice.AccepterId;
                            exInvoice.PartnerId = invoice.PartnerId;
                            exInvoice.ProviderName = invoice.ProviderName;
                            exInvoice.ProviderJuridicalAddress = invoice.ProviderJuridicalAddress;
                            exInvoice.ProviderAddress = invoice.ProviderAddress;
                            exInvoice.ProviderBank = invoice.ProviderBank;
                            exInvoice.ProviderBankAccount = invoice.ProviderBankAccount;
                            exInvoice.ProviderTaxRegistration = invoice.ProviderTaxRegistration;
                            exInvoice.RecipientName = invoice.RecipientName;
                            exInvoice.RecipientJuridicalAddress = invoice.RecipientJuridicalAddress;
                            exInvoice.RecipientAddress = invoice.RecipientAddress;
                            exInvoice.RecipientBank = invoice.RecipientBank;
                            exInvoice.RecipientBankAccount = invoice.RecipientBankAccount;
                            exInvoice.RecipientTaxRegistration = invoice.RecipientTaxRegistration;
                            exInvoice.Discount = invoice.Discount;
                            exInvoice.Summ = invoice.Summ;
                            exInvoice.Notes = invoice.Notes;
                        }
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            MessageManager.OnMessage(ex.Message, MessageTypeEnum.Warning);
                            return null;
                        }

                        #endregion

                        #region Add InvoiceItems and edit ProductItems quantity

                        var productsIds = invoiceItems.Select(t => t.ProductId).ToList();
                        List<ProductItems> productItemsFifo = GetProductItemsFifo(db, productsIds, fromStockIds).ToList();
                        decimal costPrice = 0;

                        try
                        {
                            foreach (var invoiceItem in invoiceItems)
                            {
                                var quantity = invoiceItem.Quantity ?? 0;
                                while (quantity > 0)
                                {
                                    var productItemFifo = productItemsFifo.FirstOrDefault(s => s.StockId != null && fromStockIds.Contains((long)s.StockId) && s.ProductId == invoiceItem.ProductId && s.Quantity > 0);
                                    if (productItemFifo == null)
                                    {
                                        MessageManager.ShowMessage("Անբավարար միջոցներ " + invoiceItem.Code + " " + invoiceItem.Description + " " + invoiceItem.Quantity, "Գործողության ընդհատոմ", MessageBoxImage.Warning);
                                        return null;
                                    }
                                    var curQuantity = productItemFifo.Quantity > quantity ? quantity : productItemFifo.Quantity;
                                    var newInvoiceItem = new InvoiceItems
                                    {
                                        Id = Guid.NewGuid(),
                                        InvoiceId = invoiceItem.InvoiceId,
                                        ProductId = invoiceItem.ProductId,
                                        ProductItemId = productItemFifo.Id,
                                        Index = invoiceItem.Index,
                                        Code = invoiceItem.Code,
                                        Description = invoiceItem.Description,
                                        Mu = invoiceItem.Mu,
                                        Quantity = curQuantity,
                                        Price = invoiceItem.Price,
                                        CostPrice = productItemFifo.CostPrice,
                                        Discount = invoiceItem.Discount,
                                        Note = invoiceItem.Note
                                    };
                                    productItemFifo.Quantity -= curQuantity;
                                    quantity -= curQuantity;
                                    db.InvoiceItems.Add(newInvoiceItem);
                                    costPrice += (newInvoiceItem.CostPrice ?? 0) * curQuantity;
                                    db.SaveChanges();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageManager.OnMessage(ex.Message, MessageTypeEnum.Warning);
                            return null;
                        }
                        // 714 - 216 Register cost price in AccountingRecoords
                        var pcAccountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = costPrice,
                            Debit = (long)AccountingPlanEnum.OtherOperationalExpenses,
                            Credit = (long)AccountingPlanEnum.Purchase,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                            DebitLongId = invoice.FromStockId,
                        };
                        db.AccountingRecords.Add(pcAccountingRecords);

                        #endregion

                        db.SaveChanges();
                        transaction.Complete();
                        return invoice;
                    }
                    catch (Exception ex)
                    {
                        MessageManager.OnMessage(ex.Message, MessageTypeEnum.Warning);
                        return null;
                    }
                }
            }
        }

        private static Invoices TryApproveMoveingInvoice(Invoices invoice, List<InvoiceItems> invoiceItems)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 5, 0)))
            {
                using (var db = GetDataContext())
                {
                    invoice.ApproveDate = DateTime.Now;

                    #region Remove InvoiceItems

                    var exInvoiceItems = db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).ToList();
                    foreach (var exInvoiceItem in exInvoiceItems)
                    {
                        db.InvoiceItems.Remove(exInvoiceItem);
                    }

                    #endregion

                    #region Edit Invoices

                    var exInvoice = db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);
                    if (exInvoice == null)
                    {
                        invoice.InvoiceNumber = GetInvoiceNumber(invoice.InvoiceTypeId, db);
                        invoice.ApproveDate = invoice.CreateDate = DateTime.Now;
                        db.Invoices.Add(invoice);
                    }
                    else
                    {
                        if (exInvoice.ApproveDate != null)
                        {
                            return null;
                        }
                        exInvoice.FromStockId = invoice.FromStockId;
                        exInvoice.ToStockId = invoice.ToStockId;
                        exInvoice.ApproveDate = DateTime.Now;
                        exInvoice.ApproverId = invoice.ApproverId;
                        exInvoice.Approver = invoice.Approver;
                        exInvoice.AcceptDate = invoice.AcceptDate;
                        exInvoice.AccepterId = invoice.AccepterId;
                        exInvoice.PartnerId = invoice.PartnerId;
                        exInvoice.ProviderName = invoice.ProviderName;
                        exInvoice.ProviderJuridicalAddress = invoice.ProviderJuridicalAddress;
                        exInvoice.ProviderAddress = invoice.ProviderAddress;
                        exInvoice.ProviderBank = invoice.ProviderBank;
                        exInvoice.ProviderBankAccount = invoice.ProviderBankAccount;
                        exInvoice.ProviderTaxRegistration = invoice.ProviderTaxRegistration;
                        exInvoice.RecipientName = invoice.RecipientName;
                        exInvoice.RecipientJuridicalAddress = invoice.RecipientJuridicalAddress;
                        exInvoice.RecipientAddress = invoice.RecipientAddress;
                        exInvoice.RecipientBank = invoice.RecipientBank;
                        exInvoice.RecipientBankAccount = invoice.RecipientBankAccount;
                        exInvoice.RecipientTaxRegistration = invoice.RecipientTaxRegistration;
                        exInvoice.Discount = invoice.Discount;
                        exInvoice.Summ = invoice.Summ;
                        exInvoice.Notes = invoice.Notes;
                    }

                    #endregion

                    #region Add InvoiceItems and edit ProductItems quantity

                    var productsIds = invoiceItems.Select(t => t.ProductId).ToList();
                    List<ProductItems> productItemsFifo = GetProductItemsFifo(db, productsIds, new List<long> { invoice.FromStockId ?? 0 }).ToList();
                    decimal costPrice = 0;
                    foreach (var invoiceItem in invoiceItems)
                    {
                        var quantity = invoiceItem.Quantity ?? 0;
                        while (quantity > 0)
                        {
                            var productItemFifo = productItemsFifo.FirstOrDefault(s => s.ProductId == invoiceItem.ProductId && s.Quantity > 0);
                            if (productItemFifo == null)
                            {
                                MessageManager.ShowMessage("Անբավարար միջոցներ " + invoiceItem.Code + " " + invoiceItem.Description + " " + invoiceItem.Quantity, "Գործողության ընդհատոմ", MessageBoxImage.Warning);
                                return null;
                            }
                            var curQuantity = productItemFifo.Quantity > quantity ? quantity : productItemFifo.Quantity;
                            var newInvoiceItem = new InvoiceItems
                            {
                                Id = Guid.NewGuid(),
                                InvoiceId = invoiceItem.InvoiceId,
                                ProductId = invoiceItem.ProductId,
                                ProductItemId = productItemFifo.Id,
                                Index = invoiceItem.Index,
                                Code = invoiceItem.Code,
                                Description = invoiceItem.Description,
                                Mu = invoiceItem.Mu,
                                Quantity = curQuantity,
                                Price = invoiceItem.Price,
                                CostPrice = productItemFifo.CostPrice,
                                Discount = invoiceItem.Discount,
                                Note = invoiceItem.Note
                            };
                            productItemFifo.Quantity -= curQuantity;
                            var newProductItem = new ProductItems
                            {
                                Id = Guid.NewGuid(),
                                ProductId = productItemFifo.ProductId,
                                Description = productItemFifo.Description,
                                DeliveryInvoiceId = invoice.Id,
                                CostPrice = productItemFifo.CostPrice,
                                Quantity = curQuantity,
                                StockId = invoice.ToStockId,
                                MemberId = productItemFifo.MemberId
                            };
                            quantity -= curQuantity;
                            db.InvoiceItems.Add(newInvoiceItem);
                            costPrice += (newInvoiceItem.Quantity ?? 0) * (newInvoiceItem.CostPrice ?? 0);
                            db.ProductItems.Add(newProductItem);
                        }
                    }

                    #endregion

                    #region Cash

                    // 251 - 221 Register in AccountingRecoords
                    var cpAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = costPrice,
                        Debit = (int)AccountingPlanEnum.Purchase,
                        DebitLongId = invoice.ToStockId,
                        Credit = (int)AccountingPlanEnum.Purchase,
                        CreditLongId = invoice.FromStockId,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                    };
                    db.AccountingRecords.Add(cpAccountingRecords);

                    #endregion

                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return invoice;
                    }
                    catch (Exception ex)
                    {
                        MessageManager.OnMessage(ex.ToString());
                        invoice.ApproveDate = null;
                        return null;
                    }
                }
            }
        }

        private static ProductItems CreateProductItem(InvoiceItems invoiceItem, long stockId)
        {
            return new ProductItems
            {
                Id = Guid.NewGuid(),
                ProductId = invoiceItem.ProductId,
                DeliveryInvoiceId = invoiceItem.InvoiceId,
                StockId = stockId,
                Quantity = invoiceItem.Quantity ?? 0,
                CostPrice = invoiceItem.Price ?? 0,
                MemberId = ApplicationManager.Member.Id
            };
        }

        private static InvoiceItems Doublicate(InvoiceItems invoiceItem)
        {
            return new InvoiceItems
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoiceItem.InvoiceId,
                ProductId = invoiceItem.ProductId,
                ProductItemId = invoiceItem.ProductItemId,
                Index = invoiceItem.Index,
                Code = invoiceItem.Code,
                Description = invoiceItem.Description,
                Mu = invoiceItem.Mu,
                Price = invoiceItem.Price,
                CostPrice = invoiceItem.CostPrice,
                Discount = invoiceItem.Discount,
                Note = invoiceItem.Note
            };
        }

        private static InvoiceItems GetExistingInvoiceItem(decimal quantity, InvoiceItems invoiceItem, ProductItems productItemFifo)
        {
            var curQuantity = productItemFifo.Quantity > quantity ? quantity : productItemFifo.Quantity;
            var newInvoiceItem = Doublicate(invoiceItem);
            newInvoiceItem.ProductItemId = productItemFifo.Id;
            newInvoiceItem.Quantity = curQuantity;
            newInvoiceItem.CostPrice = productItemFifo.CostPrice;
            productItemFifo.Quantity -= curQuantity;
            return newInvoiceItem;
        }

        private static void CopyInvoiceItem(InvoiceItems sourceInvoiceItems, InvoiceItems destinationInvoiceItems)
        {
            if (destinationInvoiceItems == null) destinationInvoiceItems = new InvoiceItems();
            if (sourceInvoiceItems != null)
            {
                destinationInvoiceItems.Code = sourceInvoiceItems.Code;
                destinationInvoiceItems.CostPrice = sourceInvoiceItems.CostPrice;
                destinationInvoiceItems.Description = sourceInvoiceItems.Description;
                destinationInvoiceItems.Discount = sourceInvoiceItems.Discount;
                destinationInvoiceItems.ExpiryDate = sourceInvoiceItems.ExpiryDate;
                destinationInvoiceItems.Index = sourceInvoiceItems.Index;
                destinationInvoiceItems.InvoiceId = sourceInvoiceItems.InvoiceId;
                destinationInvoiceItems.Mu = sourceInvoiceItems.Mu;
                destinationInvoiceItems.Note = sourceInvoiceItems.Note;
                destinationInvoiceItems.Price = sourceInvoiceItems.Price;
                destinationInvoiceItems.ProductId = sourceInvoiceItems.ProductId;
                destinationInvoiceItems.ProductItemId = sourceInvoiceItems.ProductItemId;
                destinationInvoiceItems.Quantity = sourceInvoiceItems.Quantity;
                //todo Add missing properties
            }
            else
            {
                destinationInvoiceItems = null;
            }
        }

        private static List<InvoiceItems> TryGetInvoiceItemssByCode(IEnumerable<string> codes, DateTime fromDate, DateTime toDate, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    //var invoiceIds = db.Invoices.Where(s => s.MemberId == memberId && s.CreateDate >= fromDate && s.CreateDate < toDate && s.ApproveDate != null && s.InvoiceTypeId == (long)InvoiceType.PurchaseInvoice).Select(s => s.Id);
                    return db.InvoiceItems.Where(s => s.Invoices.MemberId == ApplicationManager.Member.Id && s.Invoices.ApproveDate >= fromDate && s.Invoices.ApproveDate <= toDate && s.Invoices.InvoiceTypeId == (long)InvoiceType.PurchaseInvoice).Include(s => s.Invoices).Include(s => s.Products).Include(s => s.Products.ProductCategories).Include(s => s.Products.ProductGroup).Include(s => s.ProductItems).Include(s => s.Products.ProductsAdditionalData).Where(s => codes.Contains(s.Code)).OrderBy(s => s.Code).ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }

        private static List<InvoiceItems> TryGetProductHistory(List<Guid> productIds, DateTime fromDate, DateTime toDate, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var invoiceIds = db.Invoices.Where(s => s.MemberId == memberId && s.CreateDate >= fromDate && s.CreateDate < toDate && s.ApproveDate != null).Select(s => s.Id);
                    return db.InvoiceItems.Include(s => s.Invoices).Include(s => s.Products).Include(s => s.Products.ProductCategories).Include(s => s.Products.ProductGroup).Include(s => s.ProductItems).Include(s => s.Products.ProductsAdditionalData).Where(s => invoiceIds.Contains(s.InvoiceId) && productIds.Contains(s.ProductId)).OrderBy(s => s.Invoices.ApproveDate).ThenBy(s => s.Code).ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }

        #region Invoices

        private static List<InvoiceModel> TryGetInvoicesDescriptions(InvoiceTypeEnum invoiceType, int? maxCount)
        {
            try
            {
                var types = GetInvoiceTypes(invoiceType);
                using (var db = GetDataContext())
                {
                    var items = db.Invoices.Where(s => types.Contains((int)s.InvoiceTypeId) && s.ApproveDate != null && s.MemberId == ApplicationManager.Instance.GetMember.Id).OrderByDescending(s => s.ApproveDate).Select(i => new InvoiceModel
                    {
                        Id = i.Id,
                        CreateDate = i.CreateDate,
                        ApproveDate = i.ApproveDate,
                        Creator = i.Creator,
                        Approver = i.Approver,
                        InvoiceNumber = i.InvoiceNumber,
                        InvoiceTypeId = i.InvoiceTypeId,
                        ProviderName = i.ProviderName,
                        RecipientName = i.RecipientName,
                        Total = i.Summ
                    });
                    return maxCount != null ? items.Take((int)maxCount).ToList() : items.ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceModel>();
            }
        }

        private static List<InvoiceModel> TryGetInvoicesDescriptions(InvoiceTypeEnum invoiceType)
        {
            try
            {
                var types = GetInvoiceTypes(invoiceType);
                using (var db = GetDataContext())
                {
                    return db.Invoices.Where(s => types.Contains((int)s.InvoiceTypeId) && s.ApproveDate != null && s.MemberId == ApplicationManager.Instance.GetMember.Id).Select(i => new InvoiceModel
                    {
                        Id = i.Id,
                        CreateDate = i.CreateDate,
                        ApproveDate = i.ApproveDate,
                        Creator = i.Creator,
                        Approver = i.Approver,
                        InvoiceNumber = i.InvoiceNumber,
                        InvoiceTypeId = i.InvoiceTypeId,
                        ProviderName = i.ProviderName,
                        RecipientName = i.RecipientName,
                        Total = i.Summ
                    }).ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceModel>();
            }
        }

        private static List<InvoiceModel> TryGetUnaccepedInvoicesDescriptions(InvoiceTypeEnum invoiceType)
        {
            try
            {
                var types = GetInvoiceTypes(invoiceType);
                using (var db = GetDataContext())
                {
                    return db.Invoices.Where(s => types.Contains((int)s.InvoiceTypeId) && s.MemberId == ApplicationManager.Instance.GetMember.Id && s.ApproveDate == null).Select(i => new InvoiceModel
                    {
                        Id = i.Id,
                        CreateDate = i.CreateDate,
                        ApproveDate = i.ApproveDate,
                        Creator = i.Creator,
                        Approver = i.Approver,
                        InvoiceNumber = i.InvoiceNumber,
                        InvoiceTypeId = i.InvoiceTypeId,
                        ProviderName = i.ProviderName,
                        RecipientName = i.RecipientName,
                        Total = i.Summ
                    }).ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceModel>();
            }
        }

        private static IInvoiceReport TryGetInvoicesReport(DateTime startDate, DateTime endDate, InvoiceType invoiceType)
        {
            var invocieName = "";
            switch (invoiceType)
            {
                case InvoiceType.PurchaseInvoice:
                    invocieName = "Գնում";
                    break;
                case InvoiceType.SaleInvoice:
                    invocieName = "Վաճառք";
                    break;
                case InvoiceType.ProductOrder:
                    break;
                case InvoiceType.MoveInvoice:
                    invocieName = "Տեղափոխում";
                    break;
                case InvoiceType.InventoryWriteOff:
                    invocieName = "Դուրսգրում";
                    break;
                case InvoiceType.ReturnFrom:
                    invocieName = "Վերադարձ";
                    break;
                case InvoiceType.ReturnTo:
                    invocieName = "Վերադարձ մատակարարին";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("invoiceType", invoiceType, null);
            }

            try
            {
                using (var db = GetDataContext())
                {
                    var items = db.InvoiceItems.Where(s => s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate && invoiceType == (InvoiceType)s.Invoices.InvoiceTypeId && s.Invoices.ApproveDate != null && s.Invoices.MemberId == ApplicationManager.Member.Id).ToList();
                    var report = new InvoiceReport
                    {
                        Description = invocieName,
                        Count = items.GroupBy(s => s.InvoiceId).Count(),
                        Quantity = items.Sum(ii => ii.Quantity ?? 0),
                        Cost = items.Sum(ii => ii.Quantity * ii.CostPrice ?? 0),
                        Sale = items.Sum(ii => ii.Quantity * ii.Price ?? 0)
                    };
                    return report;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<IInvoiceReport> TryGetInvoiceReports(DateTime startDate, DateTime endDate, List<InvoiceType> invoiceTypes)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var items = db.InvoiceItems.Where(s => s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate && invoiceTypes.Contains((InvoiceType)s.Invoices.InvoiceTypeId) && s.Invoices.ApproveDate != null && s.Invoices.MemberId == ApplicationManager.Member.Id);

                    return new List<IInvoiceReport>(items.Select(s => new InvoiceReport
                    {
                        Date = s.Invoices.CreateDate,
                        Description = "",
                        Quantity = s.Quantity ?? 0,
                        Price = s.Price ?? 0,
                    }).ToList());
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<IInvoiceReport> TryGetInvoicesReports(DateTime startDate, DateTime endDate, List<InvoiceType> invoiceTypes)
        {
            var reports = new List<IInvoiceReport>();
            foreach (var invoiceType in invoiceTypes)
            {
                var report = TryGetInvoicesReport(startDate, endDate, invoiceType);
                if (report == null) return null;
                reports.Add(report);
            }
            return reports;
        }

        private static List<InvoiceReportByPartner> TryGetInvoicesReportsByPartnerTypes(DateTime startDate, DateTime endDate, InvoiceType invoiceType, List<PartnerType> partnerTypes, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var report = db.Partners.Where(p => partnerTypes.Contains((PartnerType)p.EsPartnersTypeId)).Join(db.InvoiceItems.Where(s => s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate && (InvoiceType)s.Invoices.InvoiceTypeId == invoiceType && s.Invoices.ApproveDate != null && s.Invoices.MemberId == memberId), p => p.Id, ii => ii.Invoices.PartnerId, (p, ii) => new { p, ii }).GroupBy(s => s.ii.InvoiceId).Select(s => new InvoiceReportByPartner
                    {
                        Invoice = s.FirstOrDefault().ii.Invoices.InvoiceNumber,
                        Date = s.FirstOrDefault().ii.Invoices.ApproveDate ?? DateTime.MinValue,
                        Partner = s.FirstOrDefault().p.FullName,
                        Count = s.Count(),
                        Quantity = s.Sum(t => t.ii.Quantity ?? 0),
                        Cost = s.Sum(t => (t.ii.Quantity ?? 0) * (t.ii.CostPrice ?? 0)),
                        Sale = s.Sum(t => (t.ii.Quantity ?? 0) * (t.ii.Price ?? 0)),
                        Approver = s.FirstOrDefault().ii.Invoices.Approver
                    }).ToList();
                    return report;
                }
            }
            catch (Exception)
            {
                return new List<InvoiceReportByPartner>();
            }
        }

        private static List<InvoiceReportByPartner> TryGetInvoicesReportsByPartners(DateTime startDate, DateTime endDate, InvoiceType invoiceType, List<Guid> partnerIds, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var invoiceItems = db.InvoiceItems
                        .Where(s => s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate && (InvoiceType)s.Invoices.InvoiceTypeId == invoiceType && s.Invoices.ApproveDate != null && s.Invoices.MemberId == memberId && s.Invoices.PartnerId != null && partnerIds.Contains((Guid)s.Invoices.PartnerId)).ToList();
                    var report = invoiceItems.Join(db.Partners, ii => ii.Invoices.PartnerId, p => p.Id, (ii, p) => new { ii, p })
                        .GroupBy(s => s.ii.InvoiceId).Where(s => s.FirstOrDefault() != null).Select(s => new InvoiceReportByPartner
                    {
                        Invoice = s.First().ii.Invoices.InvoiceNumber,
                        Date = s.First().ii.Invoices.ApproveDate ?? DateTime.MinValue,
                        Partner = s.First().p.FullName,
                        Count = s.Count(),
                        Quantity = s.Sum(t => t.ii.Quantity ?? 0),
                        Cost = s.Select(t => t.ii.CostPrice ?? 0).FirstOrDefault(),
                        Price = s.Select(t => t.ii.Price ?? 0).FirstOrDefault(),
                        Sale = s.Sum(t => (t.ii.Quantity ?? 0) * (t.ii.Price ?? 0)),
                        Approver = s.First().ii.Invoices.Approver
                    }).ToList();
                    return report;
                }
            }
            catch (Exception)
            {
                return new List<InvoiceReportByPartner>();
            }
        }

        private static List<SaleReportByPartnerDetiled> TryGetInvoicesReportsByPartnersDetiled(DateTime startDate, DateTime endDate, InvoiceType invoiceType, List<Guid> partnerIds)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var report = db.InvoiceItems.Where(s => s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate && (InvoiceType)s.Invoices.InvoiceTypeId == invoiceType && s.Invoices.ApproveDate != null && s.Invoices.MemberId == ApplicationManager.Member.Id && s.Invoices.PartnerId != null && partnerIds.Contains((Guid)s.Invoices.PartnerId)).Join(db.Partners, ii => ii.Invoices.PartnerId, p => p.Id, (ii, p) => new { ii, p }).Select(s => new SaleReportByPartnerDetiled
                    {
                        Invoice = s.ii.Invoices.InvoiceNumber,
                        Date = s.ii.Invoices.ApproveDate ?? DateTime.MinValue,
                        Partner = s.p.FullName,
                        Code = s.ii.Code,
                        Description = s.ii.Description,
                        Quantity = s.ii.Quantity ?? 0,
                        Cost = s.ii.CostPrice ?? 0,
                        Sale = (s.ii.Price ?? 0) * (s.ii.Quantity ?? 0),
                        Price = s.ii.Price ?? 0,
                        Approver = s.ii.Invoices.Approver
                    }).ToList();
                    return report;
                }
            }
            catch (Exception)
            {
                return new List<SaleReportByPartnerDetiled>();
            }
        }

        private static List<InvoiceReport> TryGetSaleByPartners(DateTime startDate, DateTime endDate, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var report = db.Invoices.Where(s => s.ApproveDate != null && s.ApproveDate >= startDate && s.ApproveDate <= endDate && s.InvoiceTypeId == (long)InvoiceType.SaleInvoice && s.MemberId == memberId).Join(db.Partners, ii => ii.PartnerId, p => p.Id, (ii, p) => new { ii, p }).GroupBy(s => s.p.Id).Select(s => new InvoiceReport
                    {
                        Description = s.FirstOrDefault().p.FullName,
                        Count = s.Count(),
                        //Quantity = s.Sum(t => t.ii.Quantity ?? 0),
                        //Cost = s.Sum(t => (t.ii.Quantity??0) * (t.ii.CostPrice ?? 0)),
                        Sale = s.Sum(t => t.ii.Summ),
                    }).ToList();
                    return report;
                }
            }
            catch (Exception)
            {
                return new List<InvoiceReport>();
            }
        }
        private static List<InvoiceItems> TryGetPurchaseInvoiceItemsByProductId(Guid productId, Guid partnerId, decimal count)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var items = db.InvoiceItems.Where(s => s.ProductId == productId && s.Invoices.PartnerId == partnerId && s.Quantity > 0 && s.Invoices.InvoiceTypeId==(int)InvoiceType.PurchaseInvoice && s.Invoices.MemberId == ApplicationManager.Member.Id).OrderByDescending(s => s.Invoices.ApproveDate);
                    var invoiceItems = new List<InvoiceItems>();
                    decimal sum = 0;
                    foreach (var item in items)
                    {
                        invoiceItems.Add(item);
                        sum += item.Quantity ?? 0;
                        if (sum > count) break;
                    }
                    return invoiceItems;
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }
        private static List<InvoiceItems> TryGetSaleInvoiceItemsByProductId(Guid productId, Guid partnerId, decimal count)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var items = db.InvoiceItems.Where(s => s.ProductId == productId && s.Invoices.PartnerId == partnerId && s.Quantity > 0 && s.Invoices.InvoiceTypeId == (int)InvoiceType.SaleInvoice && s.Invoices.MemberId == ApplicationManager.Member.Id).OrderByDescending(s => s.Invoices.ApproveDate);
                    var invoiceItems = new List<InvoiceItems>();
                    decimal sum = 0;
                    foreach (var item in items)
                    {
                        invoiceItems.Add(item);
                        sum += item.Quantity ?? 0;
                        if (sum > count) break;
                    }
                    return invoiceItems;
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }

        #endregion

        #region Invoices report

        private static FinanceReportModel TryGetInvoicesFinance(DateTime? startDate, DateTime? endDate)
        {
            long memberId = ApplicationManager.Instance.GetMember.Id;
            try
            {
                using (var db = GetDataContext())
                {
                    if (startDate == null) startDate = DateTime.Today;
                    if (endDate == null) endDate = DateTime.Today;
                    var productResidue = db.ProductItems.Where(s => s.MemberId == memberId && s.Quantity != 0).GroupBy(s => s.ProductId).Select(s => new ProductResidue
                    {
                        ProductId = s.FirstOrDefault().ProductId,
                        Quantity = s.Sum(t => t.Quantity)
                    }).ToList();
                    //var partners = db.Partners.ToList();
                    var invoiceItems = db.InvoiceItems.Where(ii => ii.Invoices.ApproveDate != null && ii.Invoices.ApproveDate >= startDate && ii.Invoices.ApproveDate <= endDate && ii.Invoices.MemberId == memberId);
                    var items = invoiceItems.Join(db.Partners, x => x.Invoices.PartnerId, p => p.Id, (x, p) => new { x, p }).Select(xp => new InvoiceItemsModel()
                    {
                        Id = xp.x.Id,
                        ProductId = xp.x.ProductId,
                        Index = xp.x.Index ?? 0,
                        Code = xp.x.Code,
                        Description = xp.x.Description,
                        Mu = xp.x.Mu,
                        CostPrice = xp.x.CostPrice,
                        Price = xp.x.Price,
                        Quantity = xp.x.Quantity,
                        Discount = xp.x.Discount,
                        Note = xp.x.Note,
                        InvoiceId = xp.x.InvoiceId,
                        Invoice = new InvoiceModel
                            {
                                Id = xp.x.InvoiceId,
                                InvoiceTypeId = xp.x.Invoices.InvoiceTypeId,
                                InvoiceNumber = xp.x.Invoices.InvoiceNumber,
                                CreateDate = xp.x.Invoices.CreateDate,
                                Creator = xp.x.Invoices.Creator,
                                ApproveDate = xp.x.Invoices.ApproveDate,
                                ApproverId = xp.x.Invoices.ApproverId,
                                Approver = xp.x.Invoices.Approver,
                                MemberId = xp.x.Invoices.MemberId,
                                PartnerId = xp.x.Invoices.PartnerId,
                                Partner = new PartnerModel
                                    {
                                        FirstName = xp.p.FirstName,
                                        LastName = xp.p.LastName,
                                        Address = xp.p.Address,
                                        Mobile = xp.p.Mobile,
                                        FullName = xp.p.FullName
                                    },
                                Total = xp.x.Invoices.Summ,
                                Notes = xp.x.Invoices.Notes
                            }
                    }).ToList();
                    return new FinanceReportModel() { InvocieItems = items, ProductResidues = productResidue };
                }
            }
            catch (Exception ex)
            {
                return new FinanceReportModel();
            }
        }

        private static List<InvoiceItems> TryGetInvoiceItems(DateTime? startDate, DateTime? endDate)
        {
            var memberId = ApplicationManager.Instance.GetMember.Id;
            try
            {
                using (var db = GetDataContext())
                {
                    if (startDate == null) startDate = DateTime.Today;
                    if (endDate == null) endDate = DateTime.Now;
                    return db.InvoiceItems.Include(s => s.Invoices).Where(s => s.Invoices.MemberId == memberId && s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate).ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<InvoiceItems>();
            }
        }

        #endregion

        #region Will bill

        private static List<InternalWayBillDetilesModel> TryGetWillBillByDetile(DateTime fromDate, DateTime toDate, long memberId)
        {
            using (var db = GetDataContext())
            {
                var items = db.InvoiceItems.Where(s => s.Invoices.MemberId == memberId && s.Invoices.ApproveDate >= fromDate && s.Invoices.ApproveDate <= toDate).Join(db.EsStock, ii => ii.Invoices.FromStockId, s1 => s1.Id, (ii, s1) => new { ii, s1 }).Join(db.EsStock, x => x.ii.Invoices.ToStockId, s2 => s2.Id, (x, s2) => new InternalWayBillDetilesModel()
                {
                    Invoice = x.ii.Invoices.InvoiceNumber,
                    ApproveDate = x.ii.Invoices.ApproveDate,
                    Code = x.ii.Code,
                    Description = x.ii.Description,
                    Mu = x.ii.Mu,
                    CostPrice = x.ii.ProductItems.CostPrice,
                    Price = x.ii.Price,
                    Quantity = x.ii.Quantity,
                    Notes = x.ii.Note,
                    FromStock = x.s1.Name,
                    ToStock = s2.Name
                }).OrderBy(s => s.ApproveDate);
                return items.ToList();
            }
        }

        private static List<InternalWayBillModel> TryGetWillBill(DateTime fromDate, DateTime toDate, long memberId)
        {
            using (var db = GetDataContext())
            {
                var invoices = db.Invoices.Where(s => s.MemberId == memberId && s.ApproveDate >= fromDate && s.ApproveDate <= toDate).Join(db.EsStock, ii => ii.FromStockId, s1 => s1.Id, (ii, s1) => new { ii, s1 }).Join(db.EsStock, x => x.ii.ToStockId, s2 => s2.Id, (x, s2) => new InternalWayBillModel()
                {
                    Invoice = x.ii.InvoiceNumber,
                    ApproveDate = x.ii.ApproveDate,
                    Amount = x.ii.Summ,
                    Notes = x.ii.Notes,
                    FromStock = x.s1.Name,
                    ToStock = s2.Name
                }).OrderBy(s => s.ApproveDate);
                //var items = db.InvoiceItems.Where(s => s.Invoices.MemberId == memberId && s.Invoices.ApproveDate >= fromDate && s.Invoices.ApproveDate <= toDate)
                //    .Join(db.EsStock, ii => ii.Invoices.FromStockId, s1 => s1.Id, (ii, s1) => new { ii, s1 })
                //    .Join(db.EsStock, x => x.ii.Invoices.ToStockId, s2 => s2.Id, (x, s2) => new InternalWayBillModel()
                //    {
                //        Invoice = x.ii.Invoices.InvoiceNumber,
                //        ApproveDate = x.ii.Invoices.ApproveDate,
                //        Amount = x.ii.Invoices.Summ,
                //        Notes = x.ii.Note,
                //        FromStock = x.s1.Name,
                //        ToStock = s2.Name
                //    }).OrderBy(s => s.ApproveDate);
                return invoices.ToList();
            }
        }

        #endregion

        #endregion

        #region Invoice External methods

        public static InvoiceModel GetInvoice(Guid? id)
        {
            var invoice = TryGetInvoice(id);
            return ConvertInvoice(invoice, PartnersManager.GetPartner(invoice.PartnerId));
        }

        public static string GetInvoiceNumber(Guid? id, long memberId, EsStockDBEntities db)
        {
            return TryGetInvoiceNumber(id, memberId);
        }

        public static List<InvoiceModel> GetInvoices(IEnumerable<Guid> ids)
        {
            var partners = PartnersManager.GetPartner();
            return TryGetInvoices(ids).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetInvoices(InvoiceType invoiceType)
        {
            var partners = PartnersManager.GetPartner();
            return TryGetInvoices(invoiceType).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetInvoices(InvoiceTypeEnum invoiceType, int? maxInvoiceCount)
        {
            var partners = PartnersManager.GetPartner();
            return TryGetInvoices(invoiceType, maxInvoiceCount).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetApprovedInvoices(InvoiceTypeEnum invoiceType, int? maxInvoiceCount)
        {
            var partners = PartnersManager.GetPartner();
            return TryGetApprovedInvoices(invoiceType, maxInvoiceCount).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetInvoices(DateTime startDate, DateTime endDate)
        {
            var invoices = TryGetInvoices(startDate, endDate);
            var partners = PartnersManager.GetPartner();
            return invoices.Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).ToList();
        }

        public static List<InvoiceModel> GetInvoicesLight(DateTime startDate, DateTime endDate)
        {
            var invoices = TryGetInvoices(startDate, endDate);
            var partners = PartnersManager.GetPartner();
            return invoices.Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).ToList();
        }

        public static List<InvoiceItemsModel> GetInvoiceItems(Guid invoiceId)
        {
            return TryGetInvoiceItems(invoiceId).Select(Convert).ToList();
        }

        public static List<InvoiceItemsModel> GetInvoiceItems(IEnumerable<Guid> invoiceIds)
        {
            return TryGetInvoiceItems(invoiceIds).Select(Convert).ToList();
        }

        public static List<InvoiceItemsModel> GetInvoiceItemsByStocks(List<Guid> invoiceIds, List<StockModel> stocks)
        {
            var items = new List<InvoiceItemsModel>();
            var invoiceItems = TryGetInvoiceItemsByStocks(invoiceIds, stocks.ToList());
            foreach (var newItem in invoiceItems.Select(Convert))
            {
                items.Add(newItem);
            }
            return items;
        }

        public static List<InvoiceItemsModel> GetSaleInvoiceItemsByStocksForReport(DateTime startDate, DateTime endDateTime, List<long> stocks)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.InvoiceItems.Where(s => s.Invoices.MemberId == ApplicationManager.Member.Id && s.ProductItems.StockId != null && stocks.Contains((int)s.ProductItems.StockId) && s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDateTime && (s.Invoices.InvoiceTypeId == (int)InvoiceType.SaleInvoice || s.Invoices.InvoiceTypeId == (int)InvoiceType.InventoryWriteOff || s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnTo || s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnFrom)).Select(s => new InvoiceItemsModel
                    {
                        Id = s.Id,
                        ProductId = s.ProductId,
                        ProductItem = s.ProductItemId != null ? new ProductItemModel()
                            {
                                Id = (Guid)s.ProductItemId,
                                StockId = s.ProductItems.StockId
                            } : null,
                        Invoice = new InvoiceModel { InvoiceTypeId = s.Invoices.InvoiceTypeId },
                        CostPrice = s.CostPrice,
                        Price = s.Price,
                        Quantity = s.Quantity,
                    }).ToList();
                }
                catch (Exception)
                {
                    return new List<InvoiceItemsModel>();
                }
            }
        }

        public static bool SaveInvoice(InvoiceModel invoiceModel, List<InvoiceItemsModel> invoiceItems)
        {
            var invoice = ConvertInvoice(invoiceModel);
            var result = TrySaveInvoice(invoice, invoiceItems.Select(Convert).ToList());
            if (result) invoiceModel.InvoiceNumber = invoice.InvoiceNumber;
            return result;
        }

        public static bool ApprovePurchaseInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, InvoicePaid invoicePaid)
        {
            if (invoice == null || invoice.ToStockId == null || invoiceItems.Count == 0 || invoicePaid == null)
                return false;
            invoice.ApproveDate = DateTime.Now;
            invoice.ApproveDate = invoice.CreateDate;
            return TryApprovePurchaseInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList(), (long)invoice.ToStockId, invoicePaid) != null;
        }

        public static InvoiceModel ApproveSaleInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, List<long> stockIds, InvoicePaid invoicePaid)
        {
            //invoice.ApproveDate = DateTime.Now;
            return ConvertInvoice(TryApproveSaleInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList(), stockIds, invoicePaid), ApplicationManager.Instance.CashProvider.GetPartners.SingleOrDefault(p => p.Id == invoice.PartnerId));
        }

        public static InvoiceModel RegisterInventoryWriteOffInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, IEnumerable<long> stockIds)
        {
            return ConvertInvoice(TryApproveInventoryWriteOffInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList(), stockIds), null);
        }

        public static InvoiceModel ApproveInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, List<StockModel> stocks, InvoicePaid invoicePaid = null)
        {
            if (invoice == null || invoiceItems == null || invoiceItems.Count == 0) return null;
            //todo remove this
            long invoiceIndex = 0;
            using (var db = GetDataContext())
            {
                invoiceIndex = string.IsNullOrEmpty(invoice.InvoiceNumber) ? GetNextInvoiceIndex(invoice.InvoiceTypeId, db) : 0;
            }

            Invoices invoiceDb = null;
            switch ((InvoiceType)invoice.InvoiceTypeId)
            {
                case InvoiceType.PurchaseInvoice:
                    if (invoicePaid == null || invoice.ToStockId == null)
                    {
                        return null;
                    }
                    return ConvertInvoice(TryApprovePurchaseInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList(), (long)invoice.ToStockId, invoicePaid), ApplicationManager.Instance.CashProvider.GetPartners.SingleOrDefault(p => p.Id == invoice.PartnerId));
                    break;
                case InvoiceType.SaleInvoice:
                    if (invoicePaid == null)
                    {
                        return null;
                    }
                    return null;
                    // TryApproveSaleInvoice(ConvertInvoice(invoice), invoiceItems.Select(ConvertInvoiceItem).ToList(), (long)invoice.FromStockId, invoicePaid);
                    break;
                case InvoiceType.MoveInvoice:
                    return ConvertInvoice(TryApproveMoveingInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList()), ApplicationManager.Instance.CashProvider.GetPartners.SingleOrDefault(p => p.Id == invoice.PartnerId));
                    break;
                case InvoiceType.ProductOrder:
                    break;
                case InvoiceType.InventoryWriteOff:
                    break;
                case InvoiceType.ReturnFrom:
                    invoiceDb = ConvertInvoice(invoice);
                    invoiceDb.InvoiceIndex = invoiceIndex;
                    invoiceDb.InvoiceNumber = string.Format("{0}{1}", "RF", invoiceDb.InvoiceIndex);
                    return ConvertInvoice(TryApproveReturnFromInvoice(invoiceDb, invoiceItems.Select(Convert).ToList(), (long)invoice.ToStockId, invoicePaid), ApplicationManager.Instance.CashProvider.GetPartners.SingleOrDefault(p => p.Id == invoice.PartnerId));
                    break;
                case InvoiceType.ReturnTo:
                    invoiceDb = ConvertInvoice(invoice);
                    invoiceDb.InvoiceIndex = invoiceIndex;
                    invoiceDb.InvoiceNumber = string.Format("{0}{1}", "RF", invoiceDb.InvoiceIndex);
                    return ConvertInvoice(TryApproveReturnToInvoice(invoiceDb, invoiceItems.Select(Convert).ToList(), stocks.Select(s => s.Id).ToList(), invoicePaid), ApplicationManager.Instance.CashProvider.GetPartners.SingleOrDefault(p => p.Id == invoice.PartnerId));
                    break;
                default:
                    return null;
            }
            return null;
        }

        public static bool Execute(long memberId, long count)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    ////var invoices = db.Invoices.Where(s => s.MemberId == memberId && s.ApproveDate != null &&
                    ////           s.InvoiceTypeId == (long)InvoiceType.SaleInvoice
                    ////           ).ToList();
                    ////var saleInCash = db.SaleInCash.ToList();

                    ////MessageBox.Show("invoices=" + invoices.Count + "\n"
                    ////    + "saleInCash=" + saleInCash.Count + "\n"
                    ////    + "invoices-saleInCash=" + (invoices.Count - saleInCash.Count) + "\n");
                    //var accountingReceivable = db.AccountsReceivable.Select(s => s.InvoiceId).ToList();
                    //var saleInCashIds = db.SaleInCash.Select(s => s.InvoiceId).ToList();
                    //var invoices = db.Invoices.Where(s => s.MemberId == memberId && s.ApproveDate != null &&
                    //             s.InvoiceTypeId == (long)InvoiceType.SaleInvoice && !saleInCashIds.Contains(s.Id)
                    //             && !accountingReceivable.Contains(s.Id)
                    //             ).ToList();

                    //MessageBox.Show("invoices=" + invoices.Count + "\n");
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageManager.ShowMessage(ex.Message);
                return false;
            }
        }

        //public static bool Execute(long memberId, long count)
        //{
        //    try
        //    {
        //       using (var db = GetDataContext())
        //        {

        //            var exCashDesk = db.CashDesk.FirstOrDefault(s => s.MemberId == memberId && s.IsCash && s.IsActive);
        //            if (exCashDesk == null)
        //            {
        //                return false;
        //            }
        //            var accountingReceivable = db.AccountsReceivable.Select(s => s.InvoiceId).ToList();
        //            var saleInCashIds = db.SaleInCash.Select(s => s.InvoiceId).ToList();
        //            var invoices = db.Invoices.Where(s => s.MemberId == memberId && s.ApproveDate != null &&
        //                        s.InvoiceTypeId == (long)InvoiceType.SaleInvoice && !saleInCashIds.Contains(s.Id)
        //                        && !accountingReceivable.Contains(s.Id)
        //                        ).ToList();
        //            MessageBox.Show("invoices=" + invoices.Count + "\n");
        //            int ind;
        //            for (ind = 0; ind < invoices.Count; ind++)
        //            {
        //                 using (var transaction = new TransactionScope())
        //        {
        //                Invoices invoiceItem = invoices[ind];

        //                #region Add SaleInCash

        //                // 711 - 216 Register cost price in AccountingRecoords 

        //                var costPrice =
        //                    db.InvoiceItems.Where(s => s.InvoiceId == invoiceItem.Id).Sum(s => s.CostPrice*s.Quantity) ??
        //                    0;
        //                var pcAccountingRecords = new AccountingRecords
        //                {
        //                    Id = Guid.NewGuid(),
        //                    RegisterDate = (DateTime) invoiceItem.ApproveDate,
        //                    Amount = costPrice,
        //                    Debit = (long) AccountingRecordsManager.AccountingPlan.CostPrice,
        //                    Credit = (long) AccountingRecordsManager.AccountingPlan.Purchase,
        //                    Description = invoiceItem.InvoiceNumber,
        //                    MemberId = invoiceItem.MemberId,
        //                    RegisterId = (long) invoiceItem.ApproverId,
        //                    DebitLongId = invoiceItem.FromStockId,
        //                };
        //                db.AccountingRecords.Add(pcAccountingRecords);
        //                // 221 - 611 Register in AccountingRecoords Accounting Receivable
        //                var apAccountingRecords = new AccountingRecords
        //                {
        //                    Id = Guid.NewGuid(),
        //                    RegisterDate = (DateTime) invoiceItem.ApproveDate,
        //                    Amount = invoiceItem.Summ,
        //                    Debit = (int) AccountingRecordsManager.AccountingPlan.AccountingReceivable,
        //                    DebitGuidId = invoiceItem.PartnerId,
        //                    Credit = (int) AccountingRecordsManager.AccountingPlan.Proceeds,
        //                    Description = invoiceItem.InvoiceNumber,
        //                    MemberId = invoiceItem.MemberId,
        //                    RegisterId = (long) invoiceItem.ApproverId,
        //                };
        //                db.AccountingRecords.Add(apAccountingRecords);

        //                // 251 - 221 Register in AccountingRecoords
        //                var amount = invoiceItem.Summ -
        //                             db.AccountsReceivable.Where(s => s.InvoiceId == invoiceItem.Id)
        //                                 .Select(s => s.Amount)
        //                                 .FirstOrDefault();
        //                if (amount > 0)
        //                {
        //                    var cpAccountingRecords = new AccountingRecords
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        RegisterDate = (DateTime) invoiceItem.ApproveDate,
        //                        Amount = amount,
        //                        Debit = (int) AccountingRecordsManager.AccountingPlan.Cash,
        //                        DebitGuidId = exCashDesk.Id,
        //                        Credit = (int) AccountingRecordsManager.AccountingPlan.AccountingReceivable,
        //                        CreditGuidId = invoiceItem.PartnerId,
        //                        Description = invoiceItem.InvoiceNumber,
        //                        MemberId = invoiceItem.MemberId,
        //                        RegisterId = (long) invoiceItem.ApproverId,
        //                    };
        //                    db.AccountingRecords.Add(cpAccountingRecords);
        //                    exCashDesk.Total += amount;
        //                    //Change in CashDesk add to SaleInCash
        //                    var exSaleInCash = db.SaleInCash.Where(s => s.InvoiceId == invoiceItem.Id).SingleOrDefault();
        //                    if (exSaleInCash != null)
        //                    {
        //                        //MessageBox.Show("InvoiceId = " + invoiceItem.Id);
        //                        exSaleInCash.Amount = amount;
        //                    }
        //                    else
        //                    {
        //                        var newSaleInCash = new SaleInCash
        //                        {
        //                            Id = Guid.NewGuid(),
        //                            CashDeskId = exCashDesk.Id,
        //                            InvoiceId = invoiceItem.Id,
        //                            Date = (DateTime) invoiceItem.ApproveDate,
        //                            Amount = amount,
        //                            Notes = invoiceItem.Notes,
        //                            CashierId = (long) invoiceItem.ApproverId,
        //                            AccountingRecordsId = cpAccountingRecords.Id
        //                        };
        //                        db.SaleInCash.Add(newSaleInCash);

        //                }

        //                #endregion

        //                #region Add AccountsReceivable

        //            } 
        //                     db.SaveChanges();
        //                transaction.Complete();
        //        }
        //        }


        //                #endregion

        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return false;

        //    }
        //}
        public static List<InvoiceItemsModel> GetInvoiceItemsByCode(IEnumerable<string> codes, DateTime fromDate, DateTime toDate, long memberId)
        {
            var items = new List<InvoiceItemsModel>();
            foreach (var newItem in TryGetInvoiceItemssByCode(codes, fromDate, toDate, memberId).Select(Convert))
            {
                items.Add(newItem);
            }
            return items;
        }

        public static bool CheckForPrize(InvoiceModel invoice)
        {
            if (invoice.ApproveDate < DateTime.Today) return false;
            using (var db = GetDataContext())
            {
                var summ = db.Invoices.Where(s => s.ApproveDate >= DateTime.Today).Sum(s => s.Summ);
                return Math.Truncate(summ / 10000) - Math.Truncate((summ - invoice.Total) / 10000) > 0;
            }
        }

        public static List<InvoiceItemsModel> GetProductHistory(List<Guid> productIds, DateTime fromDate, DateTime toDate, long memberId)
        {
            var items = new List<InvoiceItemsModel>();
            foreach (var newItem in TryGetProductHistory(productIds, fromDate, toDate, memberId).Select(Convert))
            {
                items.Add(newItem);
            }
            return items;
        }

        #region Invoices

        public static List<InvoiceModel> GetInvoicesDescriptions(InvoiceTypeEnum invoiceType, int? maxInvoiceCount)
        {
            return TryGetInvoicesDescriptions(invoiceType, maxInvoiceCount).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetInvoicesDescriptions(InvoiceTypeEnum invoiceType)
        {
            return TryGetInvoicesDescriptions(invoiceType).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetUnacceptedInvoicesDescriptions(InvoiceTypeEnum invoiceType)
        {
            return TryGetUnaccepedInvoicesDescriptions(invoiceType).OrderByDescending(s => s.CreateDate).ToList();
        }

        #endregion

        #region Invoices Reports

        public static FinanceReportModel GetInvoicesFinance(DateTime startDate, DateTime endDate)
        {
            return TryGetInvoicesFinance(startDate, endDate);
        }

        public static List<InvoiceItemsModel> GetInvoiceItems(DateTime startDate, DateTime endDate)
        {
            return TryGetInvoiceItems(startDate, endDate).Select(ConvertLite).ToList();
        }

        public static List<IInvoiceReport> GetInvoicesReports(DateTime startDate, DateTime endDate, List<InvoiceType> invoiceTypes)
        {
            return TryGetInvoicesReports(startDate, endDate, invoiceTypes);
        }

        public static List<IInvoiceReport> GetInvoiceReports(DateTime startDate, DateTime endDate, List<InvoiceType> invoiceTypes)
        {
            return TryGetInvoiceReports(startDate, endDate, invoiceTypes);
        }

        public static List<InvoiceReportByPartner> GetSaleInvoicesReportsByPartnerTypes(DateTime startDate, DateTime endDate, InvoiceType invoiceType, List<PartnerType> partnerTypes, long memberId)
        {
            return TryGetInvoicesReportsByPartnerTypes(startDate, endDate, invoiceType, partnerTypes, memberId);
        }

        public static List<InvoiceReportByPartner> GetSaleInvoicesReportsByPartners(DateTime startDate, DateTime endDate, InvoiceType invoiceType, List<Guid> partnerIds, long memberId)
        {
            return TryGetInvoicesReportsByPartners(startDate, endDate, invoiceType, partnerIds, memberId);
        }

        public static List<SaleReportByPartnerDetiled> GetSaleInvoicesReportsByPartnersDetiled(DateTime startDate, DateTime endDate, InvoiceType invoiceType, List<Guid> partnerIds)
        {
            return TryGetInvoicesReportsByPartnersDetiled(startDate, endDate, invoiceType, partnerIds);
        }

        public static List<InvoiceReport> GetSaleByPartners(Tuple<DateTime, DateTime> dateIntermediate, long memberId)
        {
            return TryGetSaleByPartners(dateIntermediate.Item1, dateIntermediate.Item2, memberId);
        }

        #endregion

        #region Will bill

        public static List<InternalWayBillModel> GetWillBill(DateTime fromDate, DateTime toDate, long memberId)
        {
            var items = TryGetWillBill(fromDate, toDate, memberId);
            return items;
        }

        public static List<InternalWayBillDetilesModel> GetWillBillByDetile(DateTime fromDate, DateTime toDate, long memberId)
        {
            var items = TryGetWillBillByDetile(fromDate, toDate, memberId);
            return items;
        }

        #endregion

        #endregion

        public static bool AutoSaveInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems)
        {
            try
            {
                var filePath = PathHelper.GetMemberTempInvoiceFilePath(invoice.Id, ApplicationManager.Member.Id);
                return !string.IsNullOrEmpty(filePath) && XmlManager.Save(new SerializableInvoice(invoice, invoiceItems), filePath);
            }
            catch (Exception)
            {
                //Ignore
                return false;
            }
        }

        //public static void AutoSave(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems)
        //{
        //    string filePath = PathHelper.GetMemberTempInvoiceFilePath(invoice.Id, ApplicationManager.Member.Id);
        //    if (string.IsNullOrEmpty(filePath)) return;
        //    FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
        //    BinaryFormatter formatter = new BinaryFormatter();
        //    try
        //    {
        //        formatter.Serialize(fs, new Tuple<InvoiceModel, List<InvoiceItemsModel>>(invoice, invoiceItems));
        //    }
        //    catch
        //    {

        //    }
        //    finally
        //    {
        //        fs.Close();
        //    }
        //}
        public static List<SerializableInvoice> LoadAutosavedInvoices()
        {
            var tempFilesPath = PathHelper.GetMemberTempInvoicePath(ApplicationManager.Member.Id);
            var invoices = new List<SerializableInvoice>();
            if (!string.IsNullOrEmpty(tempFilesPath) && Directory.Exists(tempFilesPath))
            {
                foreach (var filePath in Directory.GetFiles(tempFilesPath))
                {
                    if (File.Exists(filePath))
                    {
                        var invoice = XmlManager.Load<SerializableInvoice>(filePath);
                        if (invoice == null) continue;
                        invoices.Add(invoice);
                        File.Delete(filePath);
                    }
                }
            }
            return invoices;
        }

        public static List<InvoiceItemsModel> GetSaleInvoiceByProductId(Guid productId, Guid partnerId, decimal count)
        {
            return TryGetPurchaseInvoiceItemsByProductId(productId, partnerId, count).Select(Convert).ToList();
        }
        public static List<InvoiceItemsModel> GetPurchaseInvoiceByProductId(Guid productId, Guid partnerId, decimal count)
        {
            return TryGetSaleInvoiceItemsByProductId(productId, partnerId, count).Select(Convert).ToList();
        }
        public static bool RemoveAutoSaveInvoice(Guid id)
        {
            string filePath = PathHelper.GetMemberTempInvoiceFilePath(id, ApplicationManager.Member.Id);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static decimal GetInvoiceCost(Guid invoiceId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.InvoiceItems.Where(s => s.InvoiceId == invoiceId && s.Invoices.MemberId == MemberId).Sum(s => s.ProductItems.CostPrice);
                }
                catch (Exception)
                {
                    return 0;
                    throw;
                }
            }
        }

        public static decimal GetInvoiceTotal(Guid invoiceId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.InvoiceItems.Where(s => s.InvoiceId == invoiceId && s.Invoices.MemberId == MemberId).Sum(s => s.Products.Price ?? 0);
                }
                catch (Exception)
                {
                    return 0;
                    throw;
                }
            }
        }

        public static EsMarketInvoice ImportInvoiceFromXml()
        {
            var fileDialog = new OpenFileDialog { Title = "Ապրանքագրի բեռնում", Filter = "xml | *.xml" };
            return fileDialog.ShowDialog() == true ? XmlManager.Load<EsMarketInvoice>(fileDialog.FileName) : null;
        }

        public static bool ExportInvoiceToXml(InvoiceModel invoice, List<InvoiceItemsModel> invoiceitems)
        {
            return XmlManager.Save(Convert(invoice, invoiceitems));
        }

        public static bool ExportInvoiceToXmlAccDoc(InvoiceModel invoice, List<InvoiceItemsModel> invoiceitems)
        {
            return TaxService.Converter.Helpers.AccountingDocumentConverter.ExportToXmlAccDoc(Convert(invoice, invoiceitems));
        }
        public static bool ExportInvoiceToXmlAccDoc(EsMarketInvoice esMarketInvoice)
        {
            //XmlHelper.Save(esMarketInvoice, "d:\\xml.xml");
            return TaxService.Converter.Helpers.AccountingDocumentConverter.ExportToXmlAccDoc(esMarketInvoice);
        }
        private static EsMarketInvoice Convert(InvoiceModel invoice, List<InvoiceItemsModel> invoiceitems)
        {
            EsMarketInvoice esInvoice = new EsMarketInvoice
            {
                InvoiceInfo = new InvoiceInfo
                {
                    InvoiceNumber = invoice.InvoiceNumber,
                },
                BuyerInfo = new EsMarketPartner
                {
                    Name = invoice.Partner.FullName,
                    Address = invoice.Partner.Address,
                    Tin = invoice.Partner.TIN,
                    BankAccount = new BankAccount
                        {
                            BankName = invoice.Partner.Bank,
                            BankAccountNumber = invoice.Partner.BankAccount
                        }
                },
                SupplierInfo = new EsMarketPartner
                {
                },
                DeliveryInfo = new DeliveryInfo
                {
                },
                GoodsInfo = new List<EsGoodInfo>(),
                Notes = invoice.Notes
            };
            foreach (var invoiceItemsModel in invoiceitems)
            {
                esInvoice.GoodsInfo.Add(new EsGoodInfo
                {
                    Code = invoiceItemsModel.Code,
                    Description = invoiceItemsModel.Description,
                    Unit = invoiceItemsModel.Mu,
                    Quantity = invoiceItemsModel.Amount,
                    Price = invoiceItemsModel.Price ?? 0
                });
            }
            return esInvoice;
        }

        public static decimal GetPurchaseInvoicesPrice(DateTime? date)
        {
            if (date == null) return 0;
            using (var db = GetDataContext())
            {
                try
                {
                    //var products = db.Products.Where(s=>s.EsMemberId==ApplicationManager.Member.Id).ToList();
                    return db.InvoiceItems.Where(s => s.Invoices.ApproveDate > date && s.Invoices.MemberId == MemberId &&
                        (s.Invoices.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice || s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnFrom)).Include(s => s.Products).Sum(s => (s.Products.Price ?? 0) * (s.Quantity ?? 0));
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public static Tuple<decimal, decimal> GetSaleInvoicesPrice(DateTime? date)
        {
            if (date == null) return new Tuple<decimal, decimal>(0, 0);
            using (var db = GetDataContext())
            {
                try
                {
                    //var products = db.Products.Where(s=>s.EsMemberId==ApplicationManager.Member.Id).ToList();
                    return new Tuple<decimal, decimal>(0, db.InvoiceItems.Where(s => s.Invoices.ApproveDate > date && s.Invoices.MemberId == MemberId &&
                        (s.Invoices.InvoiceTypeId == (int)InvoiceType.SaleInvoice || s.Invoices.InvoiceTypeId == (int)InvoiceType.ReturnTo || s.Invoices.InvoiceTypeId == (int)InvoiceType.InventoryWriteOff)).Include(s => s.Products).Sum(s => (s.Products.Price ?? 0) * (s.Quantity ?? 0)));
                }
                catch (Exception)
                {
                    return new Tuple<decimal, decimal>(0, 0);
                }
            }
        }

        public static bool BeginTest()
        {
            //InvoicesManager.ExportInvoiceToXmlAccDoc(new EsMarketInvoice()
            //{
            //    GoodsInfo = new List<EsGoodInfo>() { new EsGoodInfo() }
            //});

            return false;
        }
    }
}
