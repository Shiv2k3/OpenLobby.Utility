using System;
using System.Text;

namespace OpenLobby.Utility.Utils  
{
    /// <summary>
    /// An array of ByteStrings
    /// </summary>
    public class StringArray
    {
        // first byte # of strings, C, followed by C many bytes stating offset to the start of the next string
        // max C is 255
        // max string length is 255
        private readonly ArraySegment<byte> Stream;
        private readonly ArraySegment<byte> Lengths;
        private readonly ArraySegment<byte> Body;

        /// <summary>
        /// Total number of strings
        /// </summary>
        public ByteMember Count { get; private set; }

        /// <summary>
        /// Index the internal byte stream
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public unsafe string this[int index]
        {
            get
            {
                if (index >= Count.Value)
                    throw new IndexOutOfRangeException();

                int start = Body.Offset;
                int length = Lengths[index];
                for (int i = 0; i < index; i++)
                {
                    start += Lengths[i];
                }
                return Encoding.UTF8.GetString(Stream.Array, start, length);
            }
            private set
            {
                if (index >= Count.Value)
                    throw new IndexOutOfRangeException();
                if (value.Length > 255)
                    throw new ArgumentException("String was too long");

                int start = Body.Offset;
                Lengths[index] = (byte)value.Length;
                for (int i = 0; i < index; i++)
                {
                    start += Lengths[i];
                }

                var b = Encoding.UTF8.GetBytes(value);
                Buffer.BlockCopy(b, 0, Stream.Array, start, value.Length);
            }
        }

        /// <summary>
        /// Constructs the string array into the array segment from the start index
        /// </summary>
        /// <param name="body">The array segment the strings are stored in</param>
        /// <param name="start">The starting index of the stream</param>
        /// <param name="strings">The strings to place in the array segment</param>
        /// <exception cref="ArgumentException">The given string was too long</exception>
        public StringArray(in ArraySegment<byte> body, int start, params string[] strings)
        {
            if (strings.Length > byte.MaxValue)
                throw new ArgumentException("Strings array was too long");

            int c = strings.Length;
            int bodyLength = Helper.GetStringLength(strings);
            int header = 1;
            int length = header + c + bodyLength;

            if (body.Count - start < length)
                throw new ArgumentException("Array segment was not large enough");

            Stream = body.Slice(start, length);
            Count = new ByteMember(body, 0, (byte)c);

            Lengths = Stream.Slice(header, c);
            Body = Stream.Slice(header + c, bodyLength);

            for (int i = 0; i < strings.Length; i++)
            {
                this[i] = strings[i];
            }
        }

        /// <summary>
        /// Reconstructs using the array segment and starting index
        /// </summary>
        /// <param name="body">The array segment with the strings</param>
        /// <param name="start">The starting index of the stream</param>
        public StringArray(in ArraySegment<byte> body, int start)
        {
            int c = body[start];
            int header = 1;

            int indicesStart = start + header;
            int bodyStart = indicesStart + c;
            Lengths = body.Slice(indicesStart, c);

            int bodyLength = 0;
            for (int i = 0; i < c; i++)
            {
                bodyLength += Lengths[i];
            }
            int length = header + c + bodyLength;

            Stream = body.Slice(start, length);
            Count = new ByteMember(Stream, 0, (byte)c);
            Body = body.Slice(bodyStart, bodyLength);
        }

        /// <summary>
        /// Gets the StringArray header that encodes an array of strings
        /// </summary>
        /// <param name="strings">The strings that will be encoded</param>
        /// <returns>The header length</returns>
        public static int GetHeaderSize(params string[] strings) => 1 + strings.Length + Helper.GetStringLength(strings);
    }
}