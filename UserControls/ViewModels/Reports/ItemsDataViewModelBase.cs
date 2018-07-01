using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;

namespace UserControls.ViewModels.Reports
{
    public abstract class ItemsDataViewModelBase<T> : DocumentViewModel
    {
        public ObservableCollection<T> Items { get; set; }

        protected ItemsDataViewModelBase()
        {

        }

        protected virtual void Initialize()
        {
            Items = new ObservableCollection<T>();
            OnUpdate(null);
        }

        protected abstract void AddItems(T item);
        protected abstract void Update();
        protected abstract void Export();

        protected void OnUpdate(object o)
        {
            if (IsLoading) return;
            lock (Sync)
            {
                if (IsLoading) return;
                IsLoading = true;
            }
            new Thread(Update).Start();
        }

        protected virtual void UpdateCompleted(bool isSuccess)
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { IsLoading = false; });
        }

        private ICommand _updateCommand;
        public ICommand UpdateCommand { get { return _updateCommand ?? (_updateCommand = new RelayCommand(OnUpdate)); } }
    }

    public class FallowProductsViewModel : ItemsDataViewModelBase<ProductModel>
    {
        public FallowProductsViewModel()
        {
            Initialize();
        }

        protected override sealed void Initialize()
        {
            base.Initialize();
            Title = "Կորդ մնացորդի վերլուծություն";
        }
        protected override void AddItems(ProductModel item)
        {
            throw new System.NotImplementedException();
        }

        protected override void Update()
        {
            UpdateCompleted(true);
        }

        protected override void Export()
        {
            throw new System.NotImplementedException();
        }
    }

}
