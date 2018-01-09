using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Models
{
    class ClientUser
    {
        public byte[] DataBuffer { get; set; }

        public Socket Socket { get; }

        public string UserName { get; set; } = null;

        public Guid Id { get; }

        public ClientUser(Socket clientSocket)
        {
            Id = Guid.NewGuid();
            Socket = clientSocket;
            DataBuffer = new byte[1024];
        }
    }
}
