using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("VFileManager");
            Console.WriteLine("Enter your command");
            bool isExit = false;
            while(!isExit)
            {
                Console.Write(":>");
                string input = Console.ReadLine();
                if (input == "exit") isExit = true;
            }
        }
    }
}
