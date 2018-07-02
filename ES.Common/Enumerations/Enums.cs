namespace ES.Common.Enumerations
{
    public enum ProjectCreationEnum
    {
        New,
        Edit,
        EditLast,
        View,
        ViewLast
    }

    public enum ViewInvoicesEnum
    {
        None = 0,
        ByDetiles = 1,
        ByStock = 2,
        ByPartnerType = 4,
        ByPartner = 8,
        ByPartnersDetiles = ByDetiles | ByPartner,
        ByStocksDetiles = ByDetiles | ByStock,
        BySaleChart = 16,
        ByZeroAmunt = 32,
    }

    public enum DebitEnum
    {
        None = 0,
        Debit = 0x1,
        Credit = 0x2,
        All = 0xFF
    }
}
