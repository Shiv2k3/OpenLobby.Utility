using System;
using System.Diagnostics;
using System.Linq;
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
        /// Token for cancellation
        /// </summary>
        public CancellationToken StopToken { get; }

        /// <summary>
        /// Create a listening socket async
        /// </summary>
        /// <param name="port">The port to listen on</param>
        /// <param name="ctk">Token to cancel or disconnect socket</param>
        /// <param name="listen">The Listen() task</param>
        public Client(int port, CancellationToken ctk, out Task listen)
        {
            StopToken = ctk;
            Socket = CreateDefaultSocket();
            listen = Task.Run(() =>
            {
                IPEndPoint lep = new IPEndPoint(IPAddress.Any, port);
                Socket.Bind(lep);
                Socket.Listen(10);

            }, ctk);
        }

        /// <summary>
        /// Create a listening socket
        /// </summary>
        /// <param name="port">The port to listen on</param>
        /// <param name="ctk">Token to cancel or disconnect socket</param>
        public Client(int port, CancellationToken ctk)
        {
            StopToken = ctk;
            Socket = CreateDefaultSocket();
            IPEndPoint lep = new IPEndPoint(IPAddress.Any, port);
            Socket.Bind(lep);
            Socket.Listen(10);
        }
        /// <summary>
        /// Create a listening socket
        /// </summary>
        /// <param name="localEndpoint">The endpoint to listen on</param>
        /// <param name="ctk">Token to cancel or disconnect socket</param>
        /// <param name="bind">The Bind() task</param>
        public Client(IPEndPoint localEndpoint, CancellationToken ctk, out Task bind)
        {
            StopToken = ctk;
            Socket = CreateDefaultSocket();
            bind = Task.Run(() =>
            {
                Socket.Bind(localEndpoint);
                Socket.Listen(10);
            }, ctk);
        }

        /// <summary>
        /// Connects to a remote endpoint
        /// </summary>
        /// <param name="localEndpoint">The local endpoint</param>
        /// <param name="remoteEndpoint">The remote endpoint</param>
        /// <param name="ctk">Token to cancel or disconnect socket</param>
        /// <param name="connect">The Connect() task</param>
        public Client(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint, CancellationToken ctk, out Task connect)
        {
            StopToken = ctk;
            Socket = CreateDefaultSocket();
            connect = Task.Run(async () =>
            {
                Socket.Bind(localEndpoint);
                await Socket.ConnectAsync(remoteEndpoint);
            }, ctk);
        }

        /// <summary>
        /// Creates a client using a remote socket
        /// </summary>
        /// <param name="socket">The remote socket to use</param>
        /// <param name="ctk">Token to cancel or disconnect socket</param>
        /// <exception cref="ArgumentException">The given socket was not remote</exception>
        public Client(Socket socket, CancellationToken ctk)
        {
            if (socket.RemoteEndPoint == null)
                throw new ArgumentException("Socket was not remote");

            StopToken = ctk;
            Socket = socket;
        }

        private static Socket CreateDefaultSocket()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            return socket;
        }
        private static Socket CreateDefaultSocket(Socket s)
        {
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            return s;
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Disconnect()
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Disconnect(true);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.NotConnected)
                    throw new Exception("Socket error code: " + e.SocketErrorCode);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Tries to get the transmission
        /// </summary>
        /// <returns>Null, complete transmission, or just the header of an incomplete transmission</returns>
        public async Task<(bool complete, Transmission? trms)> TryGetTransmission()
        {
            if (StalledTransmission != null)
            {
                var t = await CompleteTransmission(StalledTransmission);
                return (t != null, t);
            }

            if (StalledTransmission == null && Socket.Available >= Transmission.HEADERSIZE)
            {
                byte[] header = new byte[Transmission.HEADERSIZE];
                await Receive(header);
                var t = await CompleteTransmission(new Transmission(header));
                return t == null ? (false, StalledTransmission = t) : (true, t);
            }

            return (false, null);

            async Task<Transmission?> CompleteTransmission(Transmission stalled)
            {
                if (Socket.Available < stalled.Length)
                    return null;

                byte[] data = new byte[stalled.Length];
                await Receive(data);

                return new Transmission(stalled.Payload, data);
            }
        }

        /// <summary>
        /// Sends the payload async
        /// </summary>
        /// <param name="payload">The payload to send</param>
        public async Task<int> Send(byte[] payload)
        {
            int count = 0;
            do
            {
                var segment = new ArraySegment<byte>(payload, count, payload.Length - count);
                count += await Socket.SendAsync(segment, SocketFlags.None, StopToken);
            }
            while (count != payload.Length);
            return count;
        }

        private async Task<int> Receive(byte[] arr)
        {
            int count = 0;
            do
            {
                var segment = new ArraySegment<byte>(arr, count, arr.Length - count);
                count += await Socket.ReceiveAsync(segment, SocketFlags.None, StopToken);
            }
            while (count != arr.Length);
            return count;
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

            Socket remote = CreateDefaultSocket(await acceptTask);
            return new Client(remote, cs);

        }

        /// <returns>Remode endpoint as a string</returns>
        public override string? ToString()
        {
            return Socket.RemoteEndPoint?.ToString();
        }
    }
}