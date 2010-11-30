using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics.Contracts;

namespace Concordia.Spimi
{
    /// <summary>
    /// Creates the FileIndex.
    /// </summary>
    class FileIndexWriter<K, V> : IDisposable where K : IComparable<K> 
    {

        // +-------------------------------------------------------------------+
        // | 1. (8) Term count | 2. (8) Data ptr | .... | 3. Key , Value | ... |
        // +-------------------------------------------------------------------+

        IBinaryObjectEncoder<K> keyEncoder;
        IBinaryObjectEncoder<V> valueEncoder;

        Stream index;
        Stream data;

        BinaryWriter indexWriter;
        BinaryWriter dataWriter;

        long keyCount = 0;

        K previousKey;

        string dataFilePath;

        long startPosition;

        long indexPtr;

        bool isClean = true;

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(index != null);
            Contract.Invariant(data != null);
            Contract.Invariant(indexWriter != null);
            Contract.Invariant(dataWriter != null);
            Contract.Invariant(dataFilePath != null);
        }

        [Pure]
        public bool IsOpen
        {
            get
            {
                return index.CanWrite && data.CanWrite;
            }
        }

        public FileIndexWriter(
            IBinaryObjectEncoder<K> keyEncoder, 
            IBinaryObjectEncoder<V> valueEncoder,
            Stream index)
        {
            Contract.Requires(index != null && index.CanWrite);
            Contract.Requires(keyEncoder != null);
            Contract.Requires(valueEncoder != null);
            Contract.Ensures(this.IsOpen);
            Contract.Ensures(Contract.OldValue(index.Position) == index.Position);

            this.keyEncoder = keyEncoder;
            this.valueEncoder = valueEncoder;
            this.startPosition = index.Position;
            this.index = index;
            this.dataFilePath = Path.GetTempFileName();
            this.data = File.Open(dataFilePath, FileMode.Open);
            
            indexWriter = new BinaryWriter(index);
            dataWriter = new BinaryWriter(data);

            indexPtr = startPosition + sizeof(Int64);
        }

        // Finalizes the index and moves it to filepath
        public void WriteOut()
        {
            Contract.Requires(this.IsOpen);
            Contract.Ensures(this.index.Position == this.startPosition);

            // Write the key count at the beginning of the index stream
            index.Seek(startPosition, SeekOrigin.Begin);
            indexWriter.Write((Int64)keyCount);

            // Offset all pointers
            BinaryReader indexReader = new BinaryReader(index);
            long offset = index.Position + keyCount * sizeof(Int64);
            for (int i = 0; i < keyCount; i++)
            {
                long ptr = indexReader.ReadInt64() + offset;
                index.Seek(-sizeof(Int64), SeekOrigin.Current);
                indexWriter.Write((Int64)ptr);
            }

            // Copy the postingLists buffer to the termEntries
            data.Seek(0, SeekOrigin.Begin);
            data.CopyTo(index);

            index.Seek(this.startPosition, SeekOrigin.Begin);

            this.Dispose();
        }

        public void Add(K key, V value)
        {
            Contract.Requires(this.IsOpen);
            Contract.Requires(isClean || previousKey.CompareTo(key) < 0, 
                "Keys must be provided in ascending order");
            Contract.Requires(key != null);
            Contract.Ensures(Contract.OldValue(this.index.Position) == this.index.Position,
                "Stream position is restored");
            Contract.Ensures(!isClean);
            Contract.Ensures(Contract.OldValue(this.keyCount) + 1 == this.keyCount);
            Contract.Ensures(this.previousKey.Equals(key));

            // 2. Write the pointer to the data in the index
            long indexStreamPosition = this.index.Position;
            this.index.Seek(indexPtr, SeekOrigin.Begin);
            this.indexWriter.Write(data.Position);
            indexPtr = this.index.Position;

            // 3. Write out the value
            this.keyEncoder.write(dataWriter, key);
            this.valueEncoder.write(dataWriter, value);
            
            this.keyCount++;
            this.previousKey = key;
            isClean = false;
            this.index.Seek(indexStreamPosition, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            this.data.Close();
            if (File.Exists(this.dataFilePath))
                File.Delete(this.dataFilePath);
        }
    }
}
