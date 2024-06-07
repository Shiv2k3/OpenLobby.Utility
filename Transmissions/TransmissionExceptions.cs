using System;
namespace OpenLobby.Utility.Transmissions
{
    public class UnknownTransmission : Exception
    {
        public UnknownTransmission(string? message) : base(message) { }
    }
}