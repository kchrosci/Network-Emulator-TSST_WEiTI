using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace NetworkNode
{   
	/// <summary>
	///  This class' job is to read from file and set properties for objects.
	/// </summary>
	class NetworkNodeConfiguration
	{	
		// Get/set name of this node.
		public string NodeName { get; set; }

		// Get/Set address and port of ManagementAgent.
		public IPAddress ManagementAddress { get; set; }
		public int ManagementPort { get; set; }
		
		// Get/Set address and port of Cloud.
		public IPAddress CloudAddress { get; set; }
		public int CloudPort { get; set; }

		public List<port_map> port_maps = new List<port_map>();

		
		/// <summary>
		/// ParseConfiguration is parsing elements of config file.
		/// </summary>
		/// <param name="NodeFile"></param>
		/// <returns>Config</returns>
		public static NetworkNodeConfiguration ParseConfiguration(string NodeFile)
		{
			var LoadedFile = File.ReadAllLines(NodeFile).ToList();
			var ConfigWorker = new NetworkNodeConfiguration();

			ConfigWorker.NodeName = GetFromFile(LoadedFile,"NODENAME");
			ConfigWorker.CloudPort = int.Parse(GetFromFile(LoadedFile, "CLOUDPORT"));
			ConfigWorker.CloudAddress = IPAddress.Parse(GetFromFile(LoadedFile, "CLOUDADDRESS"));
			ConfigWorker.ManagementPort = int.Parse(GetFromFile(LoadedFile, "MANAGEMENTPORT"));
			ConfigWorker.ManagementAddress = IPAddress.Parse(GetFromFile(LoadedFile, "MANAGEMENTADDRESS"));

			string[] ports_string = GetFromFile(LoadedFile, "PORTS").Split(", ");
			foreach (string el in ports_string)
			{
				port_map link = new port_map();
				link.Config(el);
				ConfigWorker.port_maps.Add(link);
			}

			return ConfigWorker;
		}
		/// <summary>
		/// Function GetFromFile takes file set as list of strings and returns desirable parameter.
		/// </summary>
		/// <param name="ListLoadedFile"></param>
		/// <param name="SpecifiedWord"></param>
		/// <returns>string</returns>
		private static string GetFromFile(List<string> ListLoadedFile, string SpecifiedWord)
		{
			foreach(var line in ListLoadedFile)
			{
				if (line.StartsWith(SpecifiedWord))
				{
					string str = line.Replace($"{SpecifiedWord} ","");
					return str;
				}				
			}
			return "";
		}

	}

}
