using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for EditProducts.xaml
    /// </summary>
    public partial class EditItems : Window
    {
       

        public delegate bool EditItem(object item);
        private ObjectItems ObjItems = new ObjectItems();
        private EditItem EditSelectedItem;
        public EditItems(List<Object> items, EditItem editItem)
        {

            InitializeComponent();
            ObjItems.Items = new ObservableCollection<Object>(items);
            EditSelectedItem += editItem;
            //DataContext = ObjItems.Items;
            DgItems.ItemsSource = ObjItems.Items;
        }

        private void BtnEditItem_Click(object sender, EventArgs e)
        {
            var item = ObjItems.Items.FirstOrDefault(s => s == ((Button) sender).Tag);
            if (EditSelectedItem(((Button)sender).Tag)) ObjItems.Items.Remove(item);
        }
        private void BtnEditAll_Click(object sender, EventArgs e)
        {
            var index = 0;
            while (ObjItems.Items.Count > index)
            {
                if (EditSelectedItem(ObjItems.Items[index]))
                {
                    ObjItems.Items.Remove(ObjItems.Items[index]);
                    continue;
                }
                index++;
            }
            if (ObjItems.Items.Count == 0)
            {
                Close();
            }
        }
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

    public class ObjectItems : INotifyPropertyChanged
    {
        private ObservableCollection<Object> _items;
        public ObservableCollection<Object> Items { get { return _items; } set { _items = value; OnPropertyChanged("Items");} }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
