using BlueSky;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueSkyTest
{
    internal class Program
    {
        static void Main()
        {
            var client = new Client("", "");
            client.PostImage("Hello World", File.ReadAllBytes("test.png"), "image/png").Wait();
        }
    }
}
