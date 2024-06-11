using System;

namespace OpenLobby.Utility.Utils
{
    /// <summary>
    /// Wraps the serialization and deserialization of a byte into an arr
    /// </summary>
    public class ByteMember
    {
        /// <summary>
        /// Get the byte value
        /// </summary>
        public byte Value { get => value; }
        
        /// <summary>
        /// True if byte is greater than 0
        /// </summary>
        public bool AsBool { get => value > 0; }

        private readonly byte value;

        /// <summary>
        /// Constructs from the body at index set to value
        /// </summary>
        /// <param name="body"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public ByteMember(in ArraySegment<byte> body, int index, byte value)
        {
            body[index] = value;
            this.value = value;
        }

        /// <summary>
        /// Constructs using the byte at index
        /// </summary>
        /// <param name="body"></param>
        /// <param name="index"></param>
        public ByteMember(in ArraySegment<byte> body, int index) => value = body[index];
    }
}