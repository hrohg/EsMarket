using System;
using System.Collections.Generic;
using System.Linq;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Helpers;
using ES.Data.Enumerations;
using ES.Data.Model;
using ES.Data.Models;
using ES.Common;
using ES.DataAccess.Models;
using ProductModel = ES.Business.Models.ProductModel;

namespace UserControls.Helpers
{
    public class SelectItemsManager
    {
        public static DataServer SelectServer(List<DataServer> servers)
        {
            if (servers == null) return null;
            if (servers.Count < 2) { return servers.FirstOrDefault(); }
            var selectItem = new SelectItems(servers.Select(s => new ItemsToSelect {DisplayName =string.Format("{0} ({1})",s.Description, s.Name)  , SelectedValue = s.Name }).ToList(), false, "Ընտրել սերվեր");
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return null; }
            return servers.FirstOrDefault(s => selectItem.SelectedItems.Select(t => t.SelectedValue).Contains(s.Name));
        }
        public static List<XmlSettingsItem> SelectServer(List<XmlSettingsItem> servers)
        {
            if (servers == null) return new List<XmlSettingsItem>();
            if (servers.Count < 2) { return servers; }
            var selectItem = new SelectItems(servers.Select(s => new ItemsToSelect { DisplayName = s.Key, SelectedValue = s.Key }).ToList(), false, "Ընտրել սերվեր");
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<XmlSettingsItem>(); }
            return servers.Where(s => selectItem.SelectedItems.Select(t => t.SelectedValue).Contains(s.Key)).ToList();
        }

        public static List<PartnerModel> SelectPartners(List<PartnerModel> partners, bool allowMultipleSelect, string title)
        {
            if (partners == null || partners.Count == 0) return new List<PartnerModel>();
            if (partners.Count == 1) { return partners; }
            var selectItem = new SelectItems(partners.Select(s => new ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), allowMultipleSelect, title);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<PartnerModel>(); }
            return partners.Where(s => selectItem.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
        }
        public static List<PartnerModel> SelectPartners(bool allowMultipleSelect = false, string title = "Ընտրել")
        {
            var partners = PartnersManager.GetPartners(ApplicationManager.GetEsMember.Id);
            if (partners == null || partners.Count == 0) return new List<PartnerModel>();
            if (partners.Count == 1) { return partners; }
            var selectItem = new SelectItems(partners.Select(s => new ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), allowMultipleSelect,title);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<PartnerModel>(); }
            return partners.Where(s => selectItem.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
        }
        public static List<PartnerType> SelectPartnersTypes(bool allowMultipleSelect = false,  string title = "Ընտրել")
        {
            var partnerTypes = PartnersManager.GetPartnersTypes(ApplicationManager.GetEsMember.Id);
            if (partnerTypes == null || partnerTypes.Count == 0) return new List<PartnerType>();
            if (partnerTypes.Count == 1) { return partnerTypes.Select(s=>(PartnerType)s.Id).ToList(); }
            var selectItem = new SelectItems(partnerTypes.Select(s => new ItemsToSelect { DisplayName = s.Description, SelectedValue = s.Id }).ToList(), allowMultipleSelect, title);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<PartnerType>(); }
            return partnerTypes.Where(s => selectItem.SelectedItems.Select(t => (long)t.SelectedValue).Contains(s.Id)).Select(s => ((PartnerType)s.Id)).ToList();
        }
        public static List<StockModel> SelectStocks(List<StockModel> stocks, bool allowMultipleSelect = false)
        {
            if (stocks == null) return new List<StockModel>();
            if (stocks.Count < 2) { return stocks; }
            var selectItem = new SelectItems(stocks.Select(s => new ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), allowMultipleSelect);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<StockModel>(); }
            return stocks.Where(s => selectItem.SelectedItems.Select(t => (long)t.SelectedValue).Contains(s.Id)).ToList();
        }
        public static List<EsMemberModel> SelectEsMembers(List<EsMemberModel> members, bool allowMultipleChoise, string title ="Ընտրել համակարգ")
        {
            if(members == null || members.Count==0) return new List<EsMemberModel>();
            if(members.Count==1) return members;
            var ui = new SelectItems(members.Select(s => new ItemsToSelect {DisplayName = s.FullName, SelectedValue = s.Id}).ToList(), allowMultipleChoise,title);
            if (ui.ShowDialog()==true && ui.SelectedItems != null)
            {
                return members.Where(s => ui.SelectedItems.Select(t =>(long)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<EsMemberModel>();
        }
        public static List<CashDesk> SelectDefaultCashDesks(bool? isCash, long memberId, bool allowMultipleChoise, string title)
        {
            var cashDesks = CashDeskManager.GetDefaultCashDesks( memberId,isCash).ToList();
            if (cashDesks.Count == 0) return new List<CashDesk>();
            if (cashDesks.Count == 1) return cashDesks;
            var ui = new SelectItems(cashDesks.Select(s => new ItemsToSelect { DisplayName = s.Name, SelectedValue = s.Id }).ToList(), allowMultipleChoise, title);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return cashDesks.Where(s => ui.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<CashDesk>();
        }
        public static List<CashDesk> SelectCashDesks(bool? isCash, long memberId, bool allowMultipleChoise, string title)
        {
            var cashDesks = CashDeskManager.TryGetCashDesk(isCash, memberId);
            if (cashDesks == null || cashDesks.Count == 0) return new List<CashDesk>();
            if (cashDesks.Count == 1) return cashDesks;
            var ui = new SelectItems(cashDesks.Select(s => new ItemsToSelect { DisplayName = s.Name, SelectedValue = s.Id }).ToList(), allowMultipleChoise,title);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return cashDesks.Where(s => ui.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<CashDesk>();
        }
        public static List<EsUserModel> SelectUser(long memberId, bool allowMultipleChoise, string title)
        {
            var items = UsersManager.GetEsUsers(memberId);
            if (items == null || items.Count == 0) return new List<EsUserModel>();
            if (items.Count == 1) return items;
            var ui = new SelectItems(items.Select(s => new ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.UserId }).ToList(), allowMultipleChoise, title);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return items.Where(s => ui.SelectedItems.Select(t => (long)t.SelectedValue).Contains(s.UserId)).ToList();
            }
            return new List<EsUserModel>();
        }
        public static List<Brands> SelectBrands(List<Brands> brands, bool allowMultipleSelect)
        {
            if (brands == null) return new List<Brands>();
            if (brands.Count < 2) { return brands; }
            var selectItem = new SelectItems(brands.Select(s => new ItemsToSelect { DisplayName = s.BrandName, SelectedValue = s.Id }).ToList(), allowMultipleSelect);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<Brands>(); }
            return brands.Where(s => selectItem.SelectedItems.Select(t => (long)t.SelectedValue).Contains(s.Id)).ToList();
        }

        public static List<Brands> SelectBrands(List<Brands> items, List<Brands> selectedItems, string title)
        {
            if (items == null || items.Count == 0) return new List<Brands>();
            if (items.Count == 1) { return items; }
            var selectItem = new SelectItemsByCheck(items.Select(s => new ItemsToSelectByCheck() {IsChecked = selectedItems.Select(t=>t.Id).Contains(s.Id),  Description= s.BrandName, Value = s.Id }).ToList(), title);
            if (selectItem.ShowDialog() != true || selectItem.SelectedProductItems == null) { return new List<Brands>(); }
            return items.Where(s => selectItem.SelectedItems.Select(t => (long)t.Value).Contains(s.Id)).ToList();
        }

        public static List<ProductModel> SelectProduct(bool multipleChoose=false)
        {
            var products = ApplicationManager.CashManager.Products.ToList();
            if (products.Count < 2) return products;
            var selectedItems = new SelectItems(products.Select(s => new ItemsToSelect
                 {  DisplayName = string.Format("{0} ({1} {2} {3})", s.Description, s.Code, s.Barcode, s.Price), SelectedValue = s.Id }).ToList(), false, "Ընտրել արտահանվող ապրանքները");
            return (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.Where(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id)).ToList()
                : new List<ProductModel>();
            
        }
        public static List<ProductModel> SelectProductByCheck(long memberId, bool multipleChoose = false)
        {
            var products = new ProductsManager().GetProductsShortData(memberId);
            if (products.Count < 2) return products;
            var selectedItems = new SelectItemsByCheck(products.Select(s => new ItemsToSelectByCheck { Description = string.Format("{0} ({1} {2} {3})", s.Description, s.Code, s.Barcode, s.Price), Value = s.Id }).ToList(), "Ընտրել արտահանվող ապրանքները");
            return (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.Where(s => selectedItems.SelectedItems.Select(t => t.Value).ToList().Contains(s.Id)).ToList()
                : new List<ProductModel>();

        }
        public static List<InvoiceModel> SelectInvoice(long invoiceTypeId, long memberid, bool? isApproved = null, bool selectMultiple=false)
        {
            var items = InvoicesManager.GetInvoices((InvoiceType)invoiceTypeId, memberid);
            if (items == null) return new List<InvoiceModel>();
            if (isApproved != null)
            {
                items = items.Where(s => (s.ApproveDate != null && (bool)isApproved) || (s.ApproveDate == null && (bool)!isApproved)).ToList();
            }
            return SelectInvoice(items, selectMultiple);
        }

        public static List<InvoiceModel> SelectInvoice(List<InvoiceModel> invoices, bool selectMultiple)
        {
            if(invoices==null) return new List<InvoiceModel>();
            if (invoices.Count <2)
            {
                return invoices;
            }
            var ui = new SelectItems(
                    invoices.Select(s => new ItemsToSelect
                    {
                        DisplayName = string.Format("{0} {1} {2} -> {3} {4} Գումար - {5}", 
                        s.InvoiceNumber,
                        s.CreateDate,
                        s.ProviderName,
                        (s.ApproveDate!=null?s.ApproveDate.ToString():""), 
                        s.RecipientName,
                        s.Total), SelectedValue = s.Id
                    }).ToList(), selectMultiple);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return invoices.Where(s => ui.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<InvoiceModel>();
        }
        public static List<InvoiceItemsModel> SelectInvoiceItems(Guid invoiceId, long memberId, bool selectMultiple = false)
        {
            var invoiceItems = InvoicesManager.GetInvoiceItems(invoiceId, memberId);
            if(invoiceItems == null || invoiceItems.Count==0) {return new List<InvoiceItemsModel>();}
            var ui = new SelectItemsByCheck(invoiceItems.Select(ii => new ProductToSelectByCheck(ii.Id, ii.Code, ii.Description, ii.Quantity)).ToList(), selectMultiple);
            ui.ShowDialog();
            if (ui.DialogResult == null || ui.DialogResult == false)
            {
                return new List<InvoiceItemsModel>();
            }
            var selectedItems = ui.SelectedProductItems.Where(si => si.Count != null && si.Count > 0).ToList();
            foreach (var ii in invoiceItems)
            {
                ii.Quantity = selectedItems.SingleOrDefault(si => si.GuidId == ii.Id) != null? 
                    selectedItems.Where(si => si.GuidId == ii.Id).Select(si => si.Count).SingleOrDefault(): 0;
            }
            return invoiceItems.Where(ii =>ii.Quantity>0).ToList();
        }
        public static List<InvoiceItemsModel> SelectProductItems(List<long> stockId, long memberId, bool selectMultiple = false)
        {
            var productItems = new ProductsManager().GetProductItemsFromStocks(stockId,memberId);
            if (productItems == null || productItems.Count == 0) { return new List<InvoiceItemsModel>(); }
            var ui = new SelectItemsByCheck(productItems.Select(pi => new ProductToSelectByCheck((Guid)pi.Id, pi.Product.Code, pi.Product.Description, pi.Quantity)).ToList(), selectMultiple);
            ui.ShowDialog();
            if (ui.DialogResult == null || ui.DialogResult == false)
            {
                return new List<InvoiceItemsModel>();
            }
            
            var selectedItems = ui.SelectedProductItems.Where(si => si.Count != null && si.Count > 0).ToList();
            var invoiceItems = new List<InvoiceItemsModel>();
            foreach (var ii in productItems)
            {
                if(ii.Product==null) {continue;}
                var invoiceItem = new InvoiceItemsModel
                {
                    ProductItemId = ii.Id,
                    ProductItem = ii,
                    Product = ii.Product,
                    ProductId = ii.ProductId,
                    Code = ii.Product.Code,
                    Description = ii.Product.Description,
                    Mu = ii.Product.Mu,
                    CostPrice = ii.CostPrice,
                    Price = ii.Product.Price,
                    Quantity =  selectedItems.SingleOrDefault(si => si.GuidId == ii.Id) != null ?
                    selectedItems.Where(si => si.GuidId == ii.Id).Select(si => si.Count).SingleOrDefault() : 0,
                    Discount = ii.Product.Discount,
                    Note = ii.Product.Note
                };
                if (invoiceItem.Quantity == 0) { continue;}
                invoiceItems.Add(invoiceItem);
            }
            return invoiceItems.Where(ii => ii.Quantity > 0).ToList();
        }
        #region Select SubAccounting
         public static List<SubAccountingPlanModel> SelectSubAccountingPlan(List<SubAccountingPlanModel> items, bool allowMultipleSelect=false, string title="Ընտրել")
        {
            if (items == null || items.Count == 0) return new List<SubAccountingPlanModel>();
            if (items.Count == 1) { return items; }
            var selectItem = new SelectItems(items.Select(s => new ItemsToSelect { DisplayName = s.Name, SelectedValue = s.Id }).ToList(), allowMultipleSelect, title);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<SubAccountingPlanModel>(); }
            return items.Where(s => selectItem.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
        }
        #endregion
    }
}
