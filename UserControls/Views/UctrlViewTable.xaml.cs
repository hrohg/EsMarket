using System;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization;

namespace UserControls.Views
{
    /// <summary>
    /// Interaction logic for UctrlViewTable.xaml
    /// </summary>
    public partial class UctrlViewTable : UserControl
    {
        #region private properties

        #endregion

        #region External properties
        public DataGrid DataGrid { get; private set; }
        #endregion

        public UctrlViewTable(object vm)
        {
            InitializeComponent();
            DataContext = vm;
            
        }
        public UctrlViewTable()
        {
            InitializeComponent();
        }

        protected void BtnClose_Click(object sender, EventArgs e)
        {
            var tabitem = this.Parent as TabItem;
            if (tabitem != null)
            {
                var tabControl = tabitem.Parent as TabControl;
                if (tabControl == null) return;
                tabControl.Items.Remove(tabControl.SelectedItem);
            }
        }
    }

}
