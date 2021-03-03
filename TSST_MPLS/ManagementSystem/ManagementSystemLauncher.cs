using System;
using System.Collections.Generic;
using System.Text;

namespace ManagementSystem
{
    class ManagementSystemLauncher
    {
        static void Main(string[] args)
        {
            ManagementSystem ms = new ManagementSystem(args[0], args[1]);
            //MplsConfiguration mpls_config = new MplsConfiguration("..\\..\\..\\..\\Configs\\mpls_border_config.txt", "..\\..\\..\\..\\Configs\\mpls_config.txt");
            
            Console.Title = "ManagementSystem";
            ms.HandleNodes();
            Console.ReadLine();
        }
    }
}
