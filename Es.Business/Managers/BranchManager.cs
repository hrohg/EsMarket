using System;
using System.Collections.Generic;
using ES.Data.Models
    ;
namespace ES.Business.Managers
{
    public class BranchManager
    {
        public static BranchModel GetBranch(Guid id)
        {
            return new BranchModel(ApplicationManager.Member.Id);
        }

        public static List<BranchModel> GetBranches()
        {
            return new List<BranchModel>();
        }
    }
}
