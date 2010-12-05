using System;
using System.Collections.Generic;
namespace Concordia.Spimi
{
    public interface IIndex<K, V> where K : IComparable<K>
    {
        bool TryGet(K key, out V value);
    }
}
