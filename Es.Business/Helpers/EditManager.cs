using ES.Business.Managers;

namespace ES.Business.Helpers
{
    public static class EditManager
    {
        public static bool UpdateBrandsManager(int memberId)
        {
            var brands = ProductsManager.GetAllBrands(true);
            var memberBrands = ProductsManager.GetMemberBrands(memberId);
            memberBrands = SelectItemsManager.SelectBrands(brands, memberBrands, "Ավելացնել հեռացնել բրենդներ");
            if (memberBrands.Count == 0) return false;
            return ProductsManager.EditBrands(memberBrands, memberId);
        }
    }
}
