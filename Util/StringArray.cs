using System;

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
        public ByteString this[int index]
        {
            get
            {
                if (index >= Stream[0])
                    throw new IndexOutOfRangeException();

                int stringIndex = 0;
                for (int i = 0; i < index; i++)
                {
                    stringIndex += Lengths[i];
                }
                return new ByteString(Body, stringIndex);
            }
            private set
            {
                if (index >= Stream[0])
                    throw new IndexOutOfRangeException();

                Lengths[index] = value.StreamLength;
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
            int bodyLength = Helper.GetByteStringLength(strings);
            int header = 1;
            int length = header + c + bodyLength;

            Stream = body.Slice(start, length);
            Count = new ByteMember(body, 0, (byte)c);

            Lengths = Stream.Slice(header, c);
            Body = Stream.Slice(c + header, bodyLength);

            start = 0;
            for (int i = 0; i < strings.Length; i++)
            {
                var s = new ByteString(strings[i], Body, start);
                this[i] = s;
                start += s.StreamLength;
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
        public static int GetHeaderSize(params string[] strings) => 1 + strings.Length + Helper.GetByteStringLength(strings);
    }
}