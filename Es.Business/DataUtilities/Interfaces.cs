using System.Data.Common;

namespace ES.Business.DataUtilities
{
 public interface IConnectionFactory
    {
        DbConnection GetConnection();
        DbProviderFactory GetFactory();
    }
}
