using System;
using System.Text;

namespace OpenLobby.Utility.Utils  
{
    /// <summary>
    /// An array of ByteStrings
    /// </summary>
    public class StringArray
    {
        /// <summary>
        /// Segment must have exact required length, use <see cref="GetRequiredLength"/> to get exact length.
        /// </summary>
        public class SegmentOutOfRange : Exception { }
        /// <summary>
        /// Segment was null, to encode/decode the segment must be provided.
        /// </summary>
        public class StringArrayOutOfRange : Exception { }
        /// <summary>
        /// A string was longer than 255 bytes.
        /// </summary>
        public class StringOutOfRange : Exception { }
        /// <summary>
        /// A string was null or empty, every string must have 1 or more characters for safety.
        /// </summary>
        public class BadString : Exception { }

        private readonly ArraySegment<byte> Offsets;
        private readonly ArraySegment<byte> Body;

        /// <summary>
        /// Total number of strings
        /// </summary>
        public byte Count { get => _count.Value; }
        private readonly ByteMember _count;

        /// <summary>
        /// Indexes the array
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The string at index</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public string this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new IndexOutOfRangeException();

                int start = Body.Offset;
                int length = Offsets[index];
                for (int i = 0; i < index; i++)
                {
                    start += Offsets[i];
                }
                return Encoding.UTF8.GetString(Body.Array, start, length);
            }
        }

        /// <summary>
        /// Constructs the string array into the array segment from the start index
        /// </summary>
        /// <param name="segment">The correctly sized byte segment</param>
        /// <param name="strings">The strings to place in the segment</param>
        /// <exception cref="StringArrayOutOfRange"></exception>
        /// <exception cref="StringOutOfRange"></exception>
        /// <exception cref="SegmentOutOfRange"></exception>
        public StringArray(in ArraySegment<byte> segment, params string[] strings)
        {
            if (strings.Length > byte.MaxValue)
                throw new StringArrayOutOfRange();

            byte count = (byte)strings.Length;
            ushort sumOffset = Helper.SumOfStrings(strings);
            ushort length = (ushort)(1 + count + sumOffset);

            if (segment.Count != length)
                throw new SegmentOutOfRange();

            _count = new ByteMember(segment, 0, count);
            Offsets = segment.Slice(1, count);
            Body = segment.Slice(1 + count, sumOffset);

            for (int i = 0, index = 0; i < strings.Length; i++)
            {
                var str = strings[i];
                
                if (str.Length > byte.MaxValue)
                    throw new StringOutOfRange();

                Offsets[i] = (byte)str.Length;
                var bytes = Encoding.UTF8.GetBytes(str);
                Buffer.BlockCopy(bytes, 0, Body.Array, Body.Offset + index, Offsets[i]);
                index += Offsets[i];
            }
        }

        /// <summary>
        /// Reconstructs using the array segment and starting index
        /// </summary>
        /// <param name="segment">The correctly sized byte segment</param>
        /// <exception cref="BadString"></exception>
        public StringArray(in ArraySegment<byte> segment)
        {
            byte count = segment[0];
            ushort totalOffset = 0;
            for (int i = 0; i < count; i++)
            {
                byte offset = segment[1 + i];
                if (offset == 0)
                    throw new BadString();
                totalOffset += offset;
            }
            _count = new ByteMember(segment, 0);
            Offsets = segment.Slice(1, count);
            Body = segment.Slice(1 + count, totalOffset);
        }

        /// <summary>
        /// Gets the StringArray header that encodes an array of strings
        /// </summary>
        /// <param name="strings">The strings that will be encoded</param>
        /// <returns>The header length</returns>
        public static int GetRequiredLength(params string[] strings) => 1 + strings.Length + Helper.SumOfStrings(strings);
    }
}