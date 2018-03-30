using System;
using System.Linq;
using System.Windows;
using CashReg;
using CashReg.Managers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Enumerations;
using ES.Common.Managers;
using UserControls.ControlPanel.Controls;
using UserControls.ViewModels;

namespace UserControls.Helpers
{
    public class AccountingActionManager
    {
        //221
        private static void RepaymentOfReceivable()
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Member.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.CashDesk;
            accountingRecords.Credit = (long)AccountingPlanEnum.AccountingReceivable;
            var cashDesk = SelectItemsManager.SelectDefaultSaleCashDesks(null, false, "Ընտրել դրամարկղ").FirstOrDefault();
            if (cashDesk == null)
            {
                MessageBox.Show("Դրամարկղ հայտնաբերված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = cashDesk.Id;
            var partner = SelectItemsManager.SelectPartner();
            if (partner == null)
            {
                MessageBox.Show("Գործընկեր ընտրված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = partner.Id;
            var ctrlAccountingRecords =
                new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords,
                    "Դեբիտորական պարտքի մարում \n Գործընկեր։ " + partner.FullName + "\n" +
                    "Դեբտորական պարտք։ " + (partner.Debit != null ? partner.Debit.ToString() : "0") + "\n" +
                    "Կերդիտորական պարտք։ " + (partner.Credit != null ? partner.Credit.ToString() : "0")));
            ctrlAccountingRecords.ShowDialog();
            var repaymentAccountingRecord = ctrlAccountingRecords.AccountingRecord;
            repaymentAccountingRecord.DebitGuidId = cashDesk.Id;
            repaymentAccountingRecord.CreditGuidId = partner.Id;
            if (!ctrlAccountingRecords.Result || repaymentAccountingRecord == null ||
                repaymentAccountingRecord.Amount == 0) return;
            var depositAccountingRecords = new AccountingRecordsModel(date: repaymentAccountingRecord.RegisterDate,
                memberId: repaymentAccountingRecord.MemberId, registerId: repaymentAccountingRecord.RegisterId)
            {
                Amount = 0,
                Description = repaymentAccountingRecord.Description,
                Debit = (long)AccountingPlanEnum.CashDesk,
                Credit = (long)AccountingPlanEnum.ReceivedInAdvance,
                DebitGuidId = repaymentAccountingRecord.DebitGuidId,
                CreditGuidId = repaymentAccountingRecord.CreditGuidId,
            };

            if (repaymentAccountingRecord.Amount > partner.Debit)
            {
                if (MessageBox.Show(
                    "Վճարվել է " + (repaymentAccountingRecord.Amount - partner.Debit) + " դրամ ավել։ \n" +
                    "Ավելացնել " + (repaymentAccountingRecord.Amount - partner.Debit) + " դրամ որպես կանխավճար։",
                    "Գերավճար", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                depositAccountingRecords.Amount = repaymentAccountingRecord.Amount - partner.Debit;
            }
            repaymentAccountingRecord.Amount -= depositAccountingRecords.Amount;
            if (AccountingRecordsManager.SetPartnerPayment(depositeAccountRecords: depositAccountingRecords,
                repaymentAccountingRecords: repaymentAccountingRecord))
            {
                MessageBox.Show("Վճարումն իրականացվել է հաջողությամբ։");
                if (ApplicationManager.Settings.IsEcrActivated && depositAccountingRecords.Amount>0)
                {
                    new EcrManager().RepaymentOfDebts(depositAccountingRecords.Amount, partner.FullName);
                }
            }
            else
            {
                MessageBox.Show("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }
        }
        //521
        private static void RepaymentOfDebts()
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.PurchasePayables;
            accountingRecords.Credit = (long)AccountingPlanEnum.CashDesk;
            var partner = SelectItemsManager.SelectPartners(false, "Ընտրել գործընկեր դրամարկղը").FirstOrDefault();
            var fromCashDesk = SelectItemsManager.SelectDefaultSaleCashDesks(null, false, "Ընտրել ելքագրվող դրամարկղը").FirstOrDefault();
            if (partner == null || fromCashDesk == null)
            {
                MessageBox.Show("Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = fromCashDesk.Id;
            accountingRecords.DebitGuidId = partner.Id;
            var description = "Գործընկեր։ " + partner.FullName + "։ Կրեդիտորական պարտք։" + partner.Credit + " դր․" + "\n" +
                "Ելքագրվող դրամարկդ։ " + fromCashDesk.Name + " առկա է։ " + fromCashDesk.Total + " դր․";
            var ui = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords, description));
            ui.ShowDialog();

            var accountingRecord = ui.AccountingRecord;
            if (!ui.Result || accountingRecord == null ||
                accountingRecords.Amount == 0) return;
            if (accountingRecords.Amount > fromCashDesk.Total)
            {
                MessageBox.Show("Գործողությունն ընդհատված է։ Վճարվել է ավելի շատ քան առկա է։", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (accountingRecords.Amount > partner.Credit)
            {
                MessageBox.Show("Գործողությունն ընդհատված է։ Վճարվել է ավելի շատ քան կրեդիտորական պարտքն է։", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (AccountingRecordsManager.SetRepaymentOfDebts(accountingRecords, ApplicationManager.Instance.GetMember.Id))
            {
                MessageBox.Show("Վճարումն իրականացվել է հաջողությամբ։");
                if (ApplicationManager.Settings.IsEcrActivated)
                {
                    new EcrManager().RepaymentOfDebts(accountingRecords.Amount, partner.FullName);
                }
            }
            else
            {
                MessageBox.Show("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }
        }
        //523
        private static void ReceivedInAdvance()
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.CashDesk;
            accountingRecords.Credit = (long)AccountingPlanEnum.ReceivedInAdvance;
            var cashDesk = SelectItemsManager.SelectDefaultSaleCashDesks(null, false, "Ընտրել դրամարկղ").FirstOrDefault();
            if (cashDesk == null)
            {
                MessageBox.Show("Դրամարկղ հայտնաբերված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = cashDesk.Id;
            var partner = SelectItemsManager.SelectPartner();
            if (partner == null)
            {
                MessageBox.Show("Գործընկեր ընտրված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = partner.Id;
            var ctrlAccountingRecords = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords,
                "Կանխավճար \n" +
                "Գործընկեր։ " + partner.FullName + "\n" +
                 "Դեբտորական պարտք։ " + (partner.Debit != null ? partner.Debit.ToString() : "0") + "\n" +
                    "Կերդիտորական պարտք։ " + (partner.Credit != null ? partner.Credit.ToString() : "0")));
            ctrlAccountingRecords.ShowDialog();
            var receivedInAdvance = ctrlAccountingRecords.AccountingRecord;
            receivedInAdvance.CreditGuidId = partner.Id;
            receivedInAdvance.DebitGuidId = cashDesk.Id;
            if (!ctrlAccountingRecords.Result || receivedInAdvance == null || receivedInAdvance.Amount == 0) return;
            AccountingRecordsManager.SetPartnerPayment(depositeAccountRecords: receivedInAdvance, repaymentAccountingRecords: null);

            if (ApplicationManager.Settings.IsEcrActivated)
            {
                ApplicationManager.CreateEcrConnection().SetCashReceipt(receivedInAdvance.Amount, partner.FullName);
            }
        }

        public static void Action(AccountingPlanEnum accountingPlan)
        {
            switch (accountingPlan)
            {
                case AccountingPlanEnum.None:
                    break;
                case AccountingPlanEnum.Purchase:
                    break;
                case AccountingPlanEnum.AccountingReceivable:
                    RepaymentOfReceivable();
                    break;
                case AccountingPlanEnum.Prepayments:
                    break;
                case AccountingPlanEnum.CashDesk:
                    break;
                case AccountingPlanEnum.Accounts:
                    break;
                case AccountingPlanEnum.EquityBase:
                    break;
                case AccountingPlanEnum.PurchasePayables:
                    RepaymentOfDebts();
                    break;
                case AccountingPlanEnum.ReceivedInAdvance:
                    ReceivedInAdvance();
                    break;
                case AccountingPlanEnum.Debit_For_Salary:
                    break;
                case AccountingPlanEnum.Proceeds:
                    break;
                case AccountingPlanEnum.CostPrice:
                    break;
                case AccountingPlanEnum.CostOfSales:
                    break;
                case AccountingPlanEnum.OtherOperationalExpenses:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("accountingPlan", accountingPlan, null);
            }
        }
    }
}
