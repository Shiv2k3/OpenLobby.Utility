using OpenLobby.Utility.Utils;

namespace OpenLobby.Utility.Transmissions
{
    /// <summary>
    /// Repersents a request to query for lobbies
    /// </summary>
    public class LobbyQuery : Transmission
    {
        /// <summary>
        /// The search parameter
        /// </summary>
        public ByteString? Search;

        /// <summary>
        /// The query result
        /// </summary>
        public StringArray? Lobbies;

        /// <summary>
        /// Creates query, client-side
        /// </summary>
        /// <param name="search">Lobby name</param>
        public LobbyQuery(string search) : base(2, (ushort)StringArray.GetHeaderSize(search))
        {
            Search = new ByteString(search, Body, 0);
        }

        /// <summary>
        /// Construct using lobbies
        /// </summary>
        /// <param name="lobbies"></param>
        public LobbyQuery(params string[] lobbies) : base(2, (ushort)StringArray.GetHeaderSize(lobbies))
        {
            Lobbies = new StringArray(Body, 0, lobbies);
        }
        /// <summary>
        /// Creates query reply, server-side
        /// </summary>
        /// <param name="trms">The transmission to use for reconstruction</param>
        /// <param name="isReply">Is this a reply or request</param>
        public LobbyQuery(Transmission trms, bool isReply) : base(trms)
        {
            if (isReply)
                Lobbies = new StringArray(Body, 0);
            else
                Search = new ByteString(Body, 0);
        }
    }
}