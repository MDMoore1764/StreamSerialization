using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSerialization.Stream.Extensions
{
    public static class IEnumerableExtensions
    {

        public static QuickList<T> AsQuickList<T>(this IEnumerable<T> enumerable)
            => new (enumerable);

        public static QuickList<T> AsQuickList<T>(this IAsyncEnumerable<T> enumerable)
        => new(enumerable.ToEnumerable());

    }
}
