using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace NetworkMessageNamespace
{
    public class NetworkMessage
    {
        public int src_port { get; set; }
        public int dst_port { get; set; }
        public IPAddress dst { get; set; }
        public IPAddress src { get; set; }
        public string message { get; set; }
        public int counter { get; set; }
        public List<int> labels { get; set; } = new List<int>();
        public int last_idx { get; set; }
        public int transport_layer_port { get; set; }

        public NetworkMessage(byte[] asset)
        {
            string[] message = Encoding.ASCII.GetString(asset).Split();
            //int i = 0;
            //foreach (string str in message)
            //{   
            //    ViewOnScreen("TUTAJ "+ str +" I jest tutaj: " +i.ToString());
            //    i++;
            //}
            counter = int.Parse(message[0]);
            src_port = int.Parse(message[1]);
            dst_port = int.Parse(message[2]);

            string[] temp = message[3].Trim(Convert.ToChar("#")).Split(",");
            foreach(var str in temp)
            {
                if (!str.Equals(string.Empty))
                {
                    labels.Add(int.Parse(str));
                }
            }

            dst = IPAddress.Parse(message[4]);
            src = IPAddress.Parse(message[5]);
            transport_layer_port = int.Parse(message[6]);
            this.message = string.Join(" ", message[7..]);
        }
        public NetworkMessage(int counter, IPAddress src, int src_port, IPAddress dst, int dst_port, int[] labels, int transport_layer_port, string message)
        {
            this.src_port = src_port;
            this.dst_port = dst_port;
            this.dst = dst;
            this.src = src;
            this.transport_layer_port = transport_layer_port;
            this.message = message;
            this.counter = counter;
            foreach(var el in labels)
            {
                this.labels.Add(el);
            }
        }
        public string MakeSentenceClient(string name,int counter,IPAddress address, string destination)
		{
            return "Package sent:"+"[No."+counter+"]" +" Content= " + message +" "+name+" SourceAddress: " + src_port + " " + src.ToString() + " " + destination + " DestinationAddress: " + address.ToString();
		}

        
        public string GetStringCloud()
        {
            return " Source: " + src + ":" + src_port + " Destination: " + dst + " " + dst_port;
        }


        public string GetString()
        {
            return counter.ToString() + " " + src_port + " " + dst_port + " " + string.Join(",", labels.ToArray()) + " " + src.ToString() + " " + dst.ToString() + " "+transport_layer_port.ToString()+ " " + message;
        }
        public byte[] GetBytes()
        {
            return Encoding.ASCII.GetBytes(counter.ToString() + " "+ src_port + " " + dst_port + " ##" +string.Join(",",labels.ToArray()) + "## " + dst.ToString() + " " + src.ToString() + " " + transport_layer_port.ToString() + " " + message);
        }

        public string GetDiagnosticString()
        {
            return last_idx.ToString() + " " + src_port.ToString() + " " + dst_port.ToString() + " ##" + string.Join(",", labels.ToArray()) + "## ";
        }
        #region ViewOnScreen Method
        /// <summary>
        /// Simple function showing the start time of the event and highlighting it.
        /// </summary>
        /// <param name="msg"></param>
        public static void ViewOnScreen(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[" + "{0:HH:mm:ss.fff}" + "] ", DateTime.Now);
            Task.WaitAll(Task.Run(() => Console.ResetColor()));
            Console.WriteLine(msg);

        }
        #endregion
    }
}
