using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using CashReg.Managers;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Data.Models;
using UserControls.Helpers;
using UserControls.Views.CustomControls;
using MessageManager = ES.Common.Managers.MessageManager;

namespace UserControls.Managers
{
    public class CashDeskManager
    {
        public static void ViewDebitByPartner(DebitEnum value)
        {
            var partners = PartnersManager.GetPartners();
            if (partners == null)
            {
                MessageManager.ShowMessage("։\nԽնդրում ենք փորձել մի փոքր ուշ։", "Թերի տվյալներ");
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
        public static void CheckDebitByPartner(DebitEnum value)
        {
            var partners = PartnersManager.GetPartners();
            var date = UIHelper.Managers.SelectManager.GetDate();
            if (partners == null || date == null)
            {
                MessageManager.ShowMessage("։\nԽնդրում ենք փորձել մի փոքր ուշ։", "Թերի տվյալներ");
                return;
            }

            UIListView view = null;
            switch (value)
            {
                case DebitEnum.None:
                    break;
                case DebitEnum.Debit:
                    MessageManager.OnMessage("Դեբիտորական պարտքերի ստուգում ...");
                    new Thread(() => { GetDebitAsync(partners, (DateTime)date); }).Start();
                    break;
                case DebitEnum.Credit:
                    //var creditList = partners.Where(s => s.Credit != 0).Select(s => new { Գործընկեր = s.Description, Կրեդիտորական_պարտք = s.Credit }).ToList();
                    // view = new UIListView(creditList, "Կրեդիտորական պարտքի դիտում", (double)creditList.Sum(s => s.Կրեդիտորական_պարտք));
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

        private static void GetDebitAsync(List<PartnerModel> partners, DateTime date)
        {

            var debit = ES.Business.Managers.CashDeskManager.GetDebitByPartners((DateTime)date);
            if (debit == null)
            {
                MessageManager.ShowMessage("Խնդրում ենք փորձել մի փոքր ուշ։", "Թերի տվյալներ");
                return;
            }
            UIListView view = null;
            var debitList = partners.Select(s => new
                {
                    Գործընկեր = s.Description,
                    Դեբիտորական_պարտք = s.Debit,
                    Հաշվարկված = debit.ContainsKey(s.Id) ? debit[s.Id] : 0
                })
                .Where(s => s.Դեբիտորական_պարտք != 0 || s.Հաշվարկված != 0)
                .ToList();

            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send,
                () =>
                {
                    view = new UIListView(debitList, "Դեբիտորական պարտքի դիտում", (double)debitList.Sum(s => s.Դեբիտորական_պարտք));
                    view.Show();
                });
        }

        public static void OpenCashDesk()
        {
            if (string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.CashDeskPort))
            {
                if (!string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveCashDeskPrinter))
                {
                    var ctrl = new UserControl { Width = 100, Height = 1 };
                    ctrl.UpdateLayout();
                    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Normal, () => PrintManager.Print(ctrl, ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveCashDeskPrinter));
                }
                return;
            }
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
