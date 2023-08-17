using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models.Views;
using ES.Common.Helpers;
using ES.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UserControls.Enumerations;

namespace UserControls.ViewModels.Reports
{
    public class CommodityTurnoverViewModel : ItemsDataViewModelBase<CommodityTurnover>
    {
        #region Internal properties
        protected ProductsViewEnum ViewType;
        protected Tuple<DateTime, DateTime> DateItem { get; set; }
        protected List<PartnerModel> Partners { get; set; }
        #endregion Internal properties

        #region External properties
        public override string Title { get => "Ապրանքաշրջանառություն ըստ մատակարարի " + base.Title; set => base.Title = value; }
        public override string Description
        {
            get =>
                    string.Format("Ապրանքաշրջանառություն ըստ մատակարարի \n Պատվիրատու:{0} \nՄիջակայք:{1} - {2}",
                    Partners.Aggregate("", (partners, partner) => partners + (!string.IsNullOrEmpty(partners) ? " ," : "") + partner),
                    DateItem != null ? DateItem.Item1 : DateTime.Today,
                    DateItem != null ? DateItem.Item2 : DateTime.Now);
            set => base.Description = value;
        }
        public override double TotalCount { get => (double)Items.Sum(s => s.Quantity); }
        #endregion External properties

        #region Constructors
        public CommodityTurnoverViewModel(ProductsViewEnum viewType)
        {
            ViewType = viewType;
        }
        #endregion Constructors
        protected override void UpdateAsync()
        {
            List<PartnerModel> partners = null;
            DispatcherWrapper.Instance.Invoke(System.Windows.Threading.DispatcherPriority.Send, () =>
            {
                partners = SelectItemsManager.SelectPartners(ApplicationManager.CashManager.GetPartners, true, "Ընտրել մատակարար");
                if (partners == null || !partners.Any()) { UpdateCompleted(false); return; }
                Partners = partners;
                DateItem = UIHelper.Managers.SelectManager.GetDateIntermediate(DateItem?.Item1, DateItem?.Item2);
            });

            var invoiceItems = InvoicesManager.GetCommodityTurnover(partners.Select(p => p.Id).ToList(), DateItem);
            var items = invoiceItems.GroupBy(ii => new { ii.Code, ii.InvoiceType }).Select(s =>
              new CommodityTurnover
              {
                  CreateInvoice = s.First().CreateInvoice,
                  Product = s.First().Product,                  
                  Quantity = s.Sum(ii => ii.Quantity)
              })
                .ToList();

            foreach (var item in items)
                DispatcherWrapper.Instance.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, () =>
                {
                    Items.Add(item);
                });
            UpdateCompleted(true);
        }
    }
}
