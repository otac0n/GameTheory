namespace GameTheory
{
    using System;
    using System.Collections.Generic;

    public static class Combinatorics
    {
        public static IEnumerable<T[]> Combinations<T>(IList<T> items, int count)
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
                    yield return Array.ConvertAll(indexes, i => items[i]);
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
