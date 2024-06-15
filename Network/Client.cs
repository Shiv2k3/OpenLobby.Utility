using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using OpenLobby.Utility.Transmissions;

namespace OpenLobby.Utility.Network
{
    /// <summary>
    /// Repersents a network client
    /// </summary>
    public class Client
    {
        private readonly Socket Socket;
        private Transmission? StalledTransmission;

        /// <summary>
        /// True when new transmission is available
        /// </summary>
        public bool Available => Socket.Available >= Transmission.HEADERSIZE;

        /// <summary>
        /// The remote endpoint, null if it listening
        /// </summary>
        public IPEndPoint? RemoteEndpoint => Socket.RemoteEndPoint as IPEndPoint;

        /// <summary>
        /// The port of the local endpoint
        /// </summary>
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public int LocalPort => (Socket.LocalEndPoint as IPEndPoint).Port;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        /// <summary>
        /// Create a listening socket
        /// </summary>
        /// <param name="port">The port to listen on</param>
        public Client(int port)
        {
            IPEndPoint lep = new IPEndPoint(IPAddress.Any, port);
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(lep);
            Socket.Listen(10);
        }
        /// <summary>
        /// Create a listening socket
        /// </summary>
        /// <param name="localEndpoint">The endpoint to listen on</param>
        public Client(IPEndPoint localEndpoint)
        {
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(localEndpoint);
            Socket.Listen(10);
        }

        /// <summary>
        /// Connects to a remote endpoint
        /// </summary>
        /// <param name="localEndpoint">The local endpoint</param>
        /// <param name="remoteEndpoint">The remote endpoint</param>
        public Client(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint)
        {
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(localEndpoint);
            Socket.ConnectAsync(remoteEndpoint).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a client using a remote socket
        /// </summary>
        /// <param name="socket">The remote socket to use</param>
        /// <exception cref="ArgumentException">The given socket was not remote</exception>
        public Client(Socket socket)
        {
            if (socket.RemoteEndPoint == null)
                throw new ArgumentException("Socket was not remote");

            Socket = socket;
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Disconnect()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }

        /// <summary>
        /// Tries to get a new transmission
        /// </summary>
        /// <returns>Null if no transmission is available</returns>
        public (bool success, Transmission? trms) TryGetTransmission()
        {
            if (StalledTransmission != null)
            {
                var t = CompleteTransmission(StalledTransmission);
                return (t != null, t);
            }

            if (StalledTransmission == null && Available)
            {
                byte[] header = new byte[Transmission.HEADERSIZE];
                Receive(header);
                var t = CompleteTransmission(new Transmission(header));
                return t == null ? (false, StalledTransmission = t) : (true, t);
            }

            return (false, null);

            Transmission? CompleteTransmission(Transmission stalled)
            {
                if (Socket.Available < stalled.Length)
                    return null;

                byte[] data = new byte[stalled.Length];
                Receive(data);

                return new Transmission(stalled.Payload, data);
            }
        }

        /// <summary>
        /// Sends the payload
        /// </summary>
        /// <param name="payload">The payload to send</param>
        /// <returns>False if unable to send</returns>
        public async void Send(byte[] payload)
        {
            int count = 0;
            do
            {
                var segment = new ArraySegment<byte>(payload, count, payload.Length - count);
                count += await Socket.SendAsync(segment, SocketFlags.None);
            }
            while (count != payload.Length);
        }

        private void Receive(byte[] arr)
        {
            int count = 0;
            do
            {
                var segment = new ArraySegment<byte>(arr, count, arr.Length - count);
                count += Socket.Receive(segment, SocketFlags.None);
            }
            while (count != arr.Length);
        }

        /// <summary>
        /// Accepts one new client asynchronously
        /// </summary>
        /// <returns>The new client</returns>
        public async Task<Client> Accept(CancellationToken cs)
        {
            var acceptTask = Socket.AcceptAsync();
            var cancelTask = Task.Delay(Timeout.Infinite, cs);

            var completedTask = await Task.WhenAny(acceptTask, cancelTask);

            if (completedTask == cancelTask)
            {
                cs.ThrowIfCancellationRequested();
            }

            Socket remote = await acceptTask;
            return new Client(remote);

        }

        /// <returns>Remode endpoint as a string</returns>
        public override string? ToString()
        {
            return Socket.RemoteEndPoint?.ToString();
        }
    }
}