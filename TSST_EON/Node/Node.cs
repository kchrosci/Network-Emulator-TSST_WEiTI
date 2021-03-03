using System;
using Utility;
using NetClientNS;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace Node
{
    class Node
    {
        private class ForwardMap
        {
            public string dst { get; set; }
            public List<int> channels { get; set; }

            public ForwardMap(string[] config)
            {
                channels = new List<int>();
                int i = 0;
                dst = config[0];
                foreach(var el in config[1..]) channels.Add(int.Parse(el));
                Disp.ViewOnScreen("RESOURCES ALLOCATED: " + dst + " " + string.Join(" ", channels.ToArray()));
            }

            public static void Reallocate(List<ForwardMap> list, string old_dst, string new_dst)
            {

                foreach (var el in list)
                {
                    if (el.dst.Equals(old_dst)) el.dst = new_dst;
                    Disp.ViewOnScreen("RESOURCES REALLOCATED: " + old_dst + " " + new_dst);// " " + string.Join(" ", config[1..]));
                }
            }
        }

        static List<ForwardMap> forward_map = new List<ForwardMap>();
        static string name { get; set; }

        static private byte[] HandleMessage(byte[] asset)
        {
            NetworkMessage msg = new NetworkMessage(asset);
            string[] payload = msg.payload.Split();
            if (payload[0].Equals("ALLOCATION"))
            {
                forward_map.Add(new ForwardMap(payload[1..]));
                msg.dst = "DELETE";
            }
            else if (payload[0].Equals("REALLOCATION"))
            {
                ForwardMap.Reallocate(forward_map, payload[1], payload[2]);
                msg.dst = "DELETE";
            }
            else if (payload[0].Equals("RELEASE"))
            {
                int idx = forward_map.FindIndex(x => x.dst.Equals(payload[1]) & x.channels.Contains(int.Parse(payload[2])));
                if (idx != -1)
                {
                    forward_map.RemoveAt(idx);
                    Disp.ViewOnScreen("RESOURCES RELEASED");
                }
                else
                {
                    Disp.ViewOnScreen("RESOURCES NOT RELEASED");
                    Disp.ViewOnScreen(msg.ToString());
                }
                
                msg.dst = "DELETE";
            }
            else
            {
                int channel = msg.transport_port;
                msg.src = name;
                msg.dst = forward_map.Find(x => x.channels.Contains(channel)).dst;
            }
       
            return msg.ToBytes();
        }

        static private void Config(string file_path)
        {
            string[] config = File.ReadAllLines(file_path);
            int i = 0;
            name = config[i++];
            //foreach(string nbr in config[i++].Split()) neighbours.Add(nbr);
            //foreach (string lrm in config[i++].Split()) lrms.Add(lrm);
            Console.Title = name;
        }

        static private void WatchForLinkFailure(NetClient nc)
        {
            while (true)
            {
                string node = Console.ReadLine();
                string lrm = string.Empty;
                if (name.Equals("N1")|name.Equals("N3")|name.Equals("N2"))
                { 
                    if (node.Equals("N1") | node.Equals("N3")|node.Equals("N2"))
                    {
                        lrm = "INC1"; 
                    }
                }
                else if (name.Equals("N4") | name.Equals("N5") | name.Equals("N6"))
                {
                    if (node.Equals("N4") | node.Equals("N5") | node.Equals("N6"))
                    {
                        lrm = "INC2";
                    }
                }
                else if (name.Equals("N7") | name.Equals("N8") | name.Equals("N9"))
                {
                    if (node.Equals("N7") | node.Equals("N8") | node.Equals("N9"))
                    {
                        lrm = "NC2";
                    }
                }

                if (!string.IsNullOrEmpty(lrm))
                {
                    nc.Send(new NetworkMessage(name, lrm, "LINK_FAILURE " + node).ToBytes());
                }
            }
        }
        static void Main(string[] args)
        {   
            Config(args[0]);
            NetClient nc = new NetClient(HandleMessage, name);
            Task.Run(() => nc.ListenForMessages());
            while (true)
            {
                string node = Console.ReadLine();
                string lrm = string.Empty;
                if (name.Equals("N1") | name.Equals("N3") | name.Equals("N2"))
                {
                    if (node.Equals("N1") | node.Equals("N3") | node.Equals("N2"))
                    {
                        lrm = "INC1";
                    }
                }
                else if (name.Equals("N4") | name.Equals("N5") | name.Equals("N6"))
                {
                    if (node.Equals("N4") | node.Equals("N5") | node.Equals("N6"))
                    {
                        lrm = "INC2";
                    }
                }
                else if (name.Equals("N7") | name.Equals("N8") | name.Equals("N9"))
                {
                    if (node.Equals("N7") | node.Equals("N8") | node.Equals("N9"))
                    {
                        lrm = "NC2";
                    }
                }

                if (!string.IsNullOrEmpty(lrm))
                {
                    nc.Send(new NetworkMessage(name, lrm, "LINK_FAILURE " + node).ToBytes());
                }
            }
        }
    }
}
