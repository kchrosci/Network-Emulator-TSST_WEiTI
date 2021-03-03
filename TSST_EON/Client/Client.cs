using System;
using System.Threading.Tasks;
using NetClientNS;
using System.Collections.Generic;
using Utility;
using System.Net;
using System.IO;
using System.Threading;

namespace Client
{
    class Client
    {
        public class DataStreamGenerator
        {
            private string gate { get; set; }
            private List<string> hosts { get; set; } = new List<string>();
            private string called_host { get; set; }
        
            public void AddCalledHost(string host)
            {
                called_host = host;
            }
            public void ConfirmHost()
            {
                hosts.Add(called_host);
            }
            public void ConfirmHostDelete()
            {
                hosts.RemoveAt(hosts.FindIndex(x => x.Equals(called_host)));
            }


            public DataStreamGenerator(string gate)
            {
                this.gate = gate;
                Task.Run(() => this.SendData());
            }
            public void SendData()
            {
                while (true)
                {
                    foreach(var host in hosts)
                    {
                        //Console.WriteLine("wchodzi");
                        NetworkMessage msg = new NetworkMessage(Client.name, gate, "DATASTREAM");
                        nc.Send(msg.ToBytes());
                        Disp.ViewOnScreen(msg.ToString());
                        //AddCalledHost(host);
                        //ConfirmHostDelete();
                    }
                    Thread.Sleep(10000);
                }
            }
        }
        public static NetClient nc { get; set; }
        private static readonly CPCC myCPCC = new CPCC();
        public static CPCC CPCCInstance
        {
            get { return myCPCC; }
        }
        public class CPCC
        {
            public string port { get; set; }
            public string gate_port { get; set; }
            public string ncc_port { get; set; }
            public IPAddress dst_addr { get; set; }
            public IPAddress src_addr { get; set; }
            public CPCC() { }

            public DataStreamGenerator stream_gen { get; set; }

            public void CallRequest_req(string src, string dst, int cap, NetClient sender)
            {
                NetworkMessage msg = new NetworkMessage(this.port, this.ncc_port, "CALL_REQUEST " + src + " " + dst + " " + cap.ToString());
                sender.Send(msg.ToBytes());
                Disp.ViewOnScreen(msg.ToString("Message sent: "));
                stream_gen.AddCalledHost(dst);
            }
            public void CallTeardown_req(string dst, int cap, NetClient sender)
            {
                NetworkMessage msg = new NetworkMessage(this.port, this.ncc_port, "CALL_TEARDOWN_REQ " + this.port + " " + dst + " " + cap.ToString());
                sender.Send(msg.ToBytes());
                Disp.ViewOnScreen(msg.ToString("Message sent: "));
                stream_gen.AddCalledHost(dst);
            }
            public void CallAccept_rsp(NetworkMessage msg, NetClient sender, bool next_domain)
            {
                msg.src = myCPCC.port;
                msg.dst = myCPCC.ncc_port;
                msg.payload = "HEHE";
                Disp.ViewOnScreen("Do you accept the call?");
                string choice = Console.ReadLine();
                if (choice.Equals("accept"))
                {
                    if (next_domain) msg.payload = "CALL_ACCEPT_RSP=TRUE,NEXT_DOMAIN=TRUE";
                    else msg.payload = "CALL_ACCEPT_RSP=TRUE,NEXT_DOMAIN=FALSE";
                }
                else if (choice.Equals("refuse")) msg.payload = "CALL_ACCEPT_RSP=FALSE";
                 
                sender.Send(msg.ToBytes());
                Disp.ViewOnScreen(msg.ToString("Message sent: "));
            }

