using System.Windows.Forms;
using UserControls.Controls;

namespace UserControls.Helpers
{
    public static class ToolsManager
    {
        public static string GetInputText(string oldValue, string description)
        {
            var form = new InputBox(oldValue, description);
            if (form.ShowDialog() == DialogResult.OK)
            {return form.InputValue;}
            return null;
        }
    }
}
