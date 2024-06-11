using OpenLobby.Utility.Utils;
using System;

namespace OpenLobby.Utility.Transmissions
{
    /// <summary>
    /// Repersents a base header-only transmission without any data transmission, inherieting classes should simply wrap over Data
    /// </summary>
    public partial class Transmission
    {        
        /// <summary>
        /// Transmission types
        /// </summary>
        public enum TransmisisonType
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        {
            Host,
            Reply,
            Query,
            Join
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// The number of header bytes, 2b TypeID + 2b Length
        /// </summary>
        public const int HEADERSIZE = 4;

        /// <summary>
        /// Maximum number of transmission bytes allowed
        /// </summary>
        public const int MAXBYTES = ushort.MaxValue - HEADERSIZE;

        /// <summary>
        /// The final payload
        /// </summary>
        private readonly byte[] Stream;

        /// <summary>
        /// Stream's body
        /// </summary>
        protected readonly ArraySegment<byte> Body;

        /// <summary>
        /// Transmission type identifier
        /// </summary>
        public ushort TypeID { get => Helper.GetUshort(0, 1, Stream); protected set => Helper.SetUshort(value, 0, 1, Stream); }

        /// <summary>
        /// Same as TypeID but returns Types enum
        /// </summary>
        public TransmisisonType Type => (TransmisisonType)TypeID;

        /// <summary>
        /// The number of bytes of data
        /// </summary>
        public ushort Length { get => Helper.GetUshort(2, 3, Stream); protected set => Helper.SetUshort(value, 2, 3, Stream); }

        /// <summary>
        /// Create base class members
        /// </summary>
        /// <param name="type">The type of the derived class</param>
        /// <param name="dataLength">Length of data</param>
        protected Transmission(ushort type, ushort dataLength)
        {
            Stream = new byte[dataLength + HEADERSIZE];
            Body = new ArraySegment<byte>(Stream, HEADERSIZE, dataLength);

            TypeID = type;
            Length = dataLength;
        }

        /// <summary>
        /// Initalizates transmission using another transmission, shouldn't be used on the base class
        /// </summary>
        /// <param name="trms">The transmission to use</param>
        protected Transmission(Transmission trms)
        {
            Stream = trms.Stream;
            Body = trms.Body;
        }

        /// <summary>
        /// Use payload header to create instance for payload intel
        /// </summary>
        /// <param name="header"></param>
        public Transmission(byte[] header)
        {
            Stream = header;
        }

        /// <summary>
        /// Create transmission by combining the header and body
        /// </summary>
        /// <param name="header">The head of the payload</param>
        /// <param name="body">The body of the payload</param>
        public Transmission(byte[] header, byte[] body)
        {
            if (header.Length != HEADERSIZE) throw new Exception($"Incorrect header length: {header.Length}");

            Stream = new byte[HEADERSIZE + body.Length];
            for (int i = 0; i < Stream.Length; i++)
            {
                Stream[i] = i < HEADERSIZE ? header[i] : body[i - HEADERSIZE];
            }
            Body = new ArraySegment<byte>(Stream, HEADERSIZE, body.Length);
        }

        /// <summary>
        /// The final payload
        /// </summary>
        public byte[] Payload { get => Stream; }
    }
}