using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using ES.Business.Managers;
using ES.Common.Enumerations;
using UserControls.Views.CustomControls;

namespace UserControls.Managers
{
    public class CashDeskManager
    {
        public static void ViewDebitByPartner(DebitEnum value)
        {
            var partners = PartnersManager.GetPartners();
            if (partners == null)
            {
                MessageBox.Show("։\nԽնդրում ենք փորձել մի փոքր ուշ։", "Թերի տվյալներ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            UIListView view = null;
            switch (value)
            {
                case DebitEnum.None:
                    break;
                case DebitEnum.Debit:
                    var debitList = partners.Where(s => s.Debit != 0).Select(s => new { Գործընկեր = s.Description, Դեբիտորական_պարտք = s.Debit }).ToList();
                    view = new UIListView(debitList, "Դեբիտորական պարտքի դիտում", (double)debitList.Sum(s => s.Դեբիտորական_պարտք));

                    break;
                case DebitEnum.Credit:
                    var creditList = partners.Where(s => s.Credit != 0).Select(s => new { Գործընկեր = s.Description, Կրեդիտորական_պարտք = s.Credit }).ToList();
                    view = new UIListView(creditList, "Կրեդիտորական պարտքի դիտում", (double)creditList.Sum(s => s.Կրեդիտորական_պարտք));
                    break;
                case DebitEnum.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("value", value, null);
            }
            if (view != null)
            {
                view.Show();
            }
        }

        public static void OpenCashDesk()
        {
            if(string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.CashDeskPort)) return;
            var cashDeskPort = new SerialPort(ApplicationManager.Settings.SettingsContainer.MemberSettings.CashDeskPort)
            {
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };
            cashDeskPort.Open();
            if (cashDeskPort.IsOpen)
            {
                cashDeskPort.WriteLine("80000");
                cashDeskPort.Close();
            }
        }
    }
}
