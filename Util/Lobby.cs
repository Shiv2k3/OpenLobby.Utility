﻿using System.Collections.Generic;
using System.Net;
using OpenLobby.Utility.Network;
namespace OpenLobby.Utility.Util
{
    /// <summary>
    /// Repersents a Lobby
    /// </summary>
    public class Lobby
    {
        /// <summary>
        /// Endpoint of the lobby host
        /// </summary>
        public IPEndPoint Host;
        /// <summary>
        /// Lobby ID
        /// </summary>
        public ulong ID;
        /// <summary>
        /// Lobby name
        /// </summary>
        public string Name;
        /// <summary>
        /// Lobby password
        /// </summary>
        public string Password;
        /// <summary>
        /// Is the lobby public
        /// </summary>
        public bool PublicVisible;
        /// <summary>
        /// Lobby max client count
        /// </summary>
        public byte MaxClients;

        /// <summary>
        /// List of joined clients
        /// </summary>
        public List<Client> JoinedClients = new List<Client>();

        /// <summary>
        /// Creates the lobby record
        /// </summary>
        public Lobby(IPEndPoint endpoint, ulong id, string name, string password, bool publicVisible, byte maxClients)
        {
            Host = endpoint;
            ID = id;
            Name = name;
            Password = password;
            PublicVisible = publicVisible;
            MaxClients = maxClients;
        }

        public override string ToString()
        {
            return ID + " Name: " + Name + " @" + Host.ToString();
        }
    }
}