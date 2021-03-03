using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace CustomerNode
{
	class ReadFromFile
	{
		public List<Connections> PossibleConnections { get; set; }

		public string CloudName { get; set; }

		public IPAddress CloudAddress { get; set; }

		public int CloudPort { get; set; }


		public string NodeName { get; set; }

		public IPAddress NodeAddress { get; set; }

		public int OutPort { get; set; }
		public int DestPort { get; set; }

		public int Label { get; set; }

		public ReadFromFile(string NodeFile)
		{
			
			var LoadedFile = File.ReadAllLines(NodeFile).ToList();
			
			CloudName = GetFromFile(LoadedFile, "CLOUDNAME");
			CloudPort = int.Parse(GetFromFile(LoadedFile, "CLOUDPORT"));
			CloudAddress = IPAddress.Parse(GetFromFile(LoadedFile, "CLOUDADDRESS"));

			NodeName = GetFromFile(LoadedFile, "NODENAME");
			OutPort = int.Parse(GetFromFile(LoadedFile, "OUTPORT"));
			DestPort = int.Parse(GetFromFile(LoadedFile, "DESTPORT"));
			NodeAddress = IPAddress.Parse(GetFromFile(LoadedFile, "NODEADDRESS"));


			string[] pairs = GetFromFile(LoadedFile, "POSSIBLECONNECTIONS").Split(", ");
			PossibleConnections = new List<Connections>();
			foreach (string pair in pairs)
			{
				Connections connections = new Connections();
				connections.Config(pair);
				PossibleConnections.Add(connections);
			}
			this.PossibleConnections = PossibleConnections;

		}

		/// <summary>
		/// Function GetFromFile takes file set as list of strings and returns desirable parameter.
		/// </summary>
		/// <param name="ListLoadedFile"></param>
		/// <param name="SpecifiedWord"></param>
		/// <returns>string</returns>
		private static string GetFromFile(List<string> ListLoadedFile, string SpecifiedWord)
		{
			foreach (var line in ListLoadedFile)
			{
				if (line.StartsWith(SpecifiedWord))
				{
					string str = line.Replace($"{SpecifiedWord} ", "");
					return str;
				}
			}
			return "";
		}

	}
}
