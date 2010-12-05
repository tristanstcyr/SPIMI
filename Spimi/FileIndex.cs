using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics.Contracts;

namespace Concordia.Spimi
{
    /// <summary>
    /// The final merged index can be used through this class.
    /// </summary>
    public class FileIndex<K, V> : IIndex<K, V> where K : IComparable<K>
    {
        private const int PointerByteSize = sizeof(Int64);

        private const int HeaderByteSize = sizeof(Int64);

        Stream stream;
        BinaryReader reader;
        IBinaryObjectEncoder<K> keyDecoder;
        IBinaryObjectEncoder<V> valueDecoder;

        public long EntryCount { get; private set; }

        BinarySearch<K> binarySearch;

        // Where the data starts
        long dataStartPtr;
        // Where the index starts
        long indexStartPtr;

        public FileIndex(
            IBinaryObjectEncoder<K> keyDecoder,
            IBinaryObjectEncoder<V> valueDecoder,
            Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires(keyDecoder != null);
            Contract.Requires(valueDecoder != null);
            Contract.Ensures(Contract.OldValue(stream.Position) == stream.Position);

            this.stream = stream;
            this.reader = new BinaryReader(stream);
            this.keyDecoder = keyDecoder;
            this.valueDecoder = valueDecoder;

            this.indexStartPtr = stream.Position + HeaderByteSize;
            this.dataStartPtr = this.indexStartPtr + PointerByteSize * this.EntryCount;

            this.EntryCount = this.reader.ReadInt64();

            this.binarySearch = new BinarySearch<K>(this.GetKey, this.EntryCount - 1);

            this.restoreStreamPosition();
        }

        private void restoreStreamPosition()
        {
            this.stream.Seek(indexStartPtr - HeaderByteSize, SeekOrigin.Begin);
        }

        public K GetKey(long index)
        {
            Contract.Requires(index < this.EntryCount);
            Contract.Ensures(Contract.Result<K>() != null);
            Contract.Ensures(Contract.OldValue(this.stream.Position) == this.stream.Position);

            MoveStreamToEntryStart(index);
            K key = keyDecoder.read(this.reader);
            this.restoreStreamPosition();

            return key;
        }

        // Searches for the term using binary search
        public bool TryGet(K key, out V value)
        {
            Contract.Ensures(Contract.OldValue(this.stream.Position) == this.stream.Position);

            value = default(V);
            long index = 0;
            if (!binarySearch.TryFind(key, ref index))
                return false;
           
            MoveStreamToEntryStart(index);
           
            // Go past the key
            keyDecoder.read(this.reader);
            // Read the value
            value = valueDecoder.read(reader);

            this.restoreStreamPosition();

            return true;
        }

        public V this[K key]
        {
            get
            {
                V value;
                if (this.TryGet(key, out value))
                    return value;
                throw new IndexOutOfRangeException("File index does not have key " + key);
            }
      
        }

        private void MoveStreamToEntryStart(long index)
        {
            Contract.Requires(index < this.EntryCount);

            // Read the pointer
            Int64 ptrLocation = this.indexStartPtr + index * PointerByteSize;
            stream.Seek(ptrLocation, SeekOrigin.Begin);
            Int64 ptr = reader.ReadInt64();

            // Move to pointed location
            stream.Seek(ptr, SeekOrigin.Begin);
        }
    }
}
