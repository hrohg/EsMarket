using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for SelectCount.xaml
    /// </summary>
    public partial class SelectCount : Window
    {
        public decimal SelectedCount { get { return ((SelectCountModel) DataContext).Count ?? 0; } }
        public SelectCount(SelectCountModel selectedCountModel, string text = null)
        {
            InitializeComponent();
            DataContext = selectedCountModel;
            TxtCount.Focus();
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    BtnAccept_Click(null, null);
                    break;
                case Key.Escape:
                    DialogResult = false;
                    Close();
                    break;
            }
        }
    }
    /// <summary>
    /// Select count model class
    /// </summary>
    public class SelectCountModel : INotifyPropertyChanged
    {
        public SelectCountModel(decimal? count, decimal? maxCount, string description, bool isCountFree = false)
        {
            MaxCount = maxCount;
            Count = count;
            Descrpiton = description;
            _isCountFree = isCountFree;
        }
        #region Select count model private properties
        private decimal? _maxCount;
        private decimal? _count;
        private bool _isCountFree;
        #endregion
        #region Select count model public properties
        public decimal? Count { get { return _count; } set { _count = value > _maxCount && !_isCountFree ? null : value; } }
        public decimal? MaxCount { get { return _maxCount; } set { _maxCount = value; } }
        public string Descrpiton { get; set; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
