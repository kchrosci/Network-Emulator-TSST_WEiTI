using System;
using ServerNS;
using System.IO;

namespace CableCloud
{
    class CableCloud
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
        }
    }
}
