namespace GameTheory
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, params T[] second)
        {
            return source.Except(second.AsEnumerable());
        }

        public static IEnumerable<T> Intersect<T>(this IEnumerable<T> source, params T[] second)
        {
            return source.Intersect(second.AsEnumerable());
        }

        public static IEnumerable<IReadOnlyList<T>> Partition<T>(this IEnumerable<T> source, int count)
        {
            Contract.Requires(count > 0);

            var result = new List<T>(count);
            foreach (var item in source)
            {
                result.Add(item);
                if (result.Count >= count)
                {
                    yield return result.ToImmutableList();
                    result.Clear();
                }
            }

            if (result.Count > 0)
            {
                yield return result.ToImmutableList();
            }
        }

        public static IEnumerable<T> Times<T>(this T value, int count)
        {
            while (count-- > 0) yield return value;
        }
    }
}
