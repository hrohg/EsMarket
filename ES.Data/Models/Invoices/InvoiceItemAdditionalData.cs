using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ES.Data.Models.Invoices
{
    public delegate void OnDataChanged();

    [Serializable]
    public class InvoiceItemAdditionalData
    {

        public event OnDataChanged DataChanged;
        public ObservableCollection<string> EMarks { get; set; }

        public InvoiceItemAdditionalData()
        {
            EMarks = new ObservableCollection<string>();
            EMarks.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => { OnDataChanged(); };
        }
        private void OnDataChanged()
        {
            var handler = DataChanged;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
