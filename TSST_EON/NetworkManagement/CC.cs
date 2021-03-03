using System;
using NetClientNS;
using System.Collections.Generic;
using System.Linq;
using Utility;
using System.Net;

namespace NetworkManagement
{
    class CC
    {
        public string Name { get; set; }
		private NetClient nc;
		private RoutingProtocol rc;
		private LRM lrm;
		private string SNPP1;
		private string SNPP2;
		private string Node1;
		private string Node2;
		private int Capacity;
		private string slotsNum;
		public bool NextDomain = false;
		private List<string> Path;
		public List<int> Channels { get; set; } = new List<int>();

		public List<int> ChannelsAvailable = Enumerable.Range(1, 30).ToList();

		public CC(NetClient nc, RoutingProtocol rc, LRM lrm, string name)
		{
			this.nc = nc;
			this.rc = rc;
			this.lrm = lrm;
			Name = name;
			Disp.ViewOnScreen("Starting CC!");
		}

		public bool ConnectionRequest_req(string node1, string node2, int cap,string snpp1,string snpp2, string slots)
		{

			Disp.ViewOnScreen("[NCC -> CC]: Connection Request - request." );
			Node1 = node1;
			Node2 = node2;
			SNPP1 = snpp1;
			SNPP2 = snpp2;
			Capacity = cap;
			Disp.ViewOnScreen("[CC -> RC]: Route Table Query - request.");

			if (slots.Equals(""))
			{
				Path = RouteTableQuery_req(rc, Node1, Node2, Capacity);
				slotsNum = Path[0];
				Path.RemoveAt(0);
				Channels = getChannels(int.Parse(slotsNum));
			}
			else
			{
				Path = RouteTableQuery_req(rc, Node1, Node2);
				string[] slotsArr = slots.Split();
				foreach(var s in slotsArr)
				{
					Channels.Add(int.Parse(s));
					var p = ChannelsAvailable.Find(x => x.Equals(int.Parse(s)));
					ChannelsAvailable.Remove(p);
				}
			}

			Disp.ViewOnScreen("[RC -> CC]: Route Table Query - response.");
			Disp.ViewOnScreen("[CC]: Path has been chosen! " + string.Join(" ", Path.ToArray()));
			Disp.ViewOnScreen("[CC -> LRM]: SNP Link Connection Request - request: " + string.Join(" ", Path.ToArray()) + ", " + string.Join(" ", Channels.ToArray()));
			return lrm.SNPLinkConnectionRequest_req(Path, Channels);
		}


		private List<int> getChannels(int slotsNum)
		{
			List<int> temp = new List<int>();
			for (int i = 0; i < slotsNum; i++)
			{
				temp.Add(ChannelsAvailable[0]);
				ChannelsAvailable.RemoveAt(0);
				
			}
			Disp.ViewOnScreen("[RC]: SLOTS CHOSE: "+ string.Join(" ",temp.ToArray()));
			return temp;
		}

		public void ConnectionRequest_rsp(NetworkMessage msg, bool response,string slots)
		{
			if (NextDomain == false & response == true)
			{
				msg.payload = "CONNECTION_REQUEST_RSP=TRUE,NEXT_DOMAIN=FALSE " +slots;
				nc.Send(msg.ToBytes());
				Disp.ViewOnScreen(msg.ToString("[LOG]: "));
			}
			else if (NextDomain == true & response == true)
			{
				msg.payload = "CONNECTION_REQUEST_RSP=TRUE,NEXT_DOMAIN=TRUE "+slots;
				nc.Send(msg.ToBytes());
				Disp.ViewOnScreen(msg.ToString("[LOG]: "));
			}
			else if (NextDomain == false & response == false)
			{ 
				msg.payload = "CONNECTION_REQUEST_RSP=FALSE,NEXT_DOMAIN=FALSE "+slots;
				nc.Send(msg.ToBytes());
				Disp.ViewOnScreen(msg.ToString("[LOG]: "));
			}
			else if (NextDomain == true & response == false)
			{
				msg.payload = "CONNECTION_REQUEST_RSP=FALSE,NEXT_DOMAIN=TRUE "+slots;
				nc.Send(msg.ToBytes());
				Disp.ViewOnScreen(msg.ToString("[LOG]: "));
			}
		}

		public List<string> RouteTableQuery_req(RoutingProtocol rc, string node1, string node2, int cap) //Otrzymanie informacji o drogę przed podsieć
		{
			Disp.ViewOnScreen("[RC]: Preparing possible path!");
			Disp.ViewOnScreen("[RC]: Source: "+ node1 +" Target: "+ node2+" Cap: "+ cap.ToString());
			List<string> path = rc.ShortestPath(node1, node2,cap);
			return path;	
		}

		public List<string> RouteTableQuery_req(RoutingProtocol rc, string node1, string node2) //Otrzymanie informacji o drogę przed podsieć
		{
			Disp.ViewOnScreen("[RC]: Preparing possible path!");
			Disp.ViewOnScreen("[RC]: Source: "+ node1 +" Target: "+ node2);
			List<string> path = rc.ShortestPath(node1, node2);
			return path;	
		}

		public void ConnectionRequestInner_rsp()  
		{

		}

