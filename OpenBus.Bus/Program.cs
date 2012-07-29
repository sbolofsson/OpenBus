using System;
using OpenBus.Bus.Servers;

namespace OpenBus.Bus
{
    public static class Program
    {
        private static Server _server;

        static void Main()
        {
            Console.WriteLine("Press <Enter> to stop the bus.");
            _server = new Server();
            _server.Start();
            Console.ReadLine();
        }
    }
}
