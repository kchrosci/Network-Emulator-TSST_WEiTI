using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility;
using System.IO;
using NetClientNS;
using System.Threading;

namespace NetworkManagement
{
    class RoutingProtocol
    {
        private class Node
        {
            public string name { get; set; }
            public List<int> links { get; set; }
            public int dist { get; set; }
            public string prev { get; set; }
            public Node(string name)
            {
                this.name = name;
                links = new List<int>();
                dist = 0;
                prev = string.Empty;
            }

        }
        private class Link
        {
            public int id { get; set; }
            public string start { get; set; }
            public string stop { get; set; }
            public int dist { get; set; }
            public string innerRC { get; set; }

            public Link(int id, string start, string stop, int dist, string innerRC)
            {
                this.id = id;
                this.start = start;
                this.stop = stop;
                this.dist = dist;
                this.innerRC = innerRC;
            }
        }

        private NetClient nc {get; set;}
        private List<Node> nodes = new List<Node>();
        private List<Link> links = new List<Link>();
        private string name { get; set; }


        public void newNode(string name, List<int> links)
        {
            Node n = new Node(name);
            foreach(var l in links)
            {
                n.links.Add(l);
            }
            nodes.Add(n);
        }
        public void newLink(int id, string start, string stop, int dist, string innerRC)
        {
            Link n = new Link(id, start, stop, dist, innerRC);
            links.Add(n);
        }
        private Node getNode(string name)
        {
            return nodes.Find(x => x.name.Equals(name));
        }

        private Node getNode(string name, List<Node> list)
        {
            return list.Find(x => x.name.Equals(name));
        }

        private Link getLink(int id)
        {
            return links.Find(x => x.id == id);
        }

        private int getWeight(string start, string stop)
        {
            return links.Find(x => (x.start.Equals(start) & x.stop.Equals(stop))).dist;
        }

        private List<Node> findNeighbours(Node v, List<Node> list)
        {
            List<Node> nbr = new List<Node>();
            foreach(int link_id in v.links)
            {
                Link k = getLink(link_id);
                //Node temp = null;
                //if(k.weight<=max_weight) temp = getNode(k.stop, list);
                Node temp = getNode(k.stop, list);
                if (temp != null) nbr.Add(temp);
            }

            return nbr;
        }

        public RoutingProtocol(string node_config_path, string link_config_path, NetClient nc, string name)
        {
            this.name = name;
            this.nc = nc;
            Disp.ViewOnScreen("Starting RC!");
            string[] node_config = File.ReadAllLines(node_config_path);
            string[] link_config = File.ReadAllLines(link_config_path);

            foreach(var el in node_config)
            {
                string[] conf = el.Split();
                List<int> links = new List<int>();
                foreach (var link in conf[1..]) links.Add(int.Parse(link));
                newNode(conf[0], links);
            }

            foreach (var el in link_config)
            {
                string[] conf = el.Split();
                newLink(int.Parse(conf[0]), conf[1], conf[2], int.Parse(conf[3]), conf[4]);
            }
        }

        public void PathTeardown() { }

        public void DistUpdateRequest()
        {
            foreach(var link in links)
            {
                if (!link.innerRC.Equals("K"))
                {
                    NetworkMessage msg = new NetworkMessage(name, link.innerRC, "DIST_UPDATE_REQUEST " + link.start + " " + link.stop);
                    nc.Send(msg.ToBytes());
                    Disp.ViewOnScreen(msg.ToString("Dist update request send: "));
                }
            }
        }

        public void UpdateInnerLink(NetworkMessage msg)
        {
            string[] config = msg.payload.Split();
            string start = config[1];
            string stop = config[2];
            int dist = int.Parse(config[3]);
            links.ElementAt(links.FindIndex(x => x.start.Equals(start) & x.stop.Equals(stop))).dist = dist;
        }

