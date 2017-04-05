using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models.EsModels;
using Shared.Helpers;

namespace UserControls.ViewModels.Tools
{
    public class CategoriesToolsViewModel : ToolsViewModel
    {
        public delegate void OnSetCategoryDelegate(EsCategoriesModel category);
        public event OnSetCategoryDelegate OnSetCategory;

        #region Internal properties
        #endregion Internal properties

        #region External properties

        #region Filter

        private Timer _timer;
        private string _filter;
        public string Filter
        {
            get { return _filter ?? string.Empty; }
            set
            {
                _filter = value.ToLower();
                RaisePropertyChanged("Filter");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        private void TimerElapsed(object obj)
        {
            RaisePropertyChanged("Items");
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
        #endregion Filter

        private List<EsCategoriesModel> _items;
        public List<EsCategoriesModel> Items
        {
            get
            {
                return string.IsNullOrEmpty(Filter) ? _items :
                GetEsCategoriesList(_items, Filter);
            }
            set
            {
                _items = value;
                if (_items != null)
                {
                    //CategoriesGeneration = new ObservableCollection<CategoriesGeneration>(GenerateCategoriesTree(_categories.Where(s => s.ParentId == null).ToList()));
                    //RaisePropertyChanged("CategoriesGeneration");
                }
                RaisePropertyChanged("Items");
                ParentCategories = value;
            }
        }

        private List<EsCategoriesModel> _parentCategories;
        public List<EsCategoriesModel> ParentCategories
        {
            get { return _parentCategories; }
            set
            {
                _parentCategories = value;
                RaisePropertyChanged("ParentCategories");
            }

        }

        private EsCategoriesModel _category;

        public EsCategoriesModel Category
        {
            get { return _category; }
            set
            {
                if (value == _category) return;
                _category = value;
                RaisePropertyChanged("Category");
            }
        }

        private EsCategoriesModel _selectedItem;

        public EsCategoriesModel SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        #region Is setting parent

        private bool _isSettingParent;
        public bool IsSettingParent
        {
            get { return _isSettingParent; }
            set
            {
                if (value == _isSettingParent) return;
                _isSettingParent = value;
                RaisePropertyChanged("IsSettingParent");
            }
        }
        #endregion Is setting parent

        #endregion External properties

        #region Constructors

        public CategoriesToolsViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region internal methods
        private void Initialize()
        {
            Title = "Ապրանքային խմբեր";
            OnRefresh(null);
            IsLoading = false;
            CanFloat = true;
        }

        private List<EsCategoriesModel> GetEsCategoriesList(List<EsCategoriesModel> categories, string key = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                key = key.ToLower();
            }
            var list = new List<EsCategoriesModel>();
            foreach (var item in categories)
            {
                if (string.IsNullOrEmpty(key) ||
                    (!string.IsNullOrEmpty(item.HcDcs) && item.HcDcs.ToLower().Contains(key) ||
                     (!string.IsNullOrEmpty(item.Name) && item.Name.ToLower().Contains(key)) ||
                     (!string.IsNullOrEmpty(item.Description) && item.Description.ToLower().Contains(key))))
                {
                    list.Add(item);
                }
                else
                {
                    list.AddRange(GetEsCategoriesList(item.Children, key));
                }
            }
            return list;
        } 
        #endregion Internal methods

        #region External methods

        #endregion Extrnal methods

        #region Commands

        public ICommand NewCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }
        public ICommand ActivateCommand { get; private set; }
        public ICommand ImportExportCommand { get; private set; }

        #region Refresh command

        private ICommand _refreshCommand;

        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null) _refreshCommand = new RelayCommand(OnRefresh);
                return _refreshCommand;
            }
        }
        private void OnRefresh(object obj)
        {
            Items = ProductsManager.GetEsCategories();
        }
        #endregion Refresh command

        #region Set category command

        private ICommand _setCategoryCommand;

        public ICommand SetCategoryCommand
        {
            get
            {
                if (_setCategoryCommand == null) _setCategoryCommand = new RelayCommand(NotifySetCategory, CanSetProduct);
                return _setCategoryCommand;
            }
        }

        private bool CanSetProduct(object obj)
        {
            return Category != null;
        }

        private void NotifySetCategory(object obj)
        {
            var handler = OnSetCategory;
            if (handler != null) handler(Category);
        }
        #endregion  Set category command

        #region Selection changed
        private ICommand _selectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (_selectedItemChangedCommand == null)
                    _selectedItemChangedCommand = new RelayCommand(args => SelectedItemChanged(args));
                return _selectedItemChangedCommand;
            }
        }

        private void SelectedItemChanged(object args)
        {
            SelectedItem = args as EsCategoriesModel;
            Category = SelectedItem;
        }
        #endregion Selection changed


        #endregion Commands
    }
}
