using System;
using NetClientNS;
using System.Collections.Generic;
using System.Linq;
using Utility;
using System.Net;
using System.IO;
using System.Threading;


namespace NetworkManagement
{
    class LRM
    {
        private class LinkState
        {
            public string k1 { get; set; }
            public string k2 { get; set; }
            public List<int> channels { get; set; }
            public List<bool> up { get; set; }
            public LinkState(string config)
            {
                int i = 0;
                string[] conf = config.Split();
                k1 = conf[i++];
                k2 = conf[i++];
                channels = new List<int>();
                up = new List<bool>();
                foreach (string el in conf[(i++)..])
                {
                    channels.Add(int.Parse(el));
                    up.Add(true);
                }
            }
        }

        private NetClient nc;
        private RoutingProtocol rc;
        public CC cc { get; set; }
        private string name;
        private List<LinkState> links = new List<LinkState>();
        public LRM(NetClient nc, RoutingProtocol rc, string name, string config_path)
        {
            Disp.ViewOnScreen("Starting LRM!");
            this.nc = nc;
            this.name = name;
            string[] conf = File.ReadAllLines(config_path);
            foreach (var el in conf) links.Add(new LinkState(el));
            this.rc = rc;

        }

        public bool SNPLinkConnectionRequest_req(List<string> nodes, List<int> channels)
        {
            //Disp.ViewOnScreen("[LRM]: HANDLING SNP LINK CONNECTION REQUEST:" + string.Join(" ", nodes.ToArray()) + ", " + string.Join(" ", channels.ToArray()));

            bool free = true;
            string k1, k2;

            for(int i = 0; i < nodes.Count-1; i++)
            {
                k1 = nodes.ElementAt(i);
                k2 = nodes.ElementAt(i+1);
                LinkState temp = getLink(k1, k2);
                if (temp != null)
                {
                    foreach(var el in channels)
                    {
                        free = temp.up[temp.channels.FindIndex(x=> x == el)];
                        if (free == false) break;
                    }
                    if (free == false) break;
                    SNPAllocation_req(nodes.ElementAt(i), nodes.ElementAt(i+1), channels);
                    Thread.Sleep(200);
                }
            }

            if (free == true)
            {
                for (int i = 0; i < nodes.Count - 1; i++)
                {
                    k1 = nodes.ElementAt(i);
                    k2 = nodes.ElementAt(i + 1);
                    LinkState temp = getLink(k1, k2);
                    if (temp != null) foreach (var el in channels) temp.up[temp.channels.FindIndex(x => x == el)] = false;    
                }
                Disp.ViewOnScreen("[LRM -> CC]: SNP Link Connection Request - response. " + "Success. Slot's ID: " + string.Join(" ", channels.ToArray()));
            }
            return free;      
        }

        private void SNPAllocation_req(string node, string dst, List<int> channels)
        {
            Disp.ViewOnScreen("[CC -> LRM]: SNP ALLOCATION REQUEST: " + node+ " "+dst+ ", " + string.Join(" ", channels.ToArray()));
            NetworkMessage msg = new NetworkMessage(this.name, node, IPAddress.None, IPAddress.None, 1, "ALLOCATION " + dst + " " + string.Join(" ", channels.ToArray()));
            nc.Send(msg.ToBytes());
        }

        public void SNPReleaseResources_req(string node, string dst, List<int> channels)
        {
            Disp.ViewOnScreen("[CC -> LRM]: SNP RELEASE RESOURCES REQUEST: " + node + " " + dst + ", " + string.Join(" ", channels.ToArray()));
            NetworkMessage msg = new NetworkMessage(this.name, node, IPAddress.None, IPAddress.None, 1, "RELEASE " + dst + " " + string.Join(" ", channels.ToArray())+ " ");
            nc.Send(msg.ToBytes());
        }

        public bool SNPReleaseResources(List<string> nodes, List<int> channels)
        {
            Disp.ViewOnScreen("[CC -> LRM]: SNP LINK RESOURCES RELEASE: " + string.Join(" ", nodes.ToArray()) + ", " + string.Join(" ", channels.ToArray()));
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                SNPReleaseResources_req(nodes.ElementAt(i), nodes.ElementAt(i + 1), channels);
                Thread.Sleep(200);

            }
            return true;
        }

        private LinkState getLink(string k1, string k2)
        {
            return links.Find(x => (x.k1.Equals(k1) & x.k2.Equals(k2))| (x.k1.Equals(k2) & x.k2.Equals(k1)));
        }

        //SNPNegotiation_rsp
        //    public void SNPLinkConnectionRequest_rsp(SNPA, SNPZ) //Oznacza swoje szczeliny jako zajęte i odpowiada CC
        public void LinkFailureHandle(string src, string dst)
        {

            Disp.ViewOnScreen("[LRM -> RC] Local Topology: link_failure: " + src + " " + dst);
            rc.RemoveLink(src, dst);
            Disp.ViewOnScreen("[LRM -> CC] LINK FAILURE: " + src + " " + dst);
            cc.LinkRestoration(src, dst);
            
            /*
            nc.Send(new NetworkMessage(name, src, "REALLOCATION " + dst + " " + path[1]).ToBytes());
            List<int> slots = new List<int>();
            List<int> channels = links[links.FindIndex(x => x.k1.Equals(src) & x.k2.Equals(dst))].channels;
            int i = 0;
            foreach (bool up in links[links.FindIndex(x => x.k1.Equals(src) & x.k2.Equals(dst))].up)
            {
                if (up == false) slots.Add(channels[i]);
                i++;
            }

            for(int k = 1; k<path.Count-1; k++)
            {
                SNPAllocation_req(path[k], path[k + 1], slots);
            }
            */

        }
    }
}
