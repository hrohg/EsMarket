using System.Windows;

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
        public InputBox(string description)
        {
            InitializeComponent();
            txtDescription.Text = description;
        }
        public InputBox(string description, string inputValue)
        {
            InitializeComponent();
            txtDescription.Text = description;
            TxtImput.Text = inputValue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
