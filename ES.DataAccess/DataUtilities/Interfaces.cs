using System.Data.Common;

namespace ES.DataAccess.DataUtilities
{
 public interface IConnectionFactory
    {
        DbConnection GetConnection();
        DbProviderFactory GetFactory();
    }
}
