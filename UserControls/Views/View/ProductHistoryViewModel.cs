using System.Collections.ObjectModel;
using System.Linq;
using ES.Business.Managers;
using ES.Common.Managers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using UserControls.ControlPanel.Controls;

namespace UserControls.Views.View
{
    public class ProductHistoryViewModel : DocumentViewModel
    {
        #region Internal properties

        private ObservableCollection<InvoiceItemsModel> _items;
        #endregion

        #region External properties
        public ObservableCollection<InvoiceItemsModel> InvoiceItems
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("InvoiceItems");
                RaisePropertyChanged("Input");
                RaisePropertyChanged("Move");
                RaisePropertyChanged("Output");
            }
        }
        public InvoiceItemsModel SelectedItem { get; set; }
        public decimal Input
        {
            get
            {
                return InvoiceItems.Where(s => s.Invoice.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice).Sum(s => s.Quantity ?? 0);
            }
        }
        public decimal Move
        {
            get
            {
                return InvoiceItems.Where(s => s.Invoice.InvoiceTypeId == (int)InvoiceType.MoveInvoice).Sum(s => s.Quantity ?? 0);
            }
        }
        public decimal Output
        {
            get
            {
                return InvoiceItems.Where(
                    s => s.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice ||
                    s.Invoice.InvoiceTypeId == (int)InvoiceType.InventoryWriteOff)
                    .Sum(s => s.Quantity ?? 0);
            }
        }
        #endregion

        #region Constructors
        public ProductHistoryViewModel()
        {
            Initialize();
            var date = SelectManager.GetDateIntermediate();
            if(date==null) return;
            var products = Helpers.SelectItemsManager.SelectProductByCheck(true);
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetProductHistory(products.Select(s => s.Id).ToList(), date.Item1, date.Item2, ApplicationManager.Instance.GetMember.Id));
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Ապրանքաշրջանառություն";
        }
        #endregion
    }
}