		public void ConnectionTeardown_req(string snpp1, string snpp2, int cap)
		{
			Disp.ViewOnScreen("[NCC]: CONNECTION TEARDOWN REQUEST!");
			SNPP1 = snpp1;
			SNPP2 = snpp2;
			Capacity = cap;
			lrm.SNPReleaseResources(Path,Channels);
		}

		public void ConnectionTeardown_rsp(NetworkMessage msg)
		{
			if (NextDomain == false)
			{
				Disp.ViewOnScreen("[CC]: CONNECTION TEARDOWN REQUEST - RESPONSE: Teardown succesful!");
				msg.payload = "CONNECTION_REQUEST_RSP=FALSE,NEXT_DOMAIN=FALSE";
				//*********** Tu jest jakas lekka beka
				string temp = msg.dst;
				msg.dst = msg.src;
				msg.src = temp;
				nc.Send(msg.ToBytes());
			}
			else if (NextDomain == true)
			{
				Disp.ViewOnScreen("[CC]: CONNECTION TEARDOWN REQUEST - RESPONSE: Teardown succesful! Next Domain!");
				msg.payload = "CONNECTION_REQUEST_RSP=FALSE,NEXT_DOMAIN=TRUE";
				//*********** Tu jest jakas lekka beka
				string temp = msg.dst;
				msg.dst = msg.src;
				msg.src = temp;
				nc.Send(msg.ToBytes());
			}				
		}

		//Wysyła do węzła requesta przez które będzie odbywało się polaczenie w danej podsieci
		//podsieci (to już specjalny Request do konkretnego fizycznego wezla z numerami jego portów konkretnych we i wy)
		public void ConnectionRequestInner_req() 
		{
			for (int i = 0; i < Path.Count-1; i += 2)
			{
				if (Path[i].ToCharArray()[0].Equals('N') & Path[i + 1].ToCharArray()[0].Equals('N'))
				{
					NetworkMessage msg = new NetworkMessage(Name, getInnerName(Path.ElementAt(i)), IPAddress.Loopback, IPAddress.Loopback, 0, "");
					msg.payload = "CONNECTION_REQUEST_INNER " + Path.ElementAt(i) + " " + Path.ElementAt(i+1) + " " + Capacity.ToString() + " " + string.Join(" ", Channels.ToArray());
					nc.Send(msg.ToBytes());
				}
			}
		}
		public void ConnectionTeardownInner_req() 
		{
			for (int i = 0; i < Path.Count-1; i += 2)
			{
				if (Path[i].ToCharArray()[0].Equals('N') & Path[i + 1].ToCharArray()[0].Equals('N'))
				{
					NetworkMessage msg = new NetworkMessage(Name, getInnerName(Path.ElementAt(i)), IPAddress.Loopback, IPAddress.Loopback, 0, "");
					msg.payload = "CONNECTION_TEARDOWN_INNER " + Path.ElementAt(i) + " " + Path.ElementAt(i+1) + " " + Capacity.ToString() + " " + string.Join(" ", Channels.ToArray());
					nc.Send(msg.ToBytes());
				}
			}
		}

		//obsługa handlera wewnetrznego
		public void ConnectionRequest_req(string node1,string node2,string snpp1, string snpp2, int cap, string channels)
		{
			//List<int> chan = new List<int>();
			//foreach (var el in channels.Split()) chan.Add(int.Parse(el));
			Disp.ViewOnScreen("[CC -> Inner_CC]: Connection Request - request.");
			Node1 = node1;
			Node2 = node2;
			SNPP1 = snpp1;
			SNPP2 = snpp2;
			Capacity = cap;

			Disp.ViewOnScreen("[CC -> RC]: Route Table Query - request.");
			Path = RouteTableQuery_req(rc, Node1, Node2);
			string[] slotsArr = channels.Split();
			foreach (var s in slotsArr)
			{
				Channels.Add(int.Parse(s));
				var p = ChannelsAvailable.Find(x => x.Equals(int.Parse(s)));
				ChannelsAvailable.Remove(p);
			}

			//Path = RouteTableQuery_req(rc, Node1, Node2, Capacity);
			
			Disp.ViewOnScreen("[RC -> CC]: Route Table Query - response.");

			Disp.ViewOnScreen("[CC]: Path has been chosen! "+ string.Join(" ", Path.ToArray()));

			Disp.ViewOnScreen("[CC -> LRM]: SNP Link Connection Request - request: " + string.Join(" ", Path.ToArray()) + ", " + string.Join(" ", Channels.ToArray()));
			lrm.SNPLinkConnectionRequest_req(Path, Channels);
			Disp.ViewOnScreen("[Inner_CC -> CC]: Connection Request - response.");
		}

		public string getInnerName(string node)
        {
			if (node.Equals("N1") | node.Equals("N2") | node.Equals("N3")) return "INC1";
			else if (node.Equals("N4") | node.Equals("N5") | node.Equals("N6")) return "INC2";
			return "FAIL";
        }

		public void LinkRestoration(string src, string dst)
        {
			lrm.SNPReleaseResources_req(src, dst, Channels);
			Disp.ViewOnScreen("[CC -> RC] Route Table Query - request.");
			List<string> new_path = rc.ShortestPath(src, dst);
			Disp.ViewOnScreen("[RC -> CC] Route Table Query - response. Path: "+string.Join(" ", new_path.ToArray()));
			lrm.SNPLinkConnectionRequest_req(new_path, Channels);
        }
	}
}




