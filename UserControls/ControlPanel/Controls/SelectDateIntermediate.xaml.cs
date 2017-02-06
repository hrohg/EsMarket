using System;
using System.Windows;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for SelectDateIntermediate.xaml
    /// </summary>
    public partial class SelectDateIntermediate : Window
    {
        public override void BeginInit()
        {
            base.BeginInit();
        }

        public DateTime? StartDate
        {
            get
            {
                return DtpStartDate.SelectedDate;
            }
            set
            {
                DtpStartDate.SelectedDate = value;
            }
        }

        public DateTime? EndDate
        {
            get
            {
                return DtpEndDate.SelectedDate;
            }
            set
            {
                DtpEndDate.SelectedDate = value;
            }
        }
        public SelectDateIntermediate()
        {
            InitializeComponent();
            StartDate =
                EndDate = DateTime.Now;
        }
        public SelectDateIntermediate(DateTime startDate, DateTime endDate)
        {
            InitializeComponent();
            StartDate = startDate;
            EndDate = endDate;
        }
        #region Events

        private void BtnOk_Click(object sender, EventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        #endregion
    }
}
