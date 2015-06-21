namespace GameTheory
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public static class Combinatorics
    {
        public static IEnumerable<ImmutableList<T>> Combinations<T>(IList<T> items, int count)
        {
            if (count <= 0) yield break;
            if (count > items.Count) yield break;

            var indexes = new int[count];
            var prev = 0;
            var depth = 1;

            while (depth >= 0)
            {
                if (depth >= count)
                {
                    yield return indexes.Select(i => items[i]).ToImmutableList();
                    prev = --depth;
                    continue;
                }

                indexes[depth] = indexes[prev] + 1;

                if (indexes[depth] >= items.Count)
                {
                    prev = --depth;
                }
                else
                {
                    prev = depth++;
                }
            }
        }
    }
}
