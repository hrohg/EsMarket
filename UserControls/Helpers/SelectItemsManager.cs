﻿using System;
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
using UserControls.Controls;
using UserControls.ViewModels;
using ProductModel = ES.Business.Models.ProductModel;

namespace UserControls.Helpers
{
    public class SelectItemsManager
    {
        public static DataServer SelectServer(List<DataServer> servers = null)
        {
            if (servers == null)
            {
                servers = ConfigSettings.GetDataServers();
            }
            if (servers.Count < 2) { return servers.FirstOrDefault(); }
            var selectItem = new SelectItems(servers.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1})", s.Description, s.Name), SelectedValue = s.Name }).ToList(), false, "Ընտրել սերվեր");
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
            var partners = PartnersManager.GetPartners(ApplicationManager.Instance.GetEsMember.Id);
            if (partners == null || partners.Count == 0) return new List<PartnerModel>();
            if (partners.Count == 1) { return partners; }
            var selectItem = new SelectItems(partners.Select(s => new ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), allowMultipleSelect, title);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<PartnerModel>(); }
            return partners.Where(s => selectItem.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
        }
        public static List<PartnerType> SelectPartnersTypes(bool allowMultipleSelect = false, string title = "Ընտրել")
        {
            var partnerTypes = PartnersManager.GetPartnersTypes(ApplicationManager.Instance.GetEsMember.Id);
            if (partnerTypes == null || partnerTypes.Count == 0) return new List<PartnerType>();
            if (partnerTypes.Count == 1) { return partnerTypes.Select(s => (PartnerType)s.Id).ToList(); }
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
        public static List<EsMemberModel> SelectEsMembers(List<EsMemberModel> members, bool allowMultipleChoise, string title = "Ընտրել համակարգ")
        {
            if (members == null || members.Count == 0) return new List<EsMemberModel>();
            if (members.Count == 1) return members;
            var ui = new SelectItems(members.Select(s => new ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), allowMultipleChoise, title);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return members.Where(s => ui.SelectedItems.Select(t => (long)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<EsMemberModel>();
        }
        public static List<CashDesk> SelectDefaultCashDesks(bool? isCash, bool allowMultipleChoise, string title)
        {
            var cashDesks = CashDeskManager.GetDefaultCashDesks(isCash).ToList();
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
            var ui = new SelectItems(cashDesks.Select(s => new ItemsToSelect { DisplayName = s.Name, SelectedValue = s.Id }).ToList(), allowMultipleChoise, title);
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
            var vm = new SelectItemsViewModel(items.Select(s => new ItemsToSelectByCheck
                            {
                                IsChecked = selectedItems.Select(t => t.Id).Contains(s.Id),
                                Description = s.BrandName,
                                Value = s.Id
                            }).ToList(), title);
            var selectItem = new SelectItemsByCheck
            {
                DataContext = vm
            };
            var resoult = selectItem.ShowDialog();
            var checkedItems = vm.GetCheckedItems();
            return resoult.HasValue && resoult.Value? items.Where(s => checkedItems.Select(t => (long)t.Value).Contains(s.Id)).ToList():  new List<Brands>();
        }

        public static List<ProductModel> SelectProduct(bool multipleChoose = false)
        {
            var products = ApplicationManager.Instance.CashProvider.Products.ToList();
            if (products.Count < 2) return products;
            var selectedItems = new SelectItems(products.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1} {2} {3})", s.Description, s.Code, s.Barcode, s.Price), SelectedValue = s.Id }).ToList(), false, "Ընտրել արտահանվող ապրանքները");
            return (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.Where(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id)).ToList()
                : new List<ProductModel>();

        }
        public static List<ProductModel> SelectProductByCheck(bool multipleChoose = false)
        {
            var products = ApplicationManager.Instance.CashProvider.Products;
            if (products.Count < 2) return products;
            var vm = new SelectItemsViewModel(products.Select(s => new ItemsToSelectByCheck
            {
                Description = string.Format("{0} ({1} {2} {3})", s.Description, s.Code, s.Barcode, s.Price),
                Value = s.Id
            }).ToList(), "Ընտրել արտահանվող ապրանքները");
            var selectedItems = new SelectItemsByCheck
            {
                DataContext = vm
            };
            var resoult = selectedItems.ShowDialog();
            var checkedItems = vm.GetCheckedItems();
            return (resoult.HasValue && resoult.Value)
                ? products.Where(s => checkedItems.Select(t => t.Value).ToList().Contains(s.Id)).ToList()
                : new List<ProductModel>();

        }
        public static List<InvoiceModel> SelectInvoice(long invoiceTypeId, bool? isApproved = null, bool selectMultiple = false)
        {
            var items = InvoicesManager.GetInvoices((InvoiceType)invoiceTypeId);
            if (items == null) return new List<InvoiceModel>();
            if (isApproved != null)
            {
                items = items.Where(s => (s.ApproveDate != null && (bool)isApproved) || (s.ApproveDate == null && (bool)!isApproved)).ToList();
            }
            return SelectInvoice(items, selectMultiple);
        }

        public static List<InvoiceModel> SelectInvoice(List<InvoiceModel> invoices, bool selectMultiple)
        {
            if (invoices == null) return new List<InvoiceModel>();
            if (invoices.Count < 2)
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
                        (s.ApproveDate != null ? s.ApproveDate.ToString() : ""),
                        s.RecipientName,
                        s.Total),
                        SelectedValue = s.Id
                    }).ToList(), selectMultiple);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return invoices.Where(s => ui.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<InvoiceModel>();
        }
        public static List<InvoiceItemsModel> SelectInvoiceItems(Guid invoiceId, bool selectMultiple = false)
        {
            var items = InvoicesManager.GetInvoiceItems(invoiceId);
            if (items == null || items.Count == 0) { return new List<InvoiceItemsModel>(); }
            var vm = new SelectProductItemsViewModel(items.Select(ii => new ProductItemsToSelect { Id = ii.Id, Code = ii.Code, Description = ii.Description, Quantity = ii.Quantity ?? 0 , Price = ii.Product.Price, IsChecked = true}).ToList(), "Ընտրել ապրանքները");
            var selectedItems = new SelectItemsByCheck
            {
                DataContext = vm
            };
            var resoult = selectedItems.ShowDialog();
            if (resoult == null || resoult.Value == false) return null;
            var checkedItems = vm.GetItems();
            var list = new List<InvoiceItemsModel>();
            foreach (var checkedItem in checkedItems)
            {
                var item = items.Single(i => i.Id == checkedItem.Id);
                item.Quantity = checkedItem.Quantity;
                list.Add(item);
            }
            return list;
        }
        public static List<InvoiceItemsModel> SelectProductItemsFromStock(List<long> stockId, bool selectMultiple = false)
        {
            var items = new ProductsManager().GetProductItemsFromStocks(stockId);
            if (items == null || items.Count == 0) { return new List<InvoiceItemsModel>(); }
            var vm = new SelectProductItemsByCheckViewModel(
                        items.Select(
                            ii =>
                                new ProductItemsByCheck
                                {
                                    Id = ii.Id,
                                    Code = ii.Product.Code,
                                    Description = ii.Product.Description,
                                    Quantity = ii.Quantity
                                }).ToList(), "Ընտրել ապրանքները");
            var selectedItems = new SelectItemsByCheck
                {
                    DataContext = vm
                };
                var resoult = selectedItems.ShowDialog();
                if (resoult == null || resoult.Value == false) return null;
                var checkedItems = vm.GetItems();
                items = checkedItems.Select(item => items.Single(ii => ii.Id == item.Id)).ToList();
            
            var invoiceItems = new List<InvoiceItemsModel>();
            foreach (var ii in items)
            {
                if (ii.Product == null) { continue; }
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
                    Quantity = checkedItems.SingleOrDefault(si => si.Id == ii.Id) != null ?
                    checkedItems.Where(si => si.Id == ii.Id).Select(si => si.Quantity).SingleOrDefault() : 0,
                    Discount = ii.Product.Discount,
                    Note = ii.Product.Note
                };
                if (invoiceItem.Quantity == 0) { continue; }
                invoiceItems.Add(invoiceItem);
            }
            return invoiceItems.ToList();
        }
        #region Select SubAccounting
        public static List<SubAccountingPlanModel> SelectSubAccountingPlan(List<SubAccountingPlanModel> items, bool allowMultipleSelect = false, string title = "Ընտրել")
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
