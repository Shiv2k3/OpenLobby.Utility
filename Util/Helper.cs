using System;

namespace OpenLobby.Utility.Utils
{
    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Sets the bytes from ushort into arr at index i1 and i2
        /// </summary>
        public static void SetUshort(in ushort value, in int i1, in int i2, in ArraySegment<byte> arr)
        {
            unchecked
            {
                arr[i1] = (byte)value;
                arr[i2] = (byte)(value >> 8);
            }
        }

        /// <summary>
        /// Gets ushort from arr by using bytes at index i1 and i2
        /// </summary>
        /// <returns></returns>
        public static ushort GetUshort(int i1, int i2, ArraySegment<byte> arr)
        {
            return (ushort)(arr[i2] << 8 | arr[i1]);
        }

        /// <summary>
        /// Counts Length of the ByteStrings
        /// </summary>
        /// <param name="strings">The strings to count</param>
        /// <returns>Length if Length is less than <seealso cref="ushort.MaxValue"/> </returns>
        /// <exception cref="ArgumentOutOfRangeException">The total Length of the strings were too long</exception>
        public static ushort SumOfByteStrings(params string[] strings)
        {
            int count = strings.Length * ByteString.HEADERSIZE;
            foreach (var str in strings)
            {
                count += str.Length;
            }
            return (count <= ushort.MaxValue) ? (ushort)count : throw new ArgumentOutOfRangeException("Strings were too long");
        }

        /// <summary>
        /// Sums the length of the strings
        /// </summary>
        /// <param name="strings">The strings to count</param>
        /// <returns>Sum if it is less than <seealso cref="ushort.MaxValue"/> </returns>
        /// <exception cref="ArgumentOutOfRangeException">The total Length of the strings were too long</exception>
        /// <exception cref="NullReferenceException">Strings or it's element was null</exception>
        public static ushort SumOfStrings(params string[] strings)
        {
            int count = 0;
            foreach (var str in strings)
            {
                count += str.Length;
            }
            return (count <= ushort.MaxValue) ? (ushort)count : throw new ArgumentOutOfRangeException("Strings were too long");
        }
    }
}