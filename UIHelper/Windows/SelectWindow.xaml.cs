using System;
using System.Windows;
using UIHelper.Interfaces;

namespace UIHelper.Windows
{
    /// <summary>
    /// Interaction logic for SelectDateIntermediate.xaml
    /// </summary>
    public partial class SelectWindow : Window
    {
        public SelectWindow(ISelectable vm)
        {
            DataContext = vm;
            InitializeComponent();
        }

        #region Events
        private void BtnOk_Click(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion
    }
}
