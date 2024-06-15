using System;

namespace OpenLobby.Utility.Network
{
    /// <summary>
    /// The address was already in use
    /// </summary>
    public class AddressInUse : Exception
    {
        /// <summary>
        /// The constructor
        /// </summary>
        public AddressInUse() : base("The address was already in use, the socket has been disposed") { }
    }
}