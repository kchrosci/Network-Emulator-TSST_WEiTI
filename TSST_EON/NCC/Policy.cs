using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace NCC
{
    class Policy
    {
        public class PolicyEntry
        {
            
            public IPAddress src { get; set; }
            public IPAddress dst { get; set; }
            public int max_cap { get; set; }
            public PolicyEntry(string[] config)
            {
                int i = 0;
                src = IPAddress.Parse(config[i++]);
                dst = IPAddress.Parse(config[i++]);
                max_cap = int.Parse(config[i++]);
            }
        }

        public List<PolicyEntry> policies = new List<PolicyEntry>();
        public Policy(string file_path)
        {
            string[] config = File.ReadAllLines(file_path);
            foreach (var str in config) policies.Add(new PolicyEntry(str.Split())); 
        }
        public bool Policy_rsp(IPAddress src, IPAddress dst, int cap) //Zezwala lub nie na zestawienie polaczenia 
        {
            PolicyEntry result = policies.Find(x => (x.src.ToString().Contains(src.ToString()) && x.dst.ToString().Contains(dst.ToString())));
            if (result.max_cap > cap)
                return true;
            else
                return false;
        }

    }
}
