﻿namespace OpenLobby.Utility.Transmissions
{
    /// <summary>
    /// Transmission type used to reply to requests
    /// </summary>
    public class Reply : Transmission
    {
        /// <summary>
        /// Reply code
        /// </summary>
        public enum Code : byte
        {
            /// <summary>
            /// Lobby creation was a success 
            /// </summary>
            HostingSuccess,
           
            /// <summary>
            /// Lobby creation was unsuccessful
            /// </summary>
            HostingError,
           
            /// <summary>
            /// Error encountered when completing query
            /// </summary>
            QueryError,
        }

        /// <summary>
        /// The received reply code
        /// </summary>
        public Code ReplyCode { get => (Code)Body[0]; set => Body[0] = (byte)value; }

        /// <summary>
        /// Construct using a code
        /// </summary>
        /// <param name="code"></param>
        public Reply(Code code) : base(1, 1) { ReplyCode = code; }
        
        /// <summary>
        /// Construct using transmission
        /// </summary>
        /// <param name="trms"></param>
        public Reply(Transmission trms) : base(trms) { }
    }
}