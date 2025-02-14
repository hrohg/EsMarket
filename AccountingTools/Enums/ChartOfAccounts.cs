﻿namespace AccountingTools.Enums
{
    public enum AccountingPlanEnum
    {
        None = 0,
        AllPlans = 1,
        //1 Ոչ ընթացիկ ակտիվներ
        //2 Ընթացիկ ակտիվներ
        //Ապրանքներ
        Purchase = 216,
        //Դեբիտորական պարտքեր վաճառքի գծով
        AccountingReceivable = 221,
        //Տրված ընթացիկ կանխավճարներ
        Prepayments = 224,
        //Դրամարկղ
        CashDesk = 251,
        //Հաշվարկային հաշիվ
        Accounts = 252,
        //3 Սեփական կապիտալ
        //Կանոնադրական կապիտալ
        EquityBase = 311,
        //4 Ոչ ընթացիկ պարտավորություններ
        //5 Ընթացիկ պարտավորություններ
        //Կրեդիտորական պերտքեր գնումների գծով
        PurchasePayables = 521,
        //Ստացված կանխավճարներ
        ReceivedInAdvance = 523,
        //
        DebitForSalary = 527,
        //6 Եկամուտներ
        //Հասույթ
        Proceeds = 611,
        //7 Ծախսեր
        //711 Իրացված արտադրանքի, ապրանքների, աշխատանքների, ծառայությունների ինքնարժեք
        CostPrice = 711,
        //Իրացման ծախսեր
        SalesCosts = 712,
        //Գործառնական այլ ծախսեր
        OtherOperationalExpenses = 714,
        //8 Կառավարչական հաշվառման հաշիվներ
        //9 Արտահաշվեկշռային հաշիվներ

        //Այլ
        BalanceDebetCredit = 221521,
    }
}