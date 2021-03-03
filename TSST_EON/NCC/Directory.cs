using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Utility;

namespace NCC
{
    class Directory
    {
        public class DirectoryEntry
        {

            public IPAddress src { get; set; }
            public IPAddress dst { get; set; }
            public string name1 { get; set; }
            public string name2 { get; set; }
            public string realname1 { get; set; }
            public string realname2 { get; set; }


            public DirectoryEntry(string[] config)
            {
                int i = 0;
                src = IPAddress.Parse(config[i++]);
                dst = IPAddress.Parse(config[i++]);
                name1 = config[i++];
                name2 = config[i++];
                realname1 = config[i++];
                realname2 = config[i++];
            }
        }

        public List<DirectoryEntry> directories = new List<DirectoryEntry>();
        public Directory(string file_path)
        {
            string[] config = File.ReadAllLines(file_path);
            foreach (var str in config) directories.Add(new DirectoryEntry(str.Split()));
        }

        public NetworkMessage DirectoryRequest_rsp(NetworkMessage msg) //(ZWRACA ADRES KLIENTA (SNPP), o którego zapytał się NCC), czyli które snpp jest zwracane?
        {
            string[] payload = msg.payload.Split();
            //Console.WriteLine(payload[1] + " " + msg.src);
            DirectoryEntry result = directories.Find(x => (x.name1.Contains(payload[1]) && x.name2.Contains(payload[2])));
            if (result == null)
                msg.payload = "CONNECTION_REQUEST=FAIL";
            else 
            { 
                msg.src_addr = result.src;
                msg.dst_addr = result.dst;
                msg.payload = "CONNECTION_REQUEST=SUCCESS " + result.realname1 + " " + result.realname2 + " " + payload[2];
            }

            return msg;
        }

    }
}
