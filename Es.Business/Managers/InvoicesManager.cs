using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using ES.Business.Helpers;
using ES.Business.Models;
using ES.Data.Enumerations;
using ES.Data.Model;
using ES.Data.Models;
using ES.Data.Models.Reports;
using ES.DataAccess.Models;
using MessageBox = System.Windows.MessageBox;
using ProductModel = ES.Business.Models.ProductModel;

namespace ES.Business.Managers
{
    public enum InvoiceType
    {
        PurchaseInvoice = 1,
        SaleInvoice = 2,
        ProductOrder = 3,
        MoveInvoice = 4,
        InventoryWriteOff = 5
        , Statement,
    }
    public enum InvoiceState
    {
        All,
        New,
        Saved,
        Accepted,
        Approved
    }

    public enum MaxInvocieCount
    {
        SmallCount = 100,
        BigCount = 1000,
        All
    }
    public class InvoicesManager : BaseManager
    {
        #region Invoice enumerables

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
        private static InvoiceItemsModel ConvertInvoiceItem(InvoiceItems item, List<ProductModel> products, List<ProductItemModel> productItems)
        {
            if (item == null) return null;
            return new InvoiceItemsModel
            {
                Id = item.Id,
                InvoiceId = item.InvoiceId,
                ProductId = item.ProductId,
                Index = item.Index,
                Product = products.SingleOrDefault(s => s.Id == item.ProductId),
                ProductItemId = item.ProductItemId,
                ProductItem = item.ProductItemId != null ? productItems.FirstOrDefault(s => s.Id == item.ProductItemId) : null,
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
                Index = item.Index,
                InvoiceId = item.InvoiceId,
                Invoice = ConvertInvoice(item.Invoices, new PartnerModel()),
                ProductId = item.ProductId,
                Product = ProductsManager.Convert(item.Products),
                ProductItemId = item.ProductItemId,
                ProductItem = new ProductsManager().Convert(item.ProductItems),
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
        private static Int64 GetNextInvoiceIndex(long invoiceTypeId, long memberId)
        {
            using (var db = GetDataContext())
            {
                try
                {
                    return db.Invoices.Where(s => s.MemberId == memberId && s.InvoiceTypeId == invoiceTypeId).Max(s => s.InvoiceIndex) + 1;
                }
                catch (Exception)
                {
                    return 1;
                }
            }
        }
        private static Invoices TryGetInvoice(Guid? id, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.Invoices.SingleOrDefault(s => s.Id == id && s.MemberId == memberId);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static List<Invoices> TryGetInvoices(IEnumerable<Guid> ids, long memberId)
        {
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
                    return db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.MemberId == memberId && s.ApproveDate == null).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }
        private static List<Invoices> TryGetInvoices(InvoiceType invoiceType, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.MemberId == memberId).ToList();
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
                    return db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.ApproveDate == null && s.MemberId == memberId).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }
        private static List<Invoices> TryGetInvoices(InvoiceType invoiceType, int? maxCount, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var items = db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.MemberId == memberId).OrderByDescending(s => s.ApproveDate);
                    return maxCount != null ? items.Take((int)maxCount).ToList() : items.ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }
        private static List<Invoices> TryGetApprovedInvoices(InvoiceType invoiceType, int? maxCount, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var items = db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.ApproveDate != null && s.MemberId == memberId).OrderByDescending(s => s.ApproveDate);
                    return maxCount != null ? items.Take((int)maxCount).ToList() : items.ToList();
                }
            }
            catch (Exception)
            {
                return new List<Invoices>();
            }
        }
        private static List<Invoices> TryGetInvoices(DateTime? startDate, DateTime? endDate, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    if (startDate == null) startDate = DateTime.Today;
                    if (endDate == null) endDate = DateTime.Today;
                    return db.Invoices.Where(s => s.ApproveDate != null
                        && s.ApproveDate >= startDate
                        && s.ApproveDate < endDate
                        && s.MemberId == memberId && s.ApproveDate != null).OrderByDescending(s => s.InvoiceIndex).ToList();
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
                        .Include(s => s.Products.ProductGroup).Where(s => s.InvoiceId == invoiceId).OrderBy(s => s.Code).ToList();
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
                    return db.InvoiceItems.Include(s => s.Invoices).Include(s => s.Products).Include(s => s.ProductItems).Include(s => s.Products.ProductCategories)
                        .Include(s => s.Products.ProductGroup).Where(s => invoiceIds.Contains(s.InvoiceId)).OrderBy(s => s.Code).ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }
        private static List<InvoiceItems> TryGetInvoiceItemsByStocks(IEnumerable<Guid> invoiceIds, IEnumerable<long> stockIds)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.InvoiceItems.Include(s => s.Invoices)
                        .Include(s => s.Products).Include(s => s.ProductItems).Include(s => s.Products.ProductGroup).Include(s => s.Products.ProductCategories)
                        .Where(s => invoiceIds.Contains(s.InvoiceId) && stockIds.ToList().Contains(s.ProductItems.StockId ?? 0)).OrderBy(s => s.Code).ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }
        private static string GetInvocieNumber(long invoiceTypeId, long memberId)
        {
            var invoiceIndex = GetNextInvoiceIndex(invoiceTypeId, memberId);
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
                case InvoiceType.Statement:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("invoiceTypeId", invoiceTypeId, null);
            }
            return null;
        }

        private static bool TrySaveInvoice(Invoices invoice, List<InvoiceItems> invoiceItems)
        {
            using (var transaction = new TransactionScope())
            {
                using (var db = GetDataContext())
                {
                    var exInvoice = db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);
                    if (exInvoice == null)
                    {
                        invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, invoice.MemberId);
                        invoice.InvoiceNumber = GetInvocieNumber(invoice.InvoiceTypeId, invoice.MemberId);
                        invoice.CreateDate = DateTime.Now;
                        db.Invoices.Add(invoice);
                    }
                    else
                    {
                        if (exInvoice.ApproveDate != null) return false;
                        var exInvoiceItems = db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).ToList();
                        foreach (var exInvoiceItem in exInvoiceItems)
                        {
                            db.InvoiceItems.Remove(exInvoiceItem);
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
                    foreach (var invoiceItem in invoiceItems)
                    {
                        db.InvoiceItems.Add(invoiceItem);
                    }
                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
            }
        }

        private static Invoices TryApprovePurchaseInvoice(Invoices invoice, List<InvoiceItems> invoiceItems, long stockId, InvoicePaid invoicePaid)
        {
            
            CashDesk selCashDesk = null;
            CashDesk selCashDeskByCheck = null;
            decimal amount = 0;
            
            amount = (invoicePaid.Paid - invoicePaid.Change) ?? 0;
            if (amount > 0)
            {
                selCashDesk = SelectItemsManager.SelectCashDesks(true, invoice.MemberId, false, "Ընտրել դրամարկղը որտեղից վճարվել է գումարը։").FirstOrDefault();
                if (selCashDesk == null) return null;
            }

            amount = invoicePaid.ByCheck ?? 0;
            if (amount > 0)
            {
                selCashDeskByCheck = SelectItemsManager.SelectCashDesks(false, invoice.MemberId, false, "Ընտրել հաշիվը որտեղից փոխանցվել է գումարը։").FirstOrDefault();
                if (selCashDeskByCheck == null) return null;
            }

            using (var transaction = new TransactionScope())
            {
                using (var db = GetDataContext())
                {
                    invoice.ApproveDate = DateTime.Now;

                    #region Remove Invoice items

                    var exInvoiceItems = db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id).ToList();
                    foreach (var exInvoiceItem in exInvoiceItems)
                    {
                        db.InvoiceItems.Remove(exInvoiceItem);
                    }

                    #endregion

                    #region Update purchase invoice

                    var exInvoice = db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);

                    if (exInvoice == null)
                    {
                        invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, invoice.MemberId);
                        switch (invoice.InvoiceTypeId)
                        {
                            case (long)InvoiceType.SaleInvoice:
                                invoice.InvoiceNumber = string.Format("{0}{1}", "SI", invoice.InvoiceIndex);
                                break;
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
                    foreach (var invoiceItem in invoiceItems)
                    {
                        db.InvoiceItems.Add(invoiceItem);
                        if (invoiceItem.Quantity == null || invoiceItem.Quantity == 0) continue;
                        var productItem = new ProductItems
                        {
                            Id = Guid.NewGuid(),
                            ProductId = invoiceItem.ProductId,
                            DeliveryInvoiceId = invoice.Id,
                            StockId = stockId,
                            Quantity = (decimal)invoiceItem.Quantity,
                            CostPrice = invoiceItem.Price ?? 0,
                            MemberId = invoice.MemberId
                        };
                        invoiceItem.ProductItemId = productItem.Id;
                        db.ProductItems.Add(productItem);
                    }

                    #endregion

                    var exPartner = db.Partners.Single(s => s.Id == invoice.PartnerId);
                    if (exPartner.Debit == null) exPartner.Debit = 0;
                    if (exPartner.Credit == null) exPartner.Credit = 0;

                    #region Add Purchase 216 - 521

                    // 216 - 521 Register in AccountingRecoords Accounting Receivable
                    amount = invoice.Summ;
                    var apAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = amount,
                        Debit = (int)AccountingRecordsManager.AccountingPlan.Purchase,
                        DebitLongId = invoice.ToStockId,
                        Credit = (int)AccountingRecordsManager.AccountingPlan.PurchasePayables,
                        CreditGuidId = exPartner.Id,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                    };
                    db.AccountingRecords.Add(apAccountingRecords);
                    exPartner.Credit += amount;

                    #endregion

                    #region Paid 521 - 251

                    amount = (invoicePaid.Paid - invoicePaid.Change ?? 0);
                    if (amount > 0)
                    {
                        if (selCashDesk == null) return null;
                        var exCashDesk = db.CashDesk.SingleOrDefault(s => s.Id == selCashDesk.Id && s.MemberId == invoice.MemberId);

                        // 521 - 251 Register in AccountingRecoords
                        var cpAccountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingRecordsManager.AccountingPlan.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingRecordsManager.AccountingPlan.CashDesk,
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
                        if (selCashDeskByCheck == null) return null;
                        var exCashDeskByCheck = db.CashDesk.SingleOrDefault(s => s.Id == selCashDeskByCheck.Id && s.MemberId == invoice.MemberId);
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
                            Debit = (int)AccountingRecordsManager.AccountingPlan.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingRecordsManager.AccountingPlan.CashDesk,
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
                            Debit = (int)AccountingRecordsManager.AccountingPlan.PurchasePayables,
                            DebitGuidId = exPartner.Id,
                            Credit = (int)AccountingRecordsManager.AccountingPlan.Prepayments,
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
                        MessageBox.Show(string.Format("{0} \n {1}",ex.Message, ex.InnerException!=null? ex.InnerException.Message: string.Empty));
                        return null;
                    }
                }
            }
        }

        private static Invoices TryApproveSaleInvoice(Invoices invoice, List<InvoiceItems> invoiceItems, IEnumerable<long> fromStockIds, InvoicePaid invoicePaid)
        {
            CashDesk cashDeskByCheck = null;
            if (invoicePaid.ByCheck != null && invoicePaid.ByCheck > 0)
            {
                cashDeskByCheck = SelectItemsManager.SelectCashDesks(false, invoice.MemberId, false, "Ընտրել հաշիվը որտեղ փոխանցվել է գումարը։").FirstOrDefault();
            }
            using (var transaction = new TransactionScope())
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
                        invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, invoice.MemberId);
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
                        exInvoice.ApproveDate = DateTime.Now; // invoice.ApproveDate;
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
                    List<ProductItems> productItemsFifo = db.ProductItems.Where(s => productsIds.Contains(s.ProductId) && s.Quantity > 0).ToList();
                    decimal costPrice = 0;
                    foreach (var invoiceItem in invoiceItems)
                    {
                        var quantity = invoiceItem.Quantity ?? 0;
                        while (quantity > 0)
                        {
                            var productItemFifo = productItemsFifo.FirstOrDefault(s => s.StockId != null && fromStockIds.Contains((long)s.StockId) && s.ProductId == invoiceItem.ProductId && s.Quantity > 0);
                            if (productItemFifo == null)
                            {
                                MessageBox.Show("Անբավարար միջոցներ " + invoiceItem.Code + " " + invoiceItem.Description + " " + invoiceItem.Quantity, "Գործողության ընդհատոմ", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        }
                    }

                    // 711 - 216 Register cost price in AccountingRecoords

                    var pcAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = costPrice,
                        Debit = (long)AccountingRecordsManager.AccountingPlan.CostPrice,
                        Credit = (long)AccountingRecordsManager.AccountingPlan.Purchase,
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
                        ApplicationManager.MessageManager.OnNewMessage(
                            new MessageModel("Պատվիրատու ընտրված չէ։ Ընտրեք պատվիրատու և փորձեք կրկին։", MessageModel.MessageTypeEnum.Warning));
                        return null;
                    }
                    if (exPartner.Credit == null) exPartner.Credit = 0;
                    if (exPartner.Debit == null) exPartner.Debit = 0;

                    #region Accounting Receivable

                    // 221 - 611 Register in AccountingRecoords Accounting Receivable
                    var amount = invoice.Summ;
                    var apAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = amount,
                        Debit = (int)AccountingRecordsManager.AccountingPlan.AccountingReceivable,
                        DebitGuidId = exPartner.Id,
                        Credit = (int)AccountingRecordsManager.AccountingPlan.Proceeds,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                    };
                    db.AccountingRecords.Add(apAccountingRecords);
                    exPartner.Debit += amount;

                    #endregion

                    #region Cash

                    if (invoicePaid.Paid != null && invoicePaid.Paid > 0)
                    {
                        var cashDesk = CashDeskManager.GetDefaultSaleCashDesk(invoice.MemberId);
                        var exCashDesk = cashDesk != null ? db.CashDesk.SingleOrDefault(s => s.Id == cashDesk.Id && s.MemberId == invoice.MemberId) : null;
                        if (exCashDesk == null)
                        {
                            ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Դրամարկղ ընտրված չէ։ Ընտրեք դրամարկղ և փորձեք կրկին։", MessageModel.MessageTypeEnum.Warning));
                            return null;
                        }
                        amount = (invoicePaid.Paid - invoicePaid.Change ?? 0);
                        // 251 - 221 Register in AccountingRecoords
                        var cpAccountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingRecordsManager.AccountingPlan.CashDesk,
                            DebitGuidId = exCashDesk.Id,
                            Credit = (int)AccountingRecordsManager.AccountingPlan.AccountingReceivable,
                            CreditGuidId = invoice.PartnerId,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(cpAccountingRecords);
                        exCashDesk.Total += amount;
                        exPartner.Debit -= amount;
                        //Change in CashDesk add to SaleInCash

                        var newSaleInCash = new SaleInCash
                        {
                            Id = Guid.NewGuid(),
                            CashDeskId = exCashDesk.Id,
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Notes = invoice.Notes,
                            CashierId = (long)invoice.ApproverId,
                            AccountingRecordsId = cpAccountingRecords.Id
                        };
                        db.SaleInCash.Add(newSaleInCash);
                    }

                    #endregion

                    #region ByCheck

                    amount = invoicePaid.ByCheck ?? 0;
                    if (amount > 0)
                    {
                        var exCashDeskByCheck = (cashDeskByCheck != null) ? db.CashDesk.SingleOrDefault(s => s.Id == cashDeskByCheck.Id && s.MemberId == invoice.MemberId) : null;
                        if (exCashDeskByCheck == null)
                        {
                            ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Անկանխիկ դրամարկղ ընտրված չէ։", MessageModel.MessageTypeEnum.Warning));
                            return null;
                        }
                        // 251 - 221 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingRecordsManager.AccountingPlan.CashDesk,
                            DebitGuidId = (Guid)exCashDeskByCheck.Id,
                            Credit = (int)AccountingRecordsManager.AccountingPlan.AccountingReceivable,
                            CreditGuidId = invoice.PartnerId,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exCashDeskByCheck.Total += amount;
                        exPartner.Debit -= amount;
                        //Change in CashDesk add to SaleInCash
                        var newSaleInCashByCheck = new SaleInCash
                        {
                            Id = Guid.NewGuid(),
                            CashDeskId = exCashDeskByCheck.Id,
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Notes = invoice.Notes,
                            CashierId = (long)invoice.ApproverId
                        };
                        db.SaleInCash.Add(newSaleInCashByCheck);
                    }

                    #endregion

                    #region Add ReceivedPrepayment

                    amount = invoicePaid.ReceivedPrepayment ?? 0;
                    if (amount > 0)
                    {
                        // 221 - 523 Register in AccountingRecoords
                        var accountingRecords = new AccountingRecords
                        {
                            Id = Guid.NewGuid(),
                            RegisterDate = (DateTime)invoice.ApproveDate,
                            Amount = amount,
                            Debit = (int)AccountingRecordsManager.AccountingPlan.AccountingReceivable,
                            Credit = (int)AccountingRecordsManager.AccountingPlan.ReceivedInAdvance,
                            Description = invoice.InvoiceNumber,
                            MemberId = invoice.MemberId,
                            RegisterId = (long)invoice.ApproverId,
                        };
                        db.AccountingRecords.Add(accountingRecords);
                        exPartner.Debit -= amount;
                        exPartner.Credit -= amount;
                    }

                    #endregion

                    #region Add AccountsReceivable

                    amount = invoicePaid.AccountsReceivable ?? 0;
                    if (amount > 0)
                    {
                        //Change in CashDesk add to SaleInCash
                        var newAccountsReceivable = new AccountsReceivable()
                        {
                            Id = Guid.NewGuid(),
                            InvoiceId = invoice.Id,
                            Date = (DateTime)invoice.ApproveDate,
                            Amount = amount,
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
                            MessageBox.Show("Դեպիտորական պարտքը սահմանվածից ավել է։", "Անբավարար միջոցներ");
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
                        return null;
                    }
                }
            }
        }

        private static Invoices TryApproveInventoryWriteOffInvoice(Invoices invoice, List<InvoiceItems> invoiceItems, IEnumerable<long> fromStockIds)
        {
            using (var transaction = new TransactionScope())
            {
                using (var db = GetDataContext())
                {
                    invoice.ApproveDate = DateTime.Now;

                    #region Remove InvoiceItems

                    var exInvoiceItems = db.InvoiceItems.Where(s => s.InvoiceId == invoice.Id);
                    foreach (var exInvoiceItem in exInvoiceItems)
                    {
                        db.InvoiceItems.Remove(exInvoiceItem);
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        ApplicationManager.MessageManager.OnNewMessage(new MessageModel(ex.Message, MessageModel.MessageTypeEnum.Warning));
                        return null;
                    }
                    #endregion

                    #region Edit Invoices

                    var exInvoice = db.Invoices.SingleOrDefault(s => s.Id == invoice.Id && s.MemberId == invoice.MemberId);
                    if (exInvoice == null)
                    {
                        invoice.InvoiceIndex = GetNextInvoiceIndex(invoice.InvoiceTypeId, invoice.MemberId);
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
                        ApplicationManager.MessageManager.OnNewMessage(new MessageModel(ex.Message, MessageModel.MessageTypeEnum.Warning));
                        return null;
                    }
                    #endregion

                    #region Add InvoiceItems and edit ProductItems quantity

                    var productsIds = invoiceItems.Select(t => t.ProductId).ToList();
                    List<ProductItems> productItemsFifo = db.ProductItems.Where(s => productsIds.Contains(s.ProductId) && s.Quantity > 0).ToList();
                    decimal costPrice = 0;
                    foreach (var invoiceItem in invoiceItems)
                    {
                        var quantity = invoiceItem.Quantity ?? 0;
                        while (quantity > 0)
                        {
                            var productItemFifo = productItemsFifo.FirstOrDefault(s => s.StockId != null && fromStockIds.Contains((long)s.StockId) && s.ProductId == invoiceItem.ProductId && s.Quantity > 0);
                            if (productItemFifo == null)
                            {
                                MessageBox.Show("Անբավարար միջոցներ " + invoiceItem.Code + " " + invoiceItem.Description + " " + invoiceItem.Quantity, "Գործողության ընդհատոմ", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        }
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        ApplicationManager.MessageManager.OnNewMessage(new MessageModel(ex.Message, MessageModel.MessageTypeEnum.Warning));
                        return null;
                    }
                    // 714 - 216 Register cost price in AccountingRecoords
                    var pcAccountingRecords = new AccountingRecords
                    {
                        Id = Guid.NewGuid(),
                        RegisterDate = (DateTime)invoice.ApproveDate,
                        Amount = costPrice,
                        Debit = (long)AccountingRecordsManager.AccountingPlan.OtherOperationalExpenses,
                        Credit = (long)AccountingRecordsManager.AccountingPlan.Purchase,
                        Description = invoice.InvoiceNumber,
                        MemberId = invoice.MemberId,
                        RegisterId = (long)invoice.ApproverId,
                        DebitLongId = invoice.FromStockId,
                    };
                    db.AccountingRecords.Add(pcAccountingRecords);

                    #endregion

                    try
                    {
                        db.SaveChanges();
                        transaction.Complete();
                        return invoice;
                    }
                    catch (Exception ex)
                    {
                        ApplicationManager.MessageManager.OnNewMessage(new MessageModel(ex.Message, MessageModel.MessageTypeEnum.Warning));
                        return null;
                    }
                    
                }
            }
        }

        private static Invoices TryApproveMoveingInvoice(Invoices invoice, List<InvoiceItems> invoiceItems)
        {
            using (var transaction = new TransactionScope())
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
                        invoice.InvoiceNumber = GetInvocieNumber(invoice.InvoiceTypeId, invoice.MemberId);
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
                    List<ProductItems> productItemsFifo = db.ProductItems.Where(s => s.StockId == invoice.FromStockId && s.Quantity > 0 && productsIds.Contains(s.ProductId)).ToList();
                    decimal costPrice = 0;
                    foreach (var invoiceItem in invoiceItems)
                    {
                        var quantity = invoiceItem.Quantity ?? 0;
                        while (quantity > 0)
                        {
                            var productItemFifo = productItemsFifo.FirstOrDefault(s => s.ProductId == invoiceItem.ProductId && s.Quantity > 0);
                            if (productItemFifo == null)
                            {
                                MessageBox.Show("Անբավարար միջոցներ " + invoiceItem.Code + " " + invoiceItem.Description + " " + invoiceItem.Quantity, "Գործողության ընդհատոմ", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        Debit = (int)AccountingRecordsManager.AccountingPlan.Purchase,
                        DebitLongId = invoice.ToStockId,
                        Credit = (int)AccountingRecordsManager.AccountingPlan.Purchase,
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
                        return null;
                    }
                }
            }
        }

        private static List<InvoiceItems> TryGetInvoiceItemssByCode(IEnumerable<string> codes, DateTime fromDate, DateTime toDate, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var invoiceIds = db.Invoices.Where(s => s.MemberId == memberId && s.CreateDate >= fromDate && s.CreateDate < toDate && s.ApproveDate != null && s.InvoiceTypeId == (long)InvoiceType.PurchaseInvoice).Select(s => s.Id);
                    return db.InvoiceItems.Include(s => s.Invoices).Include(s => s.Products).Include(s => s.Products.ProductCategories).Include(s => s.Products.ProductGroup).Include(s => s.ProductItems).Where(s => invoiceIds.Contains(s.InvoiceId) && codes.Contains(s.Code)).OrderBy(s => s.Code).ToList();
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
                    return db.InvoiceItems.Include(s => s.Invoices).Include(s => s.Products).Include(s => s.Products.ProductCategories).Include(s => s.Products.ProductGroup).Include(s => s.ProductItems).Where(s => invoiceIds.Contains(s.InvoiceId) && productIds.Contains(s.ProductId)).OrderBy(s => s.Invoices.ApproveDate).ThenBy(s => s.Code).ToList();
                }
            }
            catch (Exception)
            {
                return new List<InvoiceItems>();
            }
        }

        #region Invoices

        private static List<InvoiceModel> TryGetInvoicesDescriptions(InvoiceType invoiceType, int? maxCount, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var items = db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.ApproveDate != null && s.MemberId == memberId).OrderByDescending(s => s.ApproveDate).Select(i => new InvoiceModel
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

        private static List<InvoiceModel> TryGetInvoicesDescriptions(InvoiceType invoiceType, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.MemberId == memberId).Select(i => new InvoiceModel
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

        private static List<InvoiceModel> TryGetUnaccepedInvoicesDescriptions(InvoiceType invoiceType, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    return db.Invoices.Where(s => s.InvoiceTypeId == (long)invoiceType && s.MemberId == memberId && s.ApproveDate == null).Select(i => new InvoiceModel
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

        private static IInvoiceReport TryGetInvoicesReport(DateTime startDate, DateTime endDate, InvoiceType invoiceType, long memberId)
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
                case InvoiceType.Statement:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("invoiceType", invoiceType, null);
            }

            try
            {
                using (var db = GetDataContext())
                {
                    var items = db.InvoiceItems.Where(s => s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate && invoiceType == (InvoiceType)s.Invoices.InvoiceTypeId && s.Invoices.ApproveDate != null && s.Invoices.MemberId == memberId);
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

        private static List<IInvoiceReport> TryGetInvoicesReports(DateTime startDate, DateTime endDate, List<InvoiceType> invoiceTypes, long memberId)
        {
            var reports = new List<IInvoiceReport>();
            foreach (var invoiceType in invoiceTypes)
            {
                var report = TryGetInvoicesReport(startDate, endDate, invoiceType, memberId);
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
                        Date = s.FirstOrDefault().ii.Invoices.ApproveDate,
                        Partner = s.FirstOrDefault().p.FullName,
                        Count = s.Count(),
                        Quantity = s.Sum(t => t.ii.Quantity ?? 0),
                        Cost = s.Sum(t => (t.ii.Quantity??0) * (t.ii.CostPrice ?? 0)),
                        Sale = s.Sum(t => (t.ii.Quantity??0) * (t.ii.Price ?? 0)),
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
                    var report = db.InvoiceItems.Where(s => s.Invoices.ApproveDate >= startDate && s.Invoices.ApproveDate <= endDate && (InvoiceType)s.Invoices.InvoiceTypeId == invoiceType && s.Invoices.ApproveDate != null && s.Invoices.MemberId == memberId && s.Invoices.PartnerId != null && partnerIds.Contains((Guid)s.Invoices.PartnerId)).Join(db.Partners, ii => ii.Invoices.PartnerId, p => p.Id, (ii, p) => new { ii, p }).GroupBy(s => s.ii.InvoiceId).Select(s => new InvoiceReportByPartner
                    {
                        Invoice = s.FirstOrDefault().ii.Invoices.InvoiceNumber,
                        Date = s.FirstOrDefault().ii.Invoices.ApproveDate,
                        Partner = s.FirstOrDefault().p.FullName,
                        Count = s.Count(),
                        Quantity = s.Sum(t => t.ii.Quantity ?? 0),
                        Cost = s.Sum(t => (t.ii.Quantity??0) * (t.ii.CostPrice ?? 0)),
                        Sale = s.Sum(t => (t.ii.Quantity??0) * (t.ii.Price ?? 0)),
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
                private static List<InvoiceReport> TryGetSaleByPartners(DateTime startDate, DateTime endDate, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var report = db.Invoices
                        .Where(s => s.ApproveDate != null && s.ApproveDate >= startDate && s.ApproveDate <= endDate
                                    && s.InvoiceTypeId == (long)InvoiceType.SaleInvoice &&
                                    s.MemberId == memberId)
                        .Join(db.Partners, ii => ii.PartnerId, p => p.Id, (ii, p) =>
                            new { ii, p }).GroupBy(s => s.p.Id).Select(s => 
                        new InvoiceReport
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

        #endregion

        #region Invoices report

        private static FinanceReportModel TryGetInvoicesFinance(DateTime? startDate, DateTime? endDate, long memberId)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    if (startDate == null) startDate = DateTime.Today;
                    if (endDate == null) endDate = DateTime.Today;
                    var productResidue = db.ProductItems.Where(s => s.MemberId == memberId && s.Quantity != 0).GroupBy(s => s.ProductId).Select(s => new ProductResidue { ProductId = s.FirstOrDefault().ProductId, Quantity = s.Sum(t => t.Quantity) }).ToList();
                    //var partners = db.Partners.ToList();
                    var invoiceItems = db.InvoiceItems.Where(ii => ii.Invoices.ApproveDate != null && ii.Invoices.ApproveDate >= startDate && ii.Invoices.ApproveDate <= endDate && ii.Invoices.MemberId == memberId);
                    var items = invoiceItems.Join(db.Partners, x => x.Invoices.PartnerId, p => p.Id, (x, p) => new { x, p }).Select(xp => new InvoiceItemsModel()
                    {
                        Id = xp.x.Id,
                        ProductId = xp.x.ProductId,
                        Index = xp.x.Index,
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
                                Partner = new PartnerModel { FirstName = xp.p.FirstName, LastName = xp.p.LastName, Address = xp.p.Address, Mobile = xp.p.Mobile, FullName = xp.p.FullName },
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

        public static InvoiceModel GetInvoice(Guid? id, long memberId)
        {
            var invoice = TryGetInvoice(id, memberId);
            return ConvertInvoice(invoice, PartnersManager.GetPartner(invoice.PartnerId, memberId));
        }

        public static List<InvoiceModel> GetInvoices(IEnumerable<Guid> ids, long memberId)
        {
            var partners = PartnersManager.GetPartner(memberId);
            return TryGetInvoices(ids, memberId).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetInvoices(InvoiceType invoiceType, long memberId)
        {
            var partners = PartnersManager.GetPartner(memberId);
            return TryGetInvoices(invoiceType, memberId).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetInvoices(InvoiceType invoiceType, int? maxInvoiceCount, long memberId)
        {
            var partners = PartnersManager.GetPartner(memberId);
            return TryGetInvoices(invoiceType, maxInvoiceCount, memberId).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetApprovedInvoices(InvoiceType invoiceType, int? maxInvoiceCount, long memberId)
        {
            var partners = PartnersManager.GetPartner(memberId);
            return TryGetApprovedInvoices(invoiceType, maxInvoiceCount, memberId).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetInvoices(DateTime startDate, DateTime endDate, long memberId)
        {
            var invoices = TryGetInvoices(startDate, endDate, memberId);
            var partners = PartnersManager.GetPartner(memberId);
            return invoices.Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).ToList();
        }

        public static List<InvoiceModel> GetInvoicesProv(DateTime startDate, DateTime endDate, long memberId)
        {
            var partners = PartnersManager.GetPartner(memberId);
            return TryGetInvoices(startDate, endDate, memberId).Select(s => ConvertInvoice(s, partners.SingleOrDefault(p => p.Id == s.PartnerId))).ToList();
        }

        public static List<InvoiceItemsModel> GetInvoiceItems(Guid invoiceId, long memberId)
        {
            var items = new List<InvoiceItemsModel>();
            foreach (var newItem in TryGetInvoiceItems(invoiceId).Select(Convert))
            {
                items.Add(newItem);
            }
            return items;
        }

        public static List<InvoiceItemsModel> GetInvoiceItems(IEnumerable<Guid> invoiceIds)
        {
            return TryGetInvoiceItems(invoiceIds).Select(Convert).ToList();
        }

        public static List<InvoiceItemsModel> GetInvoiceItemsByStocks(IEnumerable<Guid> invoiceIds, IEnumerable<long> stockIds)
        {
            var items = new List<InvoiceItemsModel>();
            foreach (var newItem in TryGetInvoiceItemsByStocks(invoiceIds, stockIds).Select(Convert))
            {
                items.Add(newItem);
            }
            return items;
        }

        public static bool SaveInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems)
        {
            return TrySaveInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList());
        }

        public static bool ApprovePurchaseInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, InvoicePaid invoicePaid)
        {
            if (invoice == null || invoice.ToStockId == null || invoiceItems.Count == 0 || invoicePaid == null) return false;
            invoice.ApproveDate = DateTime.Now;
            invoice.ApproveDate = invoice.CreateDate;
            return TryApprovePurchaseInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList(), (long)invoice.ToStockId, invoicePaid) != null;
        }

        public static InvoiceModel ApproveSaleInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, IEnumerable<long> stockIds, InvoicePaid invoicePaid)
        {
            invoice.ApproveDate = DateTime.Now;
            //invoice.ApproveDate = invoice.CreateDate;
            return ConvertInvoice(TryApproveSaleInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList(), stockIds, invoicePaid), ApplicationManager.CashManager.GetPartners.SingleOrDefault(p => p.Id == invoice.PartnerId));
        }

        public static InvoiceModel RegisterInventoryWriteOffInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, IEnumerable<long> stockIds)
        {
            return ConvertInvoice(TryApproveInventoryWriteOffInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList(), stockIds), null);
        }

        public static InvoiceModel ApproveInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, InvoicePaid invoicePaid = null)
        {
            if (invoice == null || invoiceItems == null || invoiceItems.Count == 0) return null;
            switch (invoice.InvoiceTypeId)
            {
                case (long)InvoiceType.PurchaseInvoice:
                    if (invoicePaid == null || invoice.ToStockId == null)
                    {
                        return null;
                    }
                    return ConvertInvoice(TryApprovePurchaseInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList(), (long)invoice.ToStockId, invoicePaid), ApplicationManager.CashManager.GetPartners.SingleOrDefault(p => p.Id == invoice.PartnerId));
                    break;
                case (long)InvoiceType.SaleInvoice:
                    if (invoicePaid == null)
                    {
                        return null;
                    }
                    return null; // TryApproveSaleInvoice(ConvertInvoice(invoice), invoiceItems.Select(ConvertInvoiceItem).ToList(), (long)invoice.FromStockId, invoicePaid);
                    break;
                case (long)InvoiceType.MoveInvoice:
                    return ConvertInvoice(TryApproveMoveingInvoice(ConvertInvoice(invoice), invoiceItems.Select(Convert).ToList()), ApplicationManager.CashManager.GetPartners.SingleOrDefault(p => p.Id == invoice.PartnerId));
                    break;
                default:
                    return null;
            }
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
                MessageBox.Show(ex.Message);
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
        public static List<InvoiceItemsModel> GetInvoiceItemssByCode(IEnumerable<string> codes, DateTime fromDate, DateTime toDate, long memberId)
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

        public static List<InvoiceModel> GetInvoicesDescriptions(InvoiceType invoiceType, int? maxInvoiceCount, long memberId)
        {
            return TryGetInvoicesDescriptions(invoiceType, maxInvoiceCount, memberId).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetInvoicesDescriptions(InvoiceType invoiceType, long memberId)
        {
            return TryGetInvoicesDescriptions(invoiceType, memberId).OrderByDescending(s => s.CreateDate).ToList();
        }

        public static List<InvoiceModel> GetUnacceptedInvoicesDescriptions(InvoiceType invoiceType, long memberId)
        {
            var partners = PartnersManager.GetPartner(memberId);
            return TryGetUnaccepedInvoicesDescriptions(invoiceType, memberId).OrderByDescending(s => s.CreateDate).ToList();
        }

        #endregion

        #region Invoices Reports

        public static FinanceReportModel GetInvoicesFinance(DateTime startDate, DateTime endDate, long memberId)
        {
            return TryGetInvoicesFinance(startDate, endDate, memberId);
        }

        public static List<IInvoiceReport> GetInvoicesReports(DateTime startDate, DateTime endDate, List<InvoiceType> invoiceTypes, long memberId)
        {
            return TryGetInvoicesReports(startDate, endDate, invoiceTypes, memberId);
        }

        public static List<InvoiceReportByPartner> GetSaleInvoicesReportsByPartnerTypes(DateTime startDate, DateTime endDate, InvoiceType invoiceType, List<PartnerType> partnerTypes, long memberId)
        {
            return TryGetInvoicesReportsByPartnerTypes(startDate, endDate, invoiceType, partnerTypes, memberId);
        }

        public static List<InvoiceReportByPartner> GetSaleInvoicesReportsByPartners(DateTime startDate, DateTime endDate, InvoiceType invoiceType, List<Guid> partnerIds, long memberId)
        {
            return TryGetInvoicesReportsByPartners(startDate, endDate, invoiceType, partnerIds, memberId);
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
    }
}
