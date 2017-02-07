using System.Windows;
using System.Windows.Controls;
using UserControls.ViewModels.Invoices;
using UserControls.ViewModels.Reports;

namespace UserControls.Controls
{
    /// <summary>
    /// Interaction logic for UctrlMyTime.xaml
    /// </summary>
    public partial class UctrlMyTime : UserControl
    {
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(MyTime), typeof(UserControl), null);
        public MyTime Time
        {
            //get { return (MyTime)GetValue(TimeProperty); }
            get { return DataContext as MyTime;}
            set { SetValue(TimeProperty, value); }
        }
        //public MyTime Time
        //{
        //    get
        //    {
        //        return (MyTime)DataContext;
        //    }
        //    set { DataContext = value; }
        //}
        public UctrlMyTime()
        {
            InitializeComponent();
            DataContext = Time = new MyTime();
        }
        public UctrlMyTime(MyTime time)
        {
            InitializeComponent();
            DataContext = Time = time;
        }
    }
}
