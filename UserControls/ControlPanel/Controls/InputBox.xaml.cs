using System.Windows;
using System.Windows.Input;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for ImputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public string Description { get { return txtDescription.Text; } set { txtDescription.Text = value; } }
        public string InputValue { get { return TxtImput.Text; } set { TxtImput.Text = value; } }
        public InputBox()
        {
            InitializeComponent();
        }
        public InputBox(string description):this()
        {
            txtDescription.Text = description;
            TxtImput.Focus();
        }
        public InputBox(string description, string inputValue):this(description)
        {            
            TxtImput.Text = inputValue;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    Button_Click(null, null);
                    break;
                case Key.Escape:
                    e.Handled = true;
                    DialogResult = false;
                    Close();
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
