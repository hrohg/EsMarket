using System;
using System.Linq;
using System.Threading;
using System.Windows;
using CashReg;
using CashReg.Helper;
using CashReg.Interfaces;
using ES.Business.Managers;
using ES.Common.Managers;
using ES.Data.Models;
using UserControls.ViewModels.Invoices;
using MessageBox = System.Windows.MessageBox;

namespace UserControls.Helpers
{
    public class EcrManager
    {
        #region Private properties
        private static EcrServer _ecrServer;
        //private string _ecrServerIp = "217.113.7.68";
        //private int _ecrServerPort = 9999;
        //private string _ecrPass = "HS0FBZZZ";
        //private string _cashierCrn;
        //private int _cashierId = 3;
        //private string _cashierPas = "3";
        
        #endregion

        public static EcrServer EcrServer
        {
            get
            {
                if (_ecrServer == null)
                {
                    _ecrServer = new EcrServer(ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig);
                    _ecrServer.OnErrorEvent += delegate(Exception ex) { MessageManager.OnMessage(ex.ToString()); };
                }
                return _ecrServer;
            }
        }
        
        #region Public Methods
        public bool ConnectionCheck()
        {
            
            if (_ecrServer.TryConnection())
            {
                MessageManager.ShowMessage(
                    "Հսկիչ դրամարկղային մեքենայի կապի ստուգումն իրականացվել է հաջողությամբ։ \n",
                    "Տեղեկացում", MessageBoxImage.Information);
                return true;
            }
            else
            {
                MessageBox.Show(
                    "Հսկիչ դրամարկղային մեքենայի կապի ստուգումը ձախողվել է։ \n",
                    "Տեղեկացում",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
            }
        }
        public bool OperatorLogin()
        {
            if (_ecrServer.TryOperatorLogin())
            {
                MessageBox.Show(
                    "Հսկիչ դրամարկղային մեքենայի օպերատորի մուտքի ստուգումն իրականացվել է հաջողությամբ։ \n",
                    "Տեղեկացում",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            else
            {
                MessageManager.ShowMessage(
                    "Հսկիչ դրամարկղային մեքենայի օպերատորի մուտքի ստուգումը ձախողվել է։ Հնարավոր է կապի խափանում։ \nՍտուգեք կապը և փորձեք ևս մեկ անգամ։ Կրկնվելու դեպքում դիմել ցանցային ադմինիստրատորին կամ ՀԴՄ սպասարկողին։ \n",
                    "Տեղեկացում",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
            }
        }
        public bool PrintReceiptFromExcel(EsUserModel user, EsMemberModel member, long memberId)
        {
            var model = new SaleInvoiceViewModel();
            if (!model.PrintReceiptFromExcel())
            {
                MessageManager.ShowMessage("Գործողությունն ընդհատված է։ \nՏվյալները կարդալու ժամանակ տեղի է ունեցել սխալ։ Տվյալները թերի են կամ ոչ Լիարժեք։", "Գործողության ընդհատում", MessageBoxButton.OK);
                return false;
            }
            if (MessageManager.ShowMessage("Տվյալները կարդացվել են հաջողությամբ։ \n Դուք ցանկանու՞մ եք տպել " +
                    (Math.Round((model.Invoice.Total + 4) / 10) * 10).ToString("0,###") + " դրամի հսկիչ դրամարկղային կտրոն", "Հարցում",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                //Print ECR Receipt
                var pr = _ecrServer.PrintReceipt(model.InvoiceItems.Select(s =>(IEcrProduct) new EcrProduct(s.Code,s.Description, s.Mu )
                    {
                        qty = s.Quantity??0,
                        price = s.Price??0,
                        //Dep = 1
                    }).ToList(), new EcrPaid { PaidAmount = (int)(Math.Round((model.Invoice.Total + 5) / 10) * 10) }, null);
                if (_ecrServer.ActionCode == EcrException.ResponseCodes.Ok)
                {
                    MessageManager.ShowMessage("Հսկիչ դրամարկղային կտրոնի տպումն իրականացվել է հաջողությամբ։ \n Խնդրում ենք վերցնել հսկիչ դրամարկղային կտրոնը։", "Տեղեկացում",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                else
                {
                    if (MessageManager.ShowMessage(
                        "Հսկիչ դրամարկղային կտրոնի տպումն ընդհատվել է։ Հնարավոր է կապի խափանում։ \nՍտուգեք կապը և փորձեք տպել ևս մեկ անգամ։ Կրկնվելու դեպքում դիմել ցանցային ադմինիստրատորին կամ ՀԴՄ սպասարկողին։ \nՏպե՞լ ևս մի անգամ։",
                        "Տեղեկացում",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {return false;}
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool PrintReceiptFromExcel(EsUserModel user, EsMemberModel member)
        {
            var model = new SaleInvoiceViewModel();
            if (!model.PrintReceiptFromExcel())
            {
                MessageManager.ShowMessage("Գործողությունն ընդհատված է։ \nՏվյալները կարդալու ժամանակ տեղի է ունեցել սխալ։ Տվյալները թերի են կամ ոչ Լիարժեք։", "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (MessageManager.ShowMessage("Տվյալները կարդացվել են հաջողությամբ։ \n Դուք ցանկանու՞մ եք տպել " +
                    (Math.Round((model.Invoice.Total + 4) / 10) * 10).ToString("0,###") + " դրամի հսկիչ դրամարկղային կտրոն", "Հարցում",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                //Print ECR Receipt
                var pr = _ecrServer.PrintReceipt(model.InvoiceItems.Select(s =>(IEcrProduct) new EcrProduct(s.Code, s.Description, s.Mu)
                    {
                        qty = s.Quantity??0,
                        price = s.Price??0,
                        //Dep = 1
                    }).ToList(), (IEcrPaid)new EcrPaid { PaidAmount = (int)(Math.Round((model.Invoice.Total + 5) / 10) * 10) }, null);
                if (_ecrServer.ActionCode == EcrException.ResponseCodes.Ok)
                {
                    MessageManager.ShowMessage("Հսկիչ դրամարկղային կտրոնի տպումն իրականացվել է հաջողությամբ։ \n Խնդրում ենք վերցնել հսկիչ դրամարկղային կտրոնը։", "Տեղեկացում",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                else
                {
                    if (MessageManager.ShowMessage(
                        "Հսկիչ դրամարկղային կտրոնի տպումն ընդհատվել է։ Հնարավոր է կապի խափանում։ \nՍտուգեք կապը և փորձեք տպել ևս մեկ անգամ։ Կրկնվելու դեպքում դիմել ցանցային ադմինիստրատորին կամ ՀԴՄ սպասարկողին։ \nՏպե՞լ ևս մի անգամ։",
                        "Տեղեկացում",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    { return false; }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private bool PrintReceiptLatestCopy(object sender, EventArgs e)
        {
           //Print ECR Receiptr
            if (_ecrServer.PrintReceiptLatestCopy())
            {
                MessageManager.ShowMessage(
                    "Հսկիչ դրամարկղային կտրոնի կրկնօրինակի տպումն իրականացվել է հաջողությամբ։ \nԽնդրում ենք վերցնել հսկիչ դրամարկղային կտրոնը։",
                    "Տեղեկացում",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            else
            {
                if (MessageManager.ShowMessage(
                    "Հսկիչ դրամարկղային կտրոնի կրկնօրինակի տպումն ընդհատվել է։ Հնարավոր է կապի խափանում։ \nՍտուգեք կապը և փորձեք տպել ևս մեկ անգամ։ Կրկնվելու դեպքում դիմել ցանցային ադմինիստրատորին կամ ՀԴՄ սպասարկողին։ \nՏպե՞լ ևս մեկ անգամ։",
                    "Տեղեկացում",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {return false;}
                return false;
            }
        }
        private bool PrintReturnTicket()
        {
            while (true)
            {
                ////Print ECR Receipt
                //var returnTicket = new InputBox("Մուտքագրել ՀԴ կտրոնի համարը");
                //returnTicket.ShowDialog();
                //if (returnTicket.DialogResult!= DialogResult.OK || string.IsNullOrEmpty(returnTicket.InputValue))
                //{
                //    MessageManager.ShowMessage("Հսկիչ դրամարկղային վերադարձի կտրոնի տպումն ընդհատվել է օգտագործողի կողմից։", "Տեղեկացում",
                //        MessageBoxButton.OK, MessageBoxImage.Information);
                //    return false;
                //}
                //if (_ecrServer.GetReturnReceipt( HgConvert.ToInt32(returnTicket.InputValue)))
                //{
                //    MessageManager.ShowMessage(
                //        "Հսկիչ դրամարկղային վերադարձի կտրոնի տպումն իրականացվել է հաջողությամբ։ \nԽնդրում ենք վերցնել հսկիչ դրամարկղային կտրոնը։",
                //        "Տեղեկացում",
                //        MessageBoxButton.OK, MessageBoxImage.Information);
                //    return true;
                //}
                //else
                //{
                //    if (MessageManager.ShowMessage(
                //        "Հսկիչ դրամարկղային վերադարձի կտրոնի տպումն ընդհատվել է։ Հնարավոր է կապի խափանում։ \nՍտուգեք կապը և փորձեք տպել ևս մեկ անգամ։ Կրկնվելու դեպքում դիմել ցանցային ադմինիստրատորին կամ ՀԴՄ սպասարկողին։ \nՏպե՞լ ևս մեկ անգամ։",
                //        "Տեղեկացում",
                //        MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                //    { return false; }
                //}
            }

        }
        #endregion


        public void PrintReceipt()
        {
            
        }
        public void ReceivedInAdvance(decimal amount, string fullName)
        {
            new Thread(() => EcrServer.SetCashReceipt(amount, fullName)).Start();
        }

        public static void RepaymentOfDebts(decimal amount, string fullName)
        {
            new Thread(() => EcrServer.SetCashWithdrawal(amount, fullName)).Start();
        }
    }
}
