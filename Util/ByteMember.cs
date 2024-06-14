using System;

namespace OpenLobby.Utility.Utils
{
    /// <summary>
    /// Wraps the serialization and deserialization of a byte into an arr
    /// </summary>
    public unsafe class ByteMember
    {
        /// <summary>
        /// Get the byte value
        /// </summary>
        public byte Value { get => ((byte*)ptr)[0]; }

        /// <summary>
        /// True if byte is greater than 0
        /// </summary>
        public bool AsBool { get => Value > 0; }

        private IntPtr ptr;

        /// <summary>
        /// Creates using the byte at index of segment
        /// </summary>
        /// <param name="segment">The segment being indexed</param>
        /// <param name="index">The index of the byte</param>
        /// <param name="value">The value to initalize</param>
        public ByteMember(in ArraySegment<byte> segment, in int index, byte value)
        {
            InitPtr(segment, index);
            segment[index] = value;
        }

        /// <summary>
        /// Constructs using the byte at index
        /// </summary>
        /// <param name="segment">The segment being indexed</param>
        /// <param name="index">The index of the byte</param>
        public ByteMember(in ArraySegment<byte> segment, in int index)
        {
            InitPtr(segment, index);
        }

        private unsafe void InitPtr(in ArraySegment<byte> segment, in int index)
        {
            fixed (void* p = segment.Array)
            {
                ptr = new IntPtr(p) + segment.Offset + index;
            }

        }
    }
}