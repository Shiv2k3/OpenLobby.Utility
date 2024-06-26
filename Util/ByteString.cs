﻿using System;
using System.Text;

namespace OpenLobby.Utility.Utils
{
    /// <summary>
    /// String encoded into an array
    /// </summary>
    public class ByteString
    {
        /// <summary>
        /// 1B
        /// </summary>
        public const int HEADERSIZE = 1;

        /// <summary>
        /// The string in the stream;
        /// </summary>
        public string Value { get => Encoding.ASCII.GetString(Body); set => Encoding.ASCII.GetBytes(value, Body.AsSpan()); }
      
        /// <summary>
        /// The length of the stream
        /// </summary>
        public byte StreamLength { get => Stream[0]; private set => Stream[0] = value; }

        private readonly ArraySegment<byte> Stream;
        private readonly ArraySegment<byte> Body;

        /// <summary>
        /// Encodes a string into an array from an index
        /// </summary>
        /// <param name="value">The string to encode</param>
        /// <param name="arr">The backstore</param>
        /// <param name="start">The starting index at the backstore</param>
        /// <exception cref="ArgumentException">Length of arr or value was invalid</exception>
        public ByteString(string value, ArraySegment<byte> arr, int start)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value was null or empty");
            if (value.Length > byte.MaxValue)
                throw new ArgumentException("String length must not overflow a byte");
            if (arr.Count - start - value.Length - HEADERSIZE < 0)
                throw new ArgumentOutOfRangeException("The array is not big enough for the encoding, array length must account for HEADERSIZE");

            Stream = arr.Slice(start, HEADERSIZE + value.Length);
            Body = Stream.Slice(HEADERSIZE, value.Length);

            StreamLength = (byte)(value.Length + HEADERSIZE);
            Value = value;
        }

        /// <summary>
        /// Reconstructs string using a backstore
        /// </summary>
        /// <param name="arr">The backstore to use for decoding the string</param>
        /// <param name="start">The starting index of the encoding in arr</param>
        public ByteString(ArraySegment<byte> arr, int start)
        {
            Stream = arr.Slice(start, arr[start]);
            Body = Stream.Slice(HEADERSIZE, StreamLength - HEADERSIZE);
        }
    }
}
