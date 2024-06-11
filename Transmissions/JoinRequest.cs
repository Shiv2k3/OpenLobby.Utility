using OpenLobby.Utility.Utils;

namespace OpenLobby.Utility.Transmissions
{
    /// <summary>
    /// Represents a Join Request
    /// </summary>
    public class JoinRequest : Transmission
    {
        /// <summary>
        /// The ID of the lobby
        /// </summary>
        public ByteString? LobbyID { get; }

        /// <summary>
        /// The Lobby password
        /// </summary>
        public ByteString? LobbyPassword { get; }

        /// <summary>
        /// The IP address and port of the host
        /// </summary>
        public ByteString? HostAddress { get; }

        /// <summary>
        /// Creates the request
        /// </summary>
        /// <param name="lobbyID">Lobby id</param>
        /// <param name="password">Lobby password</param>
        public JoinRequest(string lobbyID, string password) : base((ushort)TransmisisonType.Join, Helper.GetByteStringLength(lobbyID, password))
        {
            LobbyID = new ByteString(lobbyID, Body, 0);
            LobbyPassword = new ByteString(password, Body, LobbyID.StreamLength);
        }

        /// <summary>
        /// Creates the reply with host endpoint info
        /// </summary>
        /// <param name="hostEndpoint">The host endpoint</param>
        public JoinRequest(string hostEndpoint) : base((ushort)TransmisisonType.Join, Helper.GetByteStringLength(hostEndpoint))
        {
            HostAddress = new ByteString(hostEndpoint, Body, 0);
        }

        /// <summary>
        /// Extracts the reply or request
        /// </summary>
        /// <param name="trms"></param>
        /// <param name="isReply">Is this a reply or a request</param>
        public JoinRequest(Transmission trms, bool isReply) : base(trms)
        {
            if (isReply)
            {
                HostAddress = new ByteString(Body, 0);
            }
            else
            {
                LobbyID = new ByteString(Body, 0);
                LobbyPassword = new ByteString(Body, LobbyID.StreamLength);
            }
        }
    }
}