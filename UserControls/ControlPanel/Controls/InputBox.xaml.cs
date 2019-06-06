using System.Windows;

namespace UserControls.ControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for ImputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public string Description { get { return (string)LblDescription.Content; } set { LblDescription.Content = value; }}
        public string InputValue { get { return TxtImput.Text; } set { TxtImput.Text = value; } }
        public InputBox()
        {
            InitializeComponent();
        }
        public InputBox(string description)
        {
            InitializeComponent();
            LblDescription.Content = description;
        }
        public InputBox(string description, string inputValue)
        {
            InitializeComponent();
            LblDescription.Content = description;
            TxtImput.Text = inputValue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
