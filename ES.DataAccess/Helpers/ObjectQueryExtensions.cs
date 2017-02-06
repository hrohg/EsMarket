using System;
using System.Data.Entity.Core.Objects;
using System.Linq.Expressions;

namespace ES.DataAccess.Helpers
{
    public static class ObjectQueryExtensions
    {
        public static ObjectQuery<T> Include<T>(this ObjectQuery<T> src, Expression<Func<T, object>> fetch)
        {
            if (fetch.Parameters.Count > 1)
                throw new ArgumentException("CreateFetchingStrategyDescription support only one parameter in a dynamic expression!");
            int dot = fetch.Body.ToString().IndexOf(".", System.StringComparison.Ordinal) + 1;
            string afterFirstDot = fetch.Body.ToString().Remove(0, dot);
            return src.Include(afterFirstDot.Replace(".First()", ""));
        }
    }
}
