using System;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace Utility
{
    public class PortsMap
    {
        private struct PortMap
        {
            public int src { get; set; }
            public int dst { get; set; }
        }

        List<PortMap> port_maps = new List<PortMap>();

        public PortsMap(string config)
        {
            foreach (var el in config.Split(";"))
            {
                port_maps.Add(new PortMap() { src = int.Parse(el.Split()[0]), dst = int.Parse(el.Split()[1]) });
            }
        }

        public void AddMap(string config)
        {
            port_maps.Add(new PortMap() { src = int.Parse(config.Split()[0]), dst = int.Parse(config.Split()[1]) });
        }
        
        public void RemoveMap(int[] config)
        {
            port_maps.RemoveAt(port_maps.FindIndex(x => (x.src == config[0] || x.dst == config[1])));
        }

        public int FindOtherEnd(int port)
        {
            return port_maps.Find(x => x.src == port).dst;
        }
        
        public List<int> getSrcPorts()
        {
            List<int> ports = new List<int>();
            foreach(var map in port_maps)
            {
                ports.Add(map.src);
            }
            return ports;
        }
    }

    public class Disp
    {
        public static void ViewOnScreen(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[" + "{0:HH:mm:ss.fff}" + "] ", DateTime.Now);
            Task.WaitAll(Task.Run(() => Console.ResetColor()));
            Console.WriteLine(msg);
        }
        public static void ViewClient(string name, string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[" + "{0:HH:mm:ss.fff}" + "] ", DateTime.Now);
            Task.WaitAll(Task.Run(() => Console.ResetColor()));
            Console.WriteLine(msg);
            Console.Write(name + "> ");
        }
    }


    public class NetworkMessage
    {
        
        public string src { get; set; } 
        public string dst { get; set; }
        public IPAddress src_addr { get; set; }
        public IPAddress dst_addr { get; set; }
        public int transport_port { get; set; }
        public string payload { get; set; }


        public NetworkMessage(byte[] asset)
        {
            string[] message = Encoding.ASCII.GetString(asset).Split();
            int i = 0;
            src = message[i++];
            dst = message[i++];
            src_addr = IPAddress.Parse(message[i++]);
            dst_addr = IPAddress.Parse(message[i++]);
            transport_port = int.Parse(message[i++]);
            payload = string.Join(" ", message[(i++)..]).TrimEnd('\0');
        }

        public NetworkMessage(string src_port, string dst_port, IPAddress src, IPAddress dst, int transport_port, string payload)
        {
            this.src = src_port;
            this.dst = dst_port;
            this.dst_addr = dst;
            this.src_addr = src;
            this.transport_port = transport_port;
            this.payload = payload;
        }

        public NetworkMessage(string src_port, string dst_port, string payload)
        {
            this.src = src_port;
            this.dst = dst_port;
            this.dst_addr = IPAddress.Loopback;
            this.src_addr = IPAddress.Loopback;
            this.transport_port = 1;
            this.payload = payload;
        }

        public string ToString(string heading)
        {
            return heading + "[SRC] " + src.PadRight(4, ' ') + " " + " [DST] " + dst.PadRight(4, ' ') + " [MSG] " + payload;
        }
        public override string ToString()
        {
            return "[SRC] " + src.PadRight(4,' ') + " " + " [DST] " + dst.PadRight(4,' ') + " [MSG] " + payload;
        }

        public string ToStringWAddr() //stary to string z adresami
        {
            return src + " " + dst + " " + src_addr.ToString() + " " + dst_addr.ToString() + " " + transport_port.ToString() + " " + payload;
        }

        public byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(src + " " + dst + " " + src_addr.ToString() + " " + dst_addr.ToString() + " " + transport_port.ToString() + " " + payload);
        }
    }
}

