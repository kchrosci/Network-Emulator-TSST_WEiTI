using System;
using System.IO;
using NetClientNS;
using System.Threading.Tasks;
using Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;

namespace NetworkManagement
{
    class NetworkManagement
    {
	    static string Inner_Name { get; set; }    
		static string Name { get; set; }
        static string Upper_Name { get; set; }
        public int Capacity { get;set; }
        //static string Links { get; set; } = "..\\..\\..\\..\\Configs\\LRM1.txt";
        static string Links { get; set; }
        public static RoutingProtocol _rc { get; set; }

        public static CC _cc { get; set; }
        public static LRM _lrm { get; set; }
        static bool Inner_Available { get; set;} = false;
        public static List<Connection> Connections = new List<Connection>();
        public static List<int> Channels = new List<int>();
        public class Connection
        {   
            public string SNPP1 { get; set; }
            public string SNPP2 { get; set; }
            public string Node1 { get; set; }
            public string Node2 { get; set; }
            public int Capacity { get; set; }
            public string Slots { get; set; }
           

            public Connection(string snpp1, string snpp2, string node1, string node2, int cap,string slots)
            {
                Node1 = node1;
                Node2 = node2;
                SNPP1 = snpp1;
                SNPP2 = snpp2;
                Capacity = cap;
                Slots = slots;
            }
        }
        public static void Main(string[] args)
        {
            Config(args[0],args[1]);
            //Config("..\\..\\..\\..\\Configs\\NetworkConfig1.txt");
            
            NetClient nc = new NetClient(HandleMessage, Name);
            Task.Run(() => nc.ListenForMessages());
			
            if (Inner_Name.Equals(" ") && !Name.Equals("NC2"))
			{
                Disp.ViewOnScreen("INNER SUBNETWORK " + Name);
                NetworkMessage msg = new NetworkMessage(Name,Upper_Name, IPAddress.Parse("1.0.0.1"), IPAddress.Parse("2.0.0.1"), 1,"INNER_NC_AVAILABLE");
                nc.Send(msg.ToBytes());
			}

            Disp.ViewOnScreen("Starting subnetwork!");

            //RoutingProtocol rc = new RoutingProtocol("..\\..\\..\\..\\Configs\\RCConfigNode1.txt", "..\\..\\..\\..\\Configs\\RCConfigLink1.txt");
            RoutingProtocol rc = new RoutingProtocol(args[2], args[3], nc, Name);
            LRM lrm = new LRM(nc,rc, Name, Links);
            CC cc = new CC(nc, rc, lrm,Name);
            _cc = cc;
            _rc = rc;
            _lrm = lrm;
            _lrm.cc = cc;
            while (true)
            {
                Console.ReadLine();
            }
        }
        static private void Config(string file_path,string file_path2)
        {
            string[] config = File.ReadAllLines(file_path);
            int i = 0;
            Name = config[i++];
            Inner_Name = config[i] == "." ? " " : config[i];
            Upper_Name = config[++i];
            Links = file_path2;
            Console.Title = Name;
        }
        
        static private byte[] HandleMessage(byte[] asset)
        {
            NetworkMessage msg = new NetworkMessage(asset);
           
            string[] payload = msg.payload.Split();
            if (payload[0].Equals("CONNECTION_REQUEST"))
            {
                Connection connection = new Connection(msg.src_addr.ToString(), msg.dst_addr.ToString(), payload[1], payload[2], int.Parse(payload[3]), string.Join(" ", payload[4..]));
                Connections.Add(connection);
                if (Connections.Any())
                {
                    if (Connections[0].Node2 == "N7" || Connections[0].Node2 == "N6")
                    {
                        _cc.NextDomain = true;
                    }

                    bool connection_request_flag =_cc.ConnectionRequest_req(Connections[0].Node1, Connections[0].Node2, Connections[0].Capacity, Connections[0].SNPP1, Connections[0].SNPP2,Connections[0].Slots);
                    
                    Channels = _cc.Channels;

                    if (connection_request_flag)
                    {
                        if (Inner_Available)
                        {
                            Disp.ViewOnScreen("[CC -> Inner_CC]: Connection Request - request.");
                            _cc.ConnectionRequestInner_req();
                            Disp.ViewOnScreen("[Inner_CC -> CC]: Connection Request - response.");
                        }

                        msg.src = Name;
                        msg.dst = Upper_Name;
                        Thread.Sleep(100);
                        Disp.ViewOnScreen("[CC -> NCC]: Connection Request - response.");
                        _cc.ConnectionRequest_rsp(msg, connection_request_flag, string.Join(" ", Channels.ToArray()));
                    }
                }
                else
                {
                    Disp.ViewOnScreen("Could not read connection!");
                }
            }
            else if (payload[0].Equals("CONNECTION_REQUEST=FAIL"))
            {
                int index = 0;
               
                foreach (var c in Connections)
                {
                    if (c.SNPP1.Equals(Connections[index].SNPP1) && c.SNPP2.Equals(Connections[index].SNPP2) && c.Capacity.Equals(Connections[index].Capacity))
                    {
                        if (c.SNPP2.Equals("10.10.10.3"))
						{
                            _cc.NextDomain = true;
                        }
                        if (Connections[index].Node2 == "N7" || Connections[index].Node2 == "N6")
                        {
                            _cc.NextDomain = false;
                        }
                        _cc.ConnectionTeardown_req(Connections[index].Node1, Connections[index].Node2, Connections[index].Capacity);
                       
                        if (Inner_Available)
                        {
                            Disp.ViewOnScreen("[CC -> Inner_CC]: Connection Teardown - request.");
                            _cc.ConnectionTeardownInner_req();
                            Disp.ViewOnScreen("[Inner_CC -> CC]: Connection Teardown - response.");
                        }
                        _cc.ConnectionTeardown_rsp(msg);
                        Connections.RemoveAt(index);
                        break;
                    }
                    index++;
                }
            }
            else if (payload[0].Equals("CONNECTION_REQUEST_INNER"))
            {
                _cc.ConnectionRequest_req(payload[1], payload[2], payload[1], payload[2], int.Parse(payload[3]), string.Join(" ", payload[4..]));
            }
            else if (payload[0].Equals("CONNECTION_TEARDOWN_INNER"))
            {
                _cc.ConnectionTeardown_req(payload[1], payload[2],int.Parse(payload[3]));
            }
            else if (payload[0].Equals("INNER_NC_AVAILABLE"))
            {
                Inner_Available = true;
                Inner_Name = msg.src;
            }
            else if (payload[0].Equals("CALL_TEARDOWN_REQUEST")) 
            {
            }
            else if (payload[0].Equals("DIST_UPDATE_REQUEST"))
            {
                _rc.DistUpdateResponse(msg);
            }
            else if (payload[0].Equals("DIST_UPDATE_REQUEST"))
            {
                _rc.UpdateInnerLink(msg);
            }
            else if (payload[0].Equals("LINK_FAILURE"))
            {
                _lrm.LinkFailureHandle(msg.src, payload[1]);
            }
            else
            {
                msg.dst = "DELETE";
            }
            return msg.ToBytes();
        }
    }
}
