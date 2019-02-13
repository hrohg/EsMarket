using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Data.Enumerations;
using ES.Data.Model;
using ES.Data.Models;
using ES.Common.Managers;
using ES.Common.Models;
using ES.DataAccess.Models;
using UserControls.Controls;
using UserControls.ViewModels;
using ItemsToSelectByCheck = UserControls.Controls.ItemsToSelectByCheck;
using ProductModel = ES.Data.Models.ProductModel;
using SelectItemsByCheck = UserControls.Controls.SelectItemsByCheck;

namespace UserControls.Helpers
{
    public class SelectItemsManager
    {
        public static List<DataServer> SelectServers(List<DataServer> servers = null)
        {
            if (servers == null)
            {
                servers = DataServerSettings.GetDataServers();
            }
            if (servers.Count < 2) { return servers; }
            var selectItem = new SelectItems(servers.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1})", s.Description, s.Name), SelectedValue = s.Description }).ToList(), false, "Ընտրել սերվեր");
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return null; }
            return servers.Where(s => selectItem.SelectedItems.Select(t => t.SelectedValue).Contains(s.Description)).ToList();
        }
        public static List<XmlSettingsItem> SelectServer(List<XmlSettingsItem> servers)
        {
            if (servers == null) return new List<XmlSettingsItem>();
            if (servers.Count < 2) { return servers; }
            var selectItem = new SelectItems(servers.Select(s => new ItemsToSelect { DisplayName = s.Key, SelectedValue = s.Key }).ToList(), false, "Ընտրել սերվեր");
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<XmlSettingsItem>(); }
            return servers.Where(s => selectItem.SelectedItems.Select(t => t.SelectedValue).Contains(s.Key)).ToList();
        }

        public static List<PartnerModel> SelectPartners(List<PartnerModel> partners, bool allowMultipleSelect, string title = "Ընտրել գործընկեր")
        {
            if (partners == null || partners.Count == 0) return new List<PartnerModel>();
            if (partners.Count == 1) { return partners; }
            var selectItem = new SelectItems(partners.Select(s => new ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), allowMultipleSelect, title);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null) { return new List<PartnerModel>(); }
            return partners.Where(s => selectItem.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
        }
        public static List<PartnerModel> SelectPartners(bool allowMultipleSelect = false, string title = "Ընտրել")
        {
            var partners = PartnersManager.GetPartners();
            return SelectPartners(partners, allowMultipleSelect, title);
        }
        public static List<PartnerType> SelectPartnersTypes(bool allowMultipleSelect = false, string title = "Ընտրել")
        {
            var partnerTypes = CashManager.PartnersTypes;
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
        public static List<StockModel> SelectStocks(bool allowMultipleSelect = false)
        {
            var stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(), allowMultipleSelect);
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
        public static List<CashDesk> SelectDefaultSaleCashDesks(bool? isCash, bool allowMultipleChoise, string title)
        {
            List<Guid> cashDeskIds = new List<Guid>();
            if (isCash.HasValue)
            {
                cashDeskIds = isCash.Value ? ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleCashDesks : ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBankAccounts;
            }
            else
            {
                cashDeskIds.AddRange(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleCashDesks);
                cashDeskIds.AddRange(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBankAccounts);
            }
            if (!cashDeskIds.Any())
            {
                cashDeskIds.AddRange(CashDeskManager.GetCashDesks().Select(s => s.Id));
            }
            var cashDesks = SelectCashDesksByIds(cashDeskIds);
            if (cashDesks.Count < 2) return cashDesks;
            var ui = new SelectItems(cashDesks.Select(s => new ItemsToSelect { DisplayName = s.Name, SelectedValue = s.Id }).ToList(), allowMultipleChoise, title);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return cashDesks.Where(s => ui.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<CashDesk>();
        }
        public static List<CashDesk> SelectCashDesks(bool? isCash, long memberId, bool allowMultipleChoise, string title)
        {
            var cashDesks = CashDeskManager.GetCashDesks(isCash);
            if (cashDesks == null || cashDesks.Count == 0) return new List<CashDesk>();
            if (cashDesks.Count == 1) return cashDesks;
            var ui = new SelectItems(cashDesks.Select(s => new ItemsToSelect { DisplayName = s.Name, SelectedValue = s.Id }).ToList(), allowMultipleChoise, title);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return cashDesks.Where(s => ui.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<CashDesk>();
        }
        public static List<CashDesk> SelectCashDesks(bool? isCash, bool allowMultipleChoise = false, string title = "Ընտրել դրամարկղ")
        {
            var cashDesks = CashDeskManager.GetCashDesks(isCash);
            if (cashDesks == null || cashDesks.Count == 0) return new List<CashDesk>();
            if (cashDesks.Count == 1) return cashDesks;
            var ui = new SelectItems(cashDesks.Select(s => new ItemsToSelect { DisplayName = s.Name, SelectedValue = s.Id }).ToList(), allowMultipleChoise, title);
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return cashDesks.Where(s => ui.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<CashDesk>();
        }
        public static List<CashDesk> SelectCashDesksByIds(List<Guid> ids, bool allowMultipleChoise = false, string title = "Ընտրել դրամարկղ")
        {
            var cashDesks = CashDeskManager.GetCashDesks(ids);
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
            return resoult.HasValue && resoult.Value ? items.Where(s => checkedItems.Select(t => (long)t.Value).Contains(s.Id)).ToList() : new List<Brands>();
        }

        public static List<ProductModel> SelectProduct(bool multipleChoose = false)
        {
            var products = ApplicationManager.Instance.CashProvider.Products.ToList();
            return SelectProduct(products, multipleChoose);
        }
        public static List<ProductModel> SelectProduct(List<ProductModel> products, bool multipleChoose = false)
        {
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
            return SelectInvoice(items, OrderInvoiceBy.ApprovedDate, selectMultiple);
        }

        public static List<InvoiceModel> SelectInvoice(List<InvoiceModel> invoices, OrderInvoiceBy orderBy, bool selectMultiple)
        {
            if (invoices == null) return new List<InvoiceModel>();
            if (invoices.Count < 2)
            {
                return invoices;
            }
            SelectItems ui = null;
            switch (orderBy)
            {
                case OrderInvoiceBy.Approver:
                    ui = new SelectItems(
                    invoices.Select(s => new ItemsToSelect
                    {
                        DisplayName = string.Format("{0} {1} {2} -> {3} {4} Գումար - {5} {6}",
                        s.InvoiceNumber,
                        s.CreateDate,
                        s.ProviderName,
                        (s.ApproveDate != null ? s.ApproveDate.ToString() : ""),
                        s.RecipientName,
                        s.Total,
                        s.Approver),
                        SelectedValue = s.Id
                    }).ToList(), selectMultiple);
                    break;
                case OrderInvoiceBy.CreatedDate:
                    ui = new SelectItems(
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
                    break;
                case OrderInvoiceBy.ApprovedDate:
                    ui = new SelectItems(
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orderBy", orderBy, null);
            }

            ui.Topmost = true;
            if (ui.ShowDialog() == true && ui.SelectedItems != null)
            {
                return invoices.Where(s => ui.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
            }
            return new List<InvoiceModel>();
        }
        public static List<InvoiceModel> SelectInvoice(InvoiceTypeEnum type, Tuple<DateTime, DateTime> dataIntermidiate, OrderInvoiceBy orderBy, bool selectMultiple)
        {
            var invoices = InvoicesManager.GetInvoices(dataIntermidiate.Item1, dataIntermidiate.Item2);
            invoices = invoices.Where(s => InvoicesManager.GetInvoiceTypes(type).Contains(((int)s.InvoiceTypeId))).ToList();
            return SelectInvoice(invoices, orderBy, selectMultiple);
        }
        public static List<InvoiceItemsModel> SelectInvoiceItems(Guid invoiceId, bool selectMultiple = false)
        {
            var items = InvoicesManager.GetInvoiceItems(invoiceId);
            if (items == null || items.Count == 0)
            {
                return new List<InvoiceItemsModel>();
            }
            var vm = new SelectProductItemsViewModel(items.Select(ii => new ProductItemsToSelect { Id = ii.Id, Code = ii.Code, Description = ii.Description, Quantity = ii.Quantity ?? 0, Price = ii.Product.Price, IsChecked = true }).ToList(), "Ընտրել ապրանքները");
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
            var items = ProductsManager.GetProductItemsFromStocks(stockId);
            if (items == null || items.Count == 0)
            {
                return new List<InvoiceItemsModel>();
            }
            var vm = new SelectProductItemsByCheckViewModel(items.Select(ii => new ProductItemsByCheck
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
                if (ii.Product == null)
                {
                    continue;
                }
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
                    Quantity = checkedItems.SingleOrDefault(si => si.Id == ii.Id) != null ? checkedItems.Where(si => si.Id == ii.Id).Select(si => si.Quantity).SingleOrDefault() : 0,
                    Discount = ii.Product.Discount,
                    Note = ii.Product.Note
                };
                if (invoiceItem.Quantity == 0)
                {
                    continue;
                }
                invoiceItems.Add(invoiceItem);
            }
            return invoiceItems.ToList();
        }

        public static List<ProductItemsByCheck> SelectProductItems(List<ProductItemsByCheck> productItems, bool selectMultiple = false)
        {
            if (productItems == null || productItems.Count == 0)
            {
                return new List<ProductItemsByCheck>();
            }
            var vm = new SelectProductItemsByCheckViewModel(productItems, "Ընտրել ապրանքները");
            var selectedItems = new SelectItemsByCheck
            {
                DataContext = vm
            };
            var resoult = selectedItems.ShowDialog();
            if (resoult == null || resoult.Value == false) return null;

            return vm.GetItems();
        }

        #region Select SubAccounting

        public static List<SubAccountingPlanModel> SelectSubAccountingPlan(List<SubAccountingPlanModel> items, bool allowMultipleSelect = false, string title = "Ընտրել")
        {
            if (items == null || items.Count == 0) return new List<SubAccountingPlanModel>();
            if (items.Count == 1)
            {
                return items;
            }
            var selectItem = new SelectItems(items.Select(s => new ItemsToSelect { DisplayName = s.Name, SelectedValue = s.Id }).ToList(), allowMultipleSelect, title);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems == null)
            {
                return new List<SubAccountingPlanModel>();
            }
            return items.Where(s => selectItem.SelectedItems.Select(t => (Guid)t.SelectedValue).Contains(s.Id)).ToList();
        }

        #endregion

        public static PartnerModel SelectPartner(PartnerType partnerTypeId = 0)
        {
            var partners = partnerTypeId != 0 ? PartnersManager.GetPartner(partnerTypeId) : PartnersManager.GetPartners();
            if (partners.Count == 0) return null;
            var selectedItems = new SelectItems(partners.Select(s => new ItemsToSelect { DisplayName = s.FullName + " " + s.Mobile, SelectedValue = s.Id }).ToList(), false);
            selectedItems.ShowDialog();
            if (selectedItems.DialogResult == null || selectedItems.DialogResult != true || selectedItems.SelectedItems == null)
            {
                return null;
            }
            return partners.FirstOrDefault(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id));
        }

        public static int? GetDays(int days)
        {
            var win = new SelectCount(new UserControls.SelectCountModel(days, "Մուտքագրել օրերի քանակը"), Visibility.Collapsed);
            win.ShowDialog();
            if (!win.DialogResult.HasValue || !win.DialogResult.Value) { return null; }
            return (int)win.SelectedCount;
        }
    }
}
