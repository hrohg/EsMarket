using System.Windows.Forms;

namespace UserControls.Controls
{
    public partial class InputBox : Form 
    {
        public string InputValue { get { return TxtInput.Text; } }
        public InputBox(string description)
        {
            InitializeComponent();
            LblDescription.Text = description;
            TxtInput.Focus();
        }
        public InputBox(string inputValue, string description)
        {
            InitializeComponent();
            LblDescription.Text = description;
            TxtInput.Text = inputValue;
            TxtInput.Focus();
        }
        private void TxtInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnOk_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
