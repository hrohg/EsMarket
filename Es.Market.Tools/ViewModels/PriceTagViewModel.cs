using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using Es.Market.Tools.Helpers;
using Es.Market.Tools.Interfaces;
using Es.Market.Tools.Models;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using ProductModel = ES.Data.Models.Products.ProductModel;

namespace Es.Market.Tools.ViewModels
{
    public class PriceTagViewModel : DocumentViewModel
    {
        Timer _timer;
        private string _productKey;
        private List<ProductModel> _products;
        public ObservableCollection<ILabelTag> Labels { get; private set; }

        public List<ProductModel> Products
        {
            get { return _products.Where(s => string.IsNullOrEmpty(ProductKey) || s.Code.Contains(ProductKey) || s.Barcode.Contains(ProductKey) || s.Description.ToLower().Contains(ProductKey.ToLower())).ToList(); }
        }
        public List<LabelModelBase> LabelTemplates { get; private set; }
        public LabelModelBase SelectedLabelTemplate { get; set; }
        public string ProductKey
        {
            get { return _productKey; }
            set
            {
                _productKey = value; RaisePropertyChanged("ProductKey");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        public int Count { get; set; }
        public PriceTagViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            Title = "Print price tags";
            _products = ApplicationManager.CashManager.GetProducts();
            Labels = new ObservableCollection<ILabelTag>();
            LabelTemplates = new List<LabelModelBase>();
            Labels.CollectionChanged += OnPriceTagCollectionChanged;

            LabelModelBase label = new PriceTag(LabelType.Standard)
            {
                Product = Products.FirstOrDefault()
            };
            LabelTemplates.Add(label);
            label = new PriceDroppedTag
            {
                Product = Products.FirstOrDefault()
            };
            LabelTemplates.Add(label);

            AddItemCommand = new RelayCommand(OnAddItem);
            RemoveItemCommand = new RelayCommand(OnRemoveItem);
            PrintCommand = new RelayCommand(OnPrint, CanPrint);
            CleanCommand = new RelayCommand(OnClean, CanClean);
        }

        private bool CanClean(object obj)
        {
            return Labels.Any();
        }
        private void OnClean(object obj)
        {
            Labels.Clear();
        }
        private bool CanPrint(object obj)
        {
            return Labels.Any();
        }

        private void OnPrint(object obj)
        {
            PrintManager.PrintEx(obj as Grid);
        }

        private void TimerElapsed(object obj)
        {
            RaisePropertyChanged("Products");
            DisposeTimer();
        }
        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
        private void OnRemoveItem(object obj)
        {
            var label = obj as PriceTag;
            if (label != null) Labels.Remove(label);
        }
        private void OnAddItem(object obj)
        {
            var product = obj as ProductModel;
            if (product == null || SelectedLabelTemplate == null) return;
            ILabelTag label = (ILabelTag)SelectedLabelTemplate.Clone();
            label.Product = product;
            Labels.Add(label);
        }

        private void OnPriceTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        public ICommand AddItemCommand { get; private set; }
        public ICommand RemoveItemCommand { get; private set; }
        public ICommand CleanCommand { get; private set; }
        public ICommand PrintCommand { get; private set; }
    }
}
