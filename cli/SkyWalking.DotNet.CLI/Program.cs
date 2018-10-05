using System;
using System.IO;

namespace SkyWalking.DotNet.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var current =  Directory.GetCurrentDirectory();
            Console.WriteLine(current);
        }
    }
}