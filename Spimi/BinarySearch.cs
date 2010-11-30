using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordia.Spimi
{
    class BinarySearch<K> where K : IComparable<K>
    {
        Func<long, K> valueProvider;
        long max;

        public BinarySearch(Func<long, K> valueProvider, long max)
        {
            this.valueProvider = valueProvider;
            this.max = max;
        }

        public bool TryFind(K value, ref long index)
        {
            long minEntryIndex = 0;
            long maxEntryIndex = this.max;

            while (minEntryIndex <= maxEntryIndex)
            {
                long midEntryIndex = minEntryIndex + ((maxEntryIndex - minEntryIndex) / 2);

                K found = this.valueProvider(midEntryIndex);
                int comparison = value.CompareTo(found);

                if (comparison == 0)
                {
                    index = midEntryIndex;
                    return true;
                }
                else if (comparison > 0)
                {
                    minEntryIndex = midEntryIndex + 1;
                }
                else // if (comparison < 0)
                {
                    maxEntryIndex = midEntryIndex - 1;
                }
            }
            return false;
        }
    }
}