            public void CallTeardown_rsp(NetworkMessage msg, NetClient sender, bool next_domain)
            {
                msg.src = myCPCC.port;
                msg.dst = myCPCC.ncc_port;
                Disp.ViewOnScreen("Do you accept disconnection?");
                string choice = Console.ReadLine();
                if (choice.Equals("accept")) msg.payload = "CALL_TEARDOWN_RSP=TRUE";
                else if (choice.Equals("refuse"))
                {
                    if(next_domain) msg.payload = "CALL_TEARDOWN_RSP=FALSE,NEXT_DOMAIN=TRUE";
                    else msg.payload = "CALL_TEARDOWN_RSP=FALSE,NEXT_DOMAIN=FALSE";
                }
                sender.Send(msg.ToBytes());
                Disp.ViewOnScreen(msg.ToString("Message sent: "));
            }
        }

        static private byte[] HandleMessage(byte[] asset)
        {
            NetworkMessage msg = new NetworkMessage(asset);
            Disp.ViewOnScreen(msg.ToString("Message received: "));
            string[] payload = msg.payload.Split();
            payload[0] = payload[0].TrimEnd(new char[] { (char)0 });
            if (payload[0].Equals("CALL_REQUEST_RESPONSE=FAIL")) {
                myCPCC.stream_gen.ConfirmHostDelete();
            }
            else if (payload[0].Equals("CALL_REQUEST_RESPONSE=SUCCESS"))
            {
                myCPCC.stream_gen.ConfirmHost();
            }
            else if (payload[0].Equals("CALL_ACCEPT_REQ,NEXT_DOMAIN=TRUE"))
            {
                myCPCC.CallAccept_rsp(msg, nc, true);
            }
            else if (payload[0].Equals("CALL_ACCEPT_REQ,NEXT_DOMAIN=FALSE"))
            {
                myCPCC.CallAccept_rsp(msg, nc, false);
            }
            else if (payload[0].Equals("CALL_TEARDOWN_REQ,NEXT_DOMAIN=TRUE"))
            {
                myCPCC.CallTeardown_rsp(msg, nc, true);
            }
            else if (payload[0].Equals("CALL_TEARDOWN_REQ,NEXT_DOMAIN=FALSE"))
            {
                myCPCC.CallTeardown_rsp(msg, nc, false);
            }
            else if (payload[0].Equals("CALL_TEARDOWN_RSP=FALSE"))
            {
                Disp.ViewOnScreen("Response from NCC: Called Party Call Controller refused to disconnect.");
            }  
            return msg.ToBytes();
        }

        static private int port { get; set; }
        static private IPAddress addr { get; set; }
        static private string name { get; set; }

        static private void Config(CPCC cpcc, string file_path)
        {
            string[] config = File.ReadAllLines(file_path);
            int i = 0;
            cpcc.port = config[i++];
            cpcc.ncc_port = config[i++];
            port = int.Parse(config[i++]);
            cpcc.dst_addr = IPAddress.Parse(config[i++]);
            cpcc.src_addr = IPAddress.Parse(config[i++]);
            name = config[i++];
            cpcc.gate_port = config[i++];
            Console.Title = name;
            cpcc.stream_gen = new DataStreamGenerator(cpcc.gate_port);
        }
        static void Main(string[] args)
        {
            Config(myCPCC,args[0]);
            Disp.ViewOnScreen($"*** CLI {name} ***");
            Disp.ViewOnScreen($"Commands: callrequest DST CAP, teardownrequest DST CAP");
            NetClient nc = new NetClient(HandleMessage, name);
            Client.nc = nc;
            Task.Run(() => nc.ListenForMessages());
          
            while (true)
            {
                string[] str = Console.ReadLine().Split();
                
                switch (str[0]) {
                    case "interrupt":
                        break;
                    case "callrequest": //callrequest DST CAP
                        myCPCC.CallRequest_req(name, str[1], int.Parse(str[2]), nc);
                        break;
                    case "teardownrequest": //teardownrequest DST CAP 
                        myCPCC.CallTeardown_req(str[1], int.Parse(str[2]), nc);
                        break;
                    case "true":
                        Console.WriteLine("Confirm:");
                        break;
                    default:
                        Console.WriteLine("Incorrect command");
                        break;
                }
            }
        }
    }
}
