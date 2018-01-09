using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using ChatServer.Models;

namespace ChatServer.Server
{
    /// <summary>
    /// Singleton ChatService. use INSTANCE member to get the instance
    /// </summary>
    class ChatService : IDisposable
    {
        static ChatService _instance;
        static readonly object _singletonLock = new Object();
        
        private ChatService()
        {
            ClientUsers = new ConcurrentDictionary<Guid, ClientUser>();
            UsernameToGuid = new ConcurrentDictionary<string, Guid>();
        }

        ~ChatService()
        {
            Dispose();
        }

        public void Dispose()
        {
        }

        public static ChatService Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (_singletonLock)
                {
                    return _instance ?? (_instance = new ChatService());
                }
            }
        }

        public event EventHandler ServerStatusChanged;

        private const int BACKLOG = 10;
        private int _currentPort;
        private Socket _listner;
        private ConcurrentDictionary<Guid, ClientUser> ClientUsers;
        private ConcurrentDictionary<string, Guid> UsernameToGuid;

        /// <summary>
        /// Starts the chat server in the current system in the specified port.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool StartServer(int port)
        {
            try
            {
                _currentPort = port;

                _listner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listner.Bind(new IPEndPoint(IPAddress.Any, port));
                _listner.Listen(BACKLOG);
                _listner.BeginAccept(OnClientConnected, null);
            }
            catch (SocketException)
            {
                return false;
            }
            return true;
        }

        private void OnClientConnected(IAsyncResult res)
        {
            //Get the socket object specific to the client
            var clientSocket = _listner.EndAccept(res);

            ClientUser client = new ClientUser(clientSocket);
            ClientUsers[client.Id] = client;

            //Now that we got the client, continue listening for new clients.
            _listner.BeginAccept(OnClientConnected, null);

            //now that the client has connected.. look for a login message from client
            clientSocket.BeginReceive(client.DataBuffer, 0, client.DataBuffer.Length, SocketFlags.None,
                OnClientMessageReceived, client);
        }

        private void OnClientMessageReceived(IAsyncResult res)
        {
            //This is called when one of the client socks has received a message.
            //1. make sure first message is login message
            //2. if message != login message && client is not logged in then close socket (login failed)

            ClientUser client = res.AsyncState as ClientUser;
            try
            {
                int bytesRead = client.Socket.EndReceive(res);
                string msg = Encoding.ASCII.GetString(client.DataBuffer, 0, bytesRead);

                if (client.UserName == null)
                {
                    if (msg.StartsWith("LOGIN:"))
                    {
                        client.UserName = msg.Substring(6, msg.Length - 6);
                        ClientUsers[client.Id] = client;
                        UsernameToGuid[client.UserName] = client.Id;
                    }
                    else
                    {
                        //CLIENT trying to send message before login LOL. Disconnect!
                        client.Socket.BeginDisconnect(true, null, null);
                        ClientUsers.TryRemove(client.Id, out ClientUser user);
                    }
                }
                else
                {
                    //process message. MESSAGE CAN be more than the received piece...
                    //so we should idelaly call beginReceive again now and check for a EOF char. SKipping it now

                    

                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
