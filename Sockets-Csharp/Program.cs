﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sockets_Csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            echoServer server = new echoServer();

            server.Start();

            Console.WriteLine("Server running");
            Console.ReadLine();
        }
    }
}
