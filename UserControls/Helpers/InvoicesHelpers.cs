using UserControls.ControlPanel.Controls;

namespace UserControls.Helpers
{
    public class InvoicesHelpers
    {
        public static string ReadEmark(string description, string emark = null)
        {
            //var inputWindow = new InputBox(description) { Title = "Հսկիչ նշանի ընթերցում", InputValue = emark };
            //var inputWindow = Ecr.Manager.Helpers.MarkHelper.ReadEmark(description, emark);
            //inputWindow.ShowDialog();
            //if (inputWindow.DialogResult != true) return null;
            //emark = inputWindow.InputValue;
            //return emark;
            return Ecr.Manager.Helpers.MarkHelper.ReadEmark(description, emark);
        }
    }
}
