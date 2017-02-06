using System.Data.Entity;

namespace ES.DataAccess.Models
{
    public partial class EsStockDBEntities : DbContext
    {
        public EsStockDBEntities(string connectionString)
            : base(connectionString)
        {
            var ensureDLLIsCopied = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
    }
    public partial class EsStockDbServerEntities : DbContext
    {
        public EsStockDbServerEntities(string connectionString)
            : base(connectionString)
        {
            var ensureDLLIsCopied = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
    }
}