        public void DistUpdateResponse(NetworkMessage msg)
        {
            string[] config = msg.payload.Split();
            string start = config[1];
            string stop = config[2];
            string stop_cpy = stop;
            List<string> path = ShortestPath(start, stop_cpy);
            Disp.ViewOnScreen("wyznaczona sciezka w dst update response " + string.Join(" ", path.ToArray())); //DEBUG
            //Disp.ViewOnScreen("siema to jest stop: " + stop);//DEBUG
            NetworkMessage new_msg = new NetworkMessage(msg.dst, msg.src, string.Join(" ", new string[] { "DIST_UPDATE_RESPONSE", start, stop, GetDistance(path).ToString() }));
            Disp.ViewOnScreen(new_msg.ToString("DistUpdateResponse executed: ")); //DEBUG
            nc.Send(new_msg.ToBytes());
            
        }

        private int GetDistance(List<string> path)
        {
            int dist = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                dist += links.ElementAt(links.FindIndex(x => x.start.Equals(path[i]) & x.stop.Equals(path[i + 1]))).dist;
            }

            if (path.Last().Equals("N7")) dist += 40;
            else if (path.Last().Equals("N6")) dist += 100;
            return dist;
        }
        
        public List<string> ShortestPath(string source, string target, int capacity)
        {
            DistUpdateRequest();
            //Disp.ViewOnScreen(source + ".." + target); //DEBUG

            List<Node> Q = new List<Node>();
            List<Node> U = new List<Node>();

            foreach (Node v in nodes)
            {
                v.dist = int.MaxValue;
                if (v.name.Equals(source)) v.dist = 0;
                else v.dist = int.MaxValue;
                Q.Add(v);
            }

            Node u = null;
            while (Q.Count != 0)
            {
                Q.Sort((x, y) => x.dist.CompareTo(y.dist));
                u = Q[0];
                if (u.name.Equals(target)) break;
                U.Add(u);
                Q.RemoveAt(0);

                List<Node> nbr = findNeighbours(u, Q);
                foreach(var v in nbr)
                {
                    int alt = u.dist + getWeight(u.name, v.name);
                    if (alt < getNode(v.name, Q).dist)
                    {
                        int idx = Q.FindIndex(x => x.name.Equals(v.name));
                        Q[idx].dist = alt;
                        Q[idx].prev = u.name;
                    }
                }
            }

            List<string> path = new List<string>();
            path.Insert(0, u.name);
            Node temp = getNode(u.prev, U);
            int slots_n;
            if (temp.name.Equals(source))
            {
                path.Insert(0, temp.name);
                slots_n = CalculateSlots(GetDistance(path), capacity);
                path.Insert(0, slots_n.ToString());
                return path;
            }
            else
            {
                while (!temp.prev.Equals(source))
                {
                    path.Insert(0, temp.name);
                    temp = getNode(temp.prev, U);
                }
                path.Insert(0, temp.name);
                path.Insert(0, temp.prev);
            }


            slots_n = CalculateSlots(GetDistance(path),capacity);
            path.Insert(0, slots_n.ToString());
            return path;
        }
        public List<string> ShortestPath(string source, string target)
        {
            DistUpdateRequest();
            //Disp.ViewOnScreen(source + ".." + target); //DEBUG

            List<Node> Q = new List<Node>();
            List<Node> U = new List<Node>();

            foreach (Node v in nodes)
            {
                v.dist = int.MaxValue;
                if (v.name.Equals(source)) v.dist = 0;
                else v.dist = int.MaxValue;
                Q.Add(v);
            }

            Node u = null;
            while (Q.Count != 0)
            {
                Q.Sort((x, y) => x.dist.CompareTo(y.dist));
                u = Q[0];
                if (u.name.Equals(target)) break;
                U.Add(u);
                Q.RemoveAt(0);

                List<Node> nbr = findNeighbours(u, Q);
                foreach (var v in nbr)
                {
                    int alt = u.dist + getWeight(u.name, v.name);
                    if (alt < getNode(v.name, Q).dist)
                    {
                        int idx = Q.FindIndex(x => x.name.Equals(v.name));
                        Q[idx].dist = alt;
                        Q[idx].prev = u.name;
                    }
                }
            }

            List<string> path = new List<string>();
            path.Insert(0, u.name);
            Node temp = getNode(u.prev, U);
            if (temp.name.Equals(source))
            {
                path.Insert(0, temp.name);
                return path;
            }
            else
            {
                while (!temp.prev.Equals(source))
                {
                    path.Insert(0, temp.name);
                    temp = getNode(temp.prev, U);
                }
                path.Insert(0, temp.name);
                path.Insert(0, temp.prev);
            }

            return path;
        }
        public List<string> ShortestAroundPath(string source, string target)
        {
            List<Node> Q = new List<Node>();
            List<Node> U = new List<Node>();

            foreach (Node v in nodes)
            {
                v.dist = int.MaxValue;
                if (v.name.Equals(source)) v.dist = 0;
                else v.dist = int.MaxValue;

                int remove_at_idx = 999;
                foreach(int link in v.links)
                {
                    Link temp_link = getLink(link);
                    if(temp_link.start.Equals(source) & temp_link.stop.Equals(target))
                    {
                        remove_at_idx = v.links.FindIndex(x => x == link);
                    }
                }

                if(remove_at_idx!=999) v.links.RemoveAt(remove_at_idx);

                Q.Add(v);
            }

            Node u = null;
            while (Q.Count != 0)
            {
                Q.Sort((x, y) => x.dist.CompareTo(y.dist));
                u = Q[0];
                if (u.name.Equals(target)) break;
                U.Add(u);
                Q.RemoveAt(0);

                List<Node> nbr = findNeighbours(u, Q);
                foreach (var v in nbr)
                {
                    int alt = u.dist + getWeight(u.name, v.name);
                    if (alt < getNode(v.name, Q).dist)
                    {
                        int idx = Q.FindIndex(x => x.name.Equals(v.name));
                        Q[idx].dist = alt;
                        Q[idx].prev = u.name;
                    }
                }
            }

            List<string> path = new List<string>();
            path.Insert(0, u.name);
            Node temp = getNode(u.prev, U);
            if (temp.name.Equals(source))
            {
                path.Insert(0, temp.name);
                return path;
            }
            else
            {
                while (!temp.prev.Equals(source))
                {
                    path.Insert(0, temp.name);
                    temp = getNode(temp.prev, U);
                }
                path.Insert(0, temp.name);
                path.Insert(0, temp.prev);
            }

            return path;
        }

