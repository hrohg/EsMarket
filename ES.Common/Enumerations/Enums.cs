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
        None,
        ByDetiles,
        ByStock,
        ByPartnerType,
        ByPartner,
    }

    public enum DebitEnum
    {
        None = 0,
        Debit = 0x1,
        Credit = 0x2,
        All = 0xFF
    }
}
