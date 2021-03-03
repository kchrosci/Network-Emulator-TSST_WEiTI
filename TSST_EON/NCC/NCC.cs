using System;
using System.Net;
using Utility;
using NetClientNS;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace NCC
{


    class NCC
    {
        public static Directory myDirectory { get; set; }
        public static Policy myPolicy { get; set; }
        public static NetClient ncc { get; set; }
        public string calledpcc_port { get; set; }
        public string callingpcc_port { get; set; }
        public string cc_port { get; set; }
        public string port { get; set; }
        public string ncc_port { get; set; }
        static string name { get; set; }

        public static int flag=0;
        public string hostcap { get; set; }
        public string realname1 { get; set; }
        public string realname2 { get; set; }
        public string slots { get; set; }


        private static readonly NCC myNCC = new NCC();

        public static NCC NCCInstance
        {
            get { return myNCC; }
        }
        
        public void ConnectionRequest_req(NetworkMessage msg, bool p) //Podajemy adresy portów pomiedzy którymi połaczenie ma być zestawione 
        {
            msg.src = myNCC.port;
            msg.dst = myNCC.cc_port;
            if (p == false)
            {
                msg.payload = "CONNECTION_REQUEST=FAIL " + myNCC.realname1 + " " + myNCC.realname2 + " " + myNCC.hostcap;
            }
            else
            {
                msg.payload = "CONNECTION_REQUEST "+ myNCC.realname1 + " " + myNCC.realname2 +" " + myNCC.hostcap + " " + myNCC.slots;
            }
            ncc.Send(msg.ToBytes());
            Disp.ViewOnScreen($"[NCC -> CC] Connection Request - request.");
            myNCC.slots = "";
        }
        public NetworkMessage DirectoryRequest_req(NetworkMessage msg) //Otrzymanie informacji o adresach snpp poszczególnych uzytkownikow
        {
            NetworkMessage msg2 = myDirectory.DirectoryRequest_rsp(msg);
            return msg2;
        }
        public bool Policy_req(IPAddress adr1, IPAddress adr2, int cap) //Pytanie czy użytkownik może otrzymać połączenie takie jakie oczekuje
        {
            bool k = myPolicy.Policy_rsp(adr1, adr2, cap);
            return k;
        }
       
        public void CallRequest_rsp(IPAddress src, IPAddress dst, bool p) //Możesz zrstawiac połączenie jeśli SUCCESS, nie możesz jeśli FAIL 
        {
            NetworkMessage msg;
            if (p == false)
            {
                msg = new NetworkMessage(myNCC.port, myNCC.callingpcc_port, src, dst, 0, "CALL_REQUEST_RESPONSE=FAIL");
                Disp.ViewOnScreen("[NCC -> CPCC] Call Teardown - response.");
            }
            else
            {
                msg = new NetworkMessage(myNCC.port, myNCC.callingpcc_port, src, dst, 0, "CALL_REQUEST_RESPONSE=SUCCESS");
                Disp.ViewOnScreen("[NCC -> CPCC] Call Request - response.");
            }
            ncc.Send(msg.ToBytes());
           
        }

        public void CallCoordination_req(NetworkMessage msg)//Adres domeny, Adres2, SNPP z innej domeny) //Zestawienie połączenia miedzydomenowego. On już wie ze tu wszystko zestawione wiec mowi to dla NCC obok
        {
            msg.src = myNCC.port;
            msg.dst = myNCC.ncc_port;
            string str = msg.payload.Replace("CONNECTION_REQUEST_RSP=TRUE,NEXT_DOMAIN=TRUE", "");
            msg.payload = "CALL_COORDINATION_REQ " + myNCC.callingpcc_port + " " + myNCC.calledpcc_port + " " + myNCC.hostcap + str;
            ncc.Send(msg.ToBytes());
            Disp.ViewOnScreen("[NCC -> NCC] Call Coordination - request.");
            Disp.ViewOnScreen(msg.ToString());
        }
        public void CallCoordination_rsp(NetworkMessage msg, bool p) //Przekazuje czy zezwolone polaczenie czy nie (jak nie to musi do CC i zwolnic zasoby!)
        {
            msg.src = myNCC.port;
            msg.dst = myNCC.ncc_port;
            if (p == false)
            {
                msg.payload = "CALL_COORDINATION_RSP=FALSE";
                Disp.ViewOnScreen("[NCC -> NCC] Call Coordination Teardown - response.");
            }
            else
            {
                msg.payload = "CALL_COORDINATION_RSP=TRUE";
                Disp.ViewOnScreen("[NCC -> NCC] Call Coordination - response.");
            }
            ncc.Send(msg.ToBytes());
           
        }
        public void CallAccept_req(NetworkMessage msg) //Czy akceptujesz takie zgłoszenie?
        {
            msg.src = myNCC.port;
            msg.dst = myNCC.calledpcc_port;
            if (NCC.flag == 0)
            {
                msg.payload = "CALL_ACCEPT_REQ,NEXT_DOMAIN=FALSE";
            }
            else if (NCC.flag == 1)
            {
                msg.payload = "CALL_ACCEPT_REQ,NEXT_DOMAIN=TRUE";
            }
            ncc.Send(msg.ToBytes());
            Disp.ViewOnScreen("[NCC -> CPCC] Call Accept - request.");
        }

        //CALL TEARDOWN

        public void CallTeardown_req(NetworkMessage msg) //Czy akceptujesz zakończenie połączenia?
        {
            Disp.ViewOnScreen("[NCC -> NCC] Call Coordination Teardown - request.");
            msg.src = myNCC.port;
            msg.dst = myNCC.calledpcc_port;
            if (NCC.flag == 0)
            {
                msg.payload = "CALL_TEARDOWN_REQ,NEXT_DOMAIN=FALSE";
            }
            else if (NCC.flag == 1)
            {
                msg.payload = "CALL_TEARDOWN_REQ,NEXT_DOMAIN=TRUE";
            }
            ncc.Send(msg.ToBytes());
            Disp.ViewOnScreen("[NCC -> CPCC] Call Teardown - request.");
        }

        public void CallCoordinationTeardown_req(NetworkMessage msg) //informowanie drugiej domeny o chęci zakończenia połączenia
        {
            msg.src = myNCC.port;
            msg.dst = myNCC.ncc_port;
            msg.payload = "CALL_COORDINATION_TEARDOWN_REQ";
            ncc.Send(msg.ToBytes());
            Disp.ViewOnScreen("[NCC -> NCC] Call Coordination Teardown - request.");
        }

        public void CallCoordinationTeardown_rsp(NetworkMessage msg) //odpowiedż drugiej domeny na zgłoszenie chęci zakończenia połączenia
        {
            msg.src = myNCC.port;
            msg.dst = myNCC.ncc_port;
            msg.payload = "CALL_COORDINATION_RSP=FALSE";
            ncc.Send(msg.ToBytes());
            Disp.ViewOnScreen("Message sent: ");
        }

        public void CallTeardown_rsp(NetworkMessage msg) //przesyłami pierwszemu klientowi, że ten drugi nie zgodził sie na zerwanie połączenia
        {
            msg.src = myNCC.port;
            msg.dst = myNCC.callingpcc_port;
            msg.payload = "CALL_TEARDOWN_RSP=FALSE";
            ncc.Send(msg.ToBytes());
            Disp.ViewOnScreen("[NCC -> CPCC] Call Teardown - response.");
        }

        static private byte[] HandleMessage(byte[] asset)
        {
            //tutaj jest kod do obslugi wiadomosci przychodzacej
            NetworkMessage msg = new NetworkMessage(asset);
            string[] payload = msg.payload.Split();
            if (payload[0].Equals("CALL_REQUEST"))
            {
                Disp.ViewOnScreen($"[CPCC -> NCC] Call Request - request.");
                myNCC.callingpcc_port = msg.src;
                myNCC.calledpcc_port = payload[2];
                myNCC.hostcap = payload[3];
                Disp.ViewOnScreen($"[NCC -> D] Directory Request - request: {payload[1]} to {payload[2]} ");
                msg = myNCC.DirectoryRequest_req(msg);
               

                Disp.ViewOnScreen($"[D -> NCC] SNPP1: {msg.src_addr} SNPP2: {msg.dst_addr}");
                bool l;
                string[] payload2 = msg.payload.Split();
                if (payload2[0] != "CONNECTION_REQUEST=FAIL")
                { 
                    myNCC.realname1 = payload2[1];
                    myNCC.realname2 = payload2[2];
                    Disp.ViewOnScreen($"[NCC -> P] Policy Request - request: {msg.src_addr} to {msg.dst_addr}, cap:{myNCC.hostcap}");
                    l = myNCC.Policy_req(msg.src_addr, msg.dst_addr, int.Parse(payload[3]));
                    Disp.ViewOnScreen($"[P -> NCC] Connection Allowed!");
                }
                else
                {
                    l = false;
                }
                
                if (l == false)
                {
                    Disp.ViewOnScreen($"[P -> NCC] Connection Disallowed!");
                    myNCC.CallRequest_rsp(msg.src_addr, msg.dst_addr, false);
                }
                else
                {
                   
                    myNCC.ConnectionRequest_req(msg, true);
                }
                
            }
            else if (payload[0].Equals("CONNECTION_REQUEST_RSP=FALSE,NEXT_DOMAIN=FALSE"))
            {
                Disp.ViewOnScreen($"[CC -> NCC] Connection Request - response.");
                myNCC.CallRequest_rsp(msg.src_addr, msg.dst_addr, false); 
            }
            else if (payload[0].Equals("CONNECTION_REQUEST_RSP=FALSE,NEXT_DOMAIN=TRUE"))
            {
                Disp.ViewOnScreen($"[CC -> NCC] Connection Request - response.");
                NCC.flag = 0;
                myNCC.CallCoordination_rsp(msg, false);       
            }
            else if (payload[0].Equals("CONNECTION_REQUEST_RSP=TRUE,NEXT_DOMAIN=FALSE"))
            {
                Disp.ViewOnScreen($"[CC -> NCC] Connection Request - response.");
                myNCC.CallAccept_req(msg);
            }
            else if (payload[0].Equals("CONNECTION_REQUEST_RSP=TRUE,NEXT_DOMAIN=TRUE"))
            {
                Disp.ViewOnScreen($"[CC -> NCC] Connection Request - response.");
                myNCC.CallCoordination_req(msg);

            }
            else if (payload[0].Equals("CALL_ACCEPT_RSP=TRUE,NEXT_DOMAIN=FALSE"))
            {
                Disp.ViewOnScreen($"[CPCC -> NCC] Call Accept - response.");
                myNCC.CallRequest_rsp(msg.src_addr, msg.dst_addr, true);
            }
            else if (payload[0].Equals("CALL_ACCEPT_RSP=TRUE,NEXT_DOMAIN=TRUE"))
            {
                Disp.ViewOnScreen($"[CPCC -> NCC] Call Accept - response.");
                NCC.flag = 0;
                myNCC.CallCoordination_rsp(msg, true);
            }
            else if (payload[0].Equals("CALL_ACCEPT_RSP=FALSE"))
            {
                Disp.ViewOnScreen($"[CPCC -> NCC] Call Accept - response.");
                myNCC.ConnectionRequest_req(msg, false);
            }
            else if (payload[0].Equals("CALL_COORDINATION_RSP=TRUE"))
            {
                Disp.ViewOnScreen($"[NCC -> NCC] Call Coordination - response.");
                myNCC.CallRequest_rsp(msg.src_addr, msg.dst_addr, true);
            }
            else if (payload[0].Equals("CALL_COORDINATION_RSP=FALSE"))
            {
                Disp.ViewOnScreen($"[NCC -> NCC] Call Coordination Teardown - response.");
                myNCC.ConnectionRequest_req(msg, false);
            }
            else if (payload[0].Equals("CALL_COORDINATION_REQ"))
            {
                NCC.flag = 1;
                myNCC.hostcap = payload[3];
                myNCC.calledpcc_port = payload[2];

                string str = msg.payload;
                myNCC.slots = str.Replace(payload[0] + " " + payload[1] + " " + payload[2] + " " + payload[3] + " ", "");
                Disp.ViewOnScreen($"[NCC -> D] Directory Request - request: {payload[1]} to {payload[2]} ");
                msg = myNCC.DirectoryRequest_req(msg);
                string[] payload2 = msg.payload.Split();
                myNCC.realname1 = payload2[1];
                myNCC.realname2 = payload2[2];
                Disp.ViewOnScreen($"[NCC -> NCC] Call Coordination - request.");
                myNCC.ConnectionRequest_req(msg, true);
            }

            //CALL TEARDOWN
            else if (payload[0].Equals("CALL_TEARDOWN_REQ"))
            {
                Disp.ViewOnScreen($"[CPCC -> NCC] Call Teardown - request.");
                myNCC.callingpcc_port = msg.src;
                myNCC.calledpcc_port = payload[2];
                myNCC.hostcap = payload[3];
               // Disp.ViewOnScreen($"Sending directory request: {payload[1]} to {payload[2]} ");
                
                msg = myNCC.DirectoryRequest_req(msg);
                string[] payload2 = msg.payload.Split();
                if (payload2[0] != "CONNECTION_REQUEST=FAIL")
                {
                    
                    myNCC.realname1 = payload2[1];
                    myNCC.realname2 = payload2[2];
                    if (myNCC.callingpcc_port == "H1")
                    {
                        if (myNCC.calledpcc_port == "H2")
                            myNCC.CallTeardown_req(msg);
                        if (myNCC.calledpcc_port == "H3")
                            myNCC.CallCoordinationTeardown_req(msg);
                    }
                    else if(myNCC.callingpcc_port == "H2")
                    {
                        if (myNCC.calledpcc_port == "H1")
                            myNCC.CallTeardown_req(msg);
                        if (myNCC.calledpcc_port == "H3")
                            myNCC.CallCoordinationTeardown_req(msg);
                    }
                    else if(myNCC.callingpcc_port == "H3")
                    {
                        if (myNCC.calledpcc_port == "H2")
                            myNCC.CallCoordinationTeardown_req(msg);
                        if (myNCC.calledpcc_port == "H1")
                            myNCC.CallCoordinationTeardown_req(msg);
                    }
                }
                else
                {
                    myNCC.CallTeardown_rsp(msg);
                }

            }

            else if (payload[0].Equals("CALL_COORDINATION_TEARDOWN_REQ"))
            {
                NCC.flag = 1;
                myNCC.CallTeardown_req(msg);
            }
            else if (payload[0].Equals("CALL_TEARDOWN_RSP=TRUE"))
            {
                Disp.ViewOnScreen($"[CPCC -> NCC] Call Teardown - response.");
                myNCC.ConnectionRequest_req(msg, false);
            }
            else if (payload[0].Equals("CALL_TEARDOWN_RSP=FALSE,NEXT_DOMAIN=FALSE"))
            {
                myNCC.CallTeardown_rsp(msg);
            }
            else if (payload[0].Equals("CALL_TEARDOWN_RSP=FALSE,NEXT_DOMAIN=TRUE"))
            {
                NCC.flag = 0;
                myNCC.CallCoordinationTeardown_rsp(msg);
            }
            else if (payload[0].Equals("CALL_COORDINATION_RSP=FALSE"))
            {
                myNCC.CallTeardown_rsp(msg);
            }


            return msg.ToBytes();
        }

        static private void Config(string file_path)
        {
            string[] config = File.ReadAllLines(file_path);
            int i = 0;
            myNCC.port = config[i++];
            myNCC.cc_port = config[i++];
            myNCC.ncc_port = config[i++];
            name = config[i++];
            Console.Title = name;
        }
        public static void Main(string[] args)
        {
            Config(args[0]);
            Directory myDirectory = new Directory(args[1]);
            //Directory myDirectory = new Directory("..\\..\\..\\..\\Configs\\DirectoryConfig1.txt");
            //Policy myPolicy = new Policy("..\\..\\..\\..\\Configs\\PolicyConfig1.txt");
            Policy myPolicy = new Policy(args[2]);
            NCC.myDirectory = myDirectory;
            NCC.myPolicy = myPolicy;
            
            // Config("..\\..\\..\\..\\Configs\\NCCConfig1.txt");
            NetClient ncc = new NetClient(HandleMessage, name);
            NCC.ncc = ncc;
            Task.Run(() => ncc.ListenForMessages());
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