        public void RemoveLink(string source, string target)
        {
            Node v = getNode(source);
            int remove_at_idx = 999;
            foreach (int link in v.links)
            {
                Link temp_link = getLink(link);
                if (temp_link.start.Equals(source) & temp_link.stop.Equals(target))
                {
                    remove_at_idx = v.links.FindIndex(x => x == link);
                }
            }
            if (remove_at_idx != 999) v.links.RemoveAt(remove_at_idx);
        }
        private int CalculateSlots(int distanceSum, int capacity)
		{
            int channelsNum;
            int subcarries = 2;  //podnosne
            double efficiency = 0.4; //efektywnosc
            double freq; //otrzymana fq
            double EONfreq = 12.5;
            string modname;
            int mod;
            if (distanceSum <= 100)
            {
                mod = (int)Math.Log2(16);
                modname = "16QAM";
            }
            else if (distanceSum <= 200)
            {
                mod = (int)Math.Log2(8);
                modname = "8QAM";
            }
            else if (distanceSum <= 250)
            {
                mod = (int)Math.Log2(4);
                modname = "QPSK";
            }
            else
            {
                mod = (int)Math.Log2(2);
                modname = "BPSK";
            }
            freq = ((capacity / (mod * subcarries)) * efficiency);
            channelsNum = (int)((freq / EONfreq) + 1) * subcarries;
            Disp.ViewOnScreen($"[RC]: Total distance: {distanceSum}. Modulation: {modname}");
            return channelsNum;
        }
    }
}
