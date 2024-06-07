using System;

namespace OpenLobby.Utility.Transmissions
{
    /// <summary>
    /// Unknown transmission type exception
    /// </summary>
    public class UnknownTransmission : Exception
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public UnknownTransmission() : base() { }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}