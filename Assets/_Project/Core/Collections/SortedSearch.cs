using System;
using System.Collections.Generic;

namespace LastSeed.Core.Collections
{
    public static class SortedSearch
    {
        public static int FindLastIndexAtMost<T>(
            IReadOnlyList<T> sortedItems,
            T value,
            IComparer<T> comparer = null)
        {
            if (sortedItems == null)
                throw new ArgumentNullException(nameof(sortedItems));

            comparer ??= Comparer<T>.Default;
            int lower = 0;
            int upper = sortedItems.Count - 1;
            int result = -1;

            for (int remaining = sortedItems.Count;
                 lower <= upper && remaining > 0;
                 remaining--)
            {
                int middle = lower + ((upper - lower) >> 1);

                if (comparer.Compare(sortedItems[middle], value) <= 0)
                {
                    result = middle;
                    lower = middle + 1;
                }
                else
                {
                    upper = middle - 1;
                }
            }

            return result;
        }
    }
}
