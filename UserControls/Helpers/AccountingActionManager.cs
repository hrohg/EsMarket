using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using AccountingTools.Enums;
using ES.Business.Managers;
using ES.Business.Models;
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
                MessageManager.ShowMessage("Դրամարկղ հայտնաբերված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = cashDesk.Id;
            var partner = SelectItemsManager.SelectPartner();
            if (partner == null)
            {
                MessageManager.ShowMessage("Գործընկեր ընտրված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = partner.Id;
            var ctrlAccountingRecords =
                new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords,
                    "Դեբիտորական պարտքի մարում \n Գործընկեր։ " + partner.FullName + "\n" +
                    "Դեբտորական պարտք։ " + partner.Debit + "\n" +
                    "Կերդիտորական պարտք։ " + partner.Credit));
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
                CashManager.Instance.UpdatePartnersAsync();
                MessageManager.ShowMessage("Վճարումն իրականացվել է հաջողությամբ։");
                if (ApplicationManager.Settings.IsEcrActivated && depositAccountingRecords.Amount > 0)
                {
                    EcrManager.RepaymentOfDebts(depositAccountingRecords.Amount, partner.FullName);
                }
            }
            else
            {
                MessageManager.ShowMessage("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }
        }
        //521
        private static void RepaymentOfDebts()
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.PurchasePayables;
            accountingRecords.Credit = (long)AccountingPlanEnum.CashDesk;
            var partner = SelectItemsManager.SelectPartners(false, "Ընտրել գործընկեր").FirstOrDefault();
            var fromCashDesk = SelectItemsManager.SelectDefaultSaleCashDesks(null, false, "Ընտրել ելքագրվող դրամարկղը").FirstOrDefault();
            if (partner == null || fromCashDesk == null)
            {
                MessageManager.ShowMessage("Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = partner.Id;
            accountingRecords.CreditGuidId = fromCashDesk.Id;

            var description = "Գործընկեր։ " + partner.FullName + "։ Կրեդիտորական պարտք։" + partner.Credit + " դր․" + "\n" +
                "Ելքագրվող դրամարկդ։ " + fromCashDesk.Name + " առկա է։ " + fromCashDesk.Total + " դր․";
            var ui = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords, description));
            ui.ShowDialog();

            var accountingRecord = ui.AccountingRecord;
            if (!ui.Result || accountingRecord == null ||
                accountingRecords.Amount == 0) return;
            if (accountingRecords.Amount > fromCashDesk.Total)
            {
                MessageManager.ShowMessage("Գործողությունն ընդհատված է։ Վճարվել է ավելի շատ քան առկա է։", "Գործողության ընդհատում");
                return;
            }
            if (accountingRecords.Amount > partner.Credit)
            {
                MessageManager.ShowMessage("Գործողությունն ընդհատված է։ Վճարվել է ավելի շատ քան կրեդիտորական պարտքն է։", "Գործողության ընդհատում");
                return;
            }
            if (AccountingRecordsManager.SetRepaymentOfDebts(accountingRecords, ApplicationManager.Instance.GetMember.Id))
            {
                CashManager.Instance.UpdatePartnersAsync();
                MessageManager.ShowMessage("Վճարումն իրականացվել է հաջողությամբ։");
                if (ApplicationManager.Settings.IsEcrActivated)
                {
                    EcrManager.RepaymentOfDebts(accountingRecords.Amount, partner.FullName);
                }
            }
            else
            {
                MessageManager.ShowMessage("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
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
                MessageManager.ShowMessage("Դրամարկղ հայտնաբերված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = cashDesk.Id;
            var partner = SelectItemsManager.SelectPartner();
            if (partner == null)
            {
                MessageManager.ShowMessage("Գործընկեր ընտրված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = partner.Id;
            var ctrlAccountingRecords = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords,
                "Կանխավճար \n" +
                "Գործընկեր։ " + partner.FullName + "\n" +
                 "Դեբիտորական պարտք։ " + (partner.Debit != null ? partner.Debit.ToString() : "0") + "\n" +
                    "Կրեդիտորական պարտք։ " + (partner.Credit != null ? partner.Credit.ToString() : "0")));
            ctrlAccountingRecords.ShowDialog();
            var receivedInAdvance = ctrlAccountingRecords.AccountingRecord;
            receivedInAdvance.CreditGuidId = partner.Id;
            receivedInAdvance.DebitGuidId = cashDesk.Id;
            if (!ctrlAccountingRecords.Result || receivedInAdvance == null || receivedInAdvance.Amount == 0) return;
            if (AccountingRecordsManager.SetPartnerPayment(depositeAccountRecords: receivedInAdvance,
                repaymentAccountingRecords: null))
            {
                CashManager.Instance.UpdatePartnersAsync();
                if (ApplicationManager.Settings.IsEcrActivated)
                {
                    EcrManager.EcrServer.SetCashReceipt(receivedInAdvance.Amount, partner.FullName);
                }
            }
            else { MessageManager.ShowMessage("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։"); }
        }

        private static void OnPrepayments()
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.Prepayments;
            accountingRecords.Credit = (long)AccountingPlanEnum.CashDesk;

            var partner = SelectItemsManager.SelectPartner();
            if (partner == null)
            {
                MessageManager.ShowMessage("Գործընկեր ընտրված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = partner.Id;

            var cashDesk = SelectItemsManager.SelectDefaultSaleCashDesks(null, false, "Ընտրել դրամարկղ").FirstOrDefault();
            if (cashDesk == null)
            {
                MessageManager.ShowMessage("Դրամարկղ հայտնաբերված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = cashDesk.Id;

            var ctrlAccountingRecords = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords,
                "Տրված կանխավճար \n" +
                "Գործընկեր։ " + partner.FullName + "\n" +
                 "Դեբիտորական պարտք։ " + (partner.Debit.ToString(CultureInfo.InvariantCulture)) + "\n" +
                    "Կրեդիտորական պարտք։ " + (partner.Credit.ToString(CultureInfo.InvariantCulture))));
            ctrlAccountingRecords.ShowDialog();
            var accountingRecord = ctrlAccountingRecords.AccountingRecord;
            accountingRecord.DebitGuidId = partner.Id;
            accountingRecord.CreditGuidId = cashDesk.Id;
            if (!ctrlAccountingRecords.Result || accountingRecord.Amount == 0) return;
            if (cashDesk.Total < accountingRecord.Amount)
            {
                MessageManager.ShowMessage("Անբավարար միջոցներ:", "Անբավարար միջոցներ");
                return;
            }

            if (AccountingRecordsManager.SetPartnerPrepayment(accountingRecord))
            {
                ApplicationManager.CashManager.UpdatePartnersAsync();
            }
            else
            {
                MessageManager.ShowMessage("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }

        }

        //
        private static void BalnceDebetCredit()
        {
            var partner = SelectItemsManager.SelectPartner();
            if (partner == null) return;
            if (partner.Credit <= 0 || partner.Debit <= 0)
            {
                MessageBox.Show(string.Format("Պատվիրատու: {0}\nԿանխավճար: {1}\nՊարտք: {2}\n Գործողությունը հնարավոր չէ շարունակել:", partner.FullName, partner.Credit, partner.Debit), "Դեբիտորական և կրեդիտորական պարտքերի մարում", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.PurchasePayables;
            accountingRecords.Credit = (long)AccountingPlanEnum.AccountingReceivable;

            accountingRecords.CreditGuidId = partner.Id;
            accountingRecords.DebitGuidId = partner.Id;
            accountingRecords.Amount = partner.Credit < partner.Debit ? partner.Credit : partner.Debit;

            if (AccountingRecordsManager.BalanceDebetCredit(accountingRecords))
            {
                CashManager.Instance.UpdatePartnersAsync();
                MessageBox.Show(string.Format("Պատվիրատու: {0}\nԿանխավճար: {1}\nՊարտք: {2}\n Վճարումն իրականացվել է հաջողությամբ։", partner.FullName, partner.Credit, partner.Debit), "Դեբիտորական և կրեդիտորական պարտքերի մարում", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageManager.ShowMessage("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
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
                    OnPrepayments();
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
                case AccountingPlanEnum.DebitForSalary:
                    break;
                case AccountingPlanEnum.Proceeds:
                    break;
                case AccountingPlanEnum.CostPrice:
                    break;
                case AccountingPlanEnum.CostOfSales:
                    break;
                case AccountingPlanEnum.OtherOperationalExpenses:
                    break;
                //Other
                case AccountingPlanEnum.BalanceDebetCredit:
                    BalnceDebetCredit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("accountingPlan", accountingPlan, null);
            }
        }
    }
}
