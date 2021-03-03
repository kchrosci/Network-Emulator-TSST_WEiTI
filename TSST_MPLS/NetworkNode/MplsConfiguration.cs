using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace NetworkNode
{
	public enum operation { ADD, SWAP, POP };
	public struct MplsBorderEntry
	{
		public IPAddress dst { get; set; }
		public int transport_layer_port { get; set; }
		public int out_port { get; set; }

		public int label { get; set; }
		//public string node { get; set; }
	}
	public struct MplsForwardEntry
	{
		//public string node { get; set; }
		public int idx { get; set; }
		public int inc_port { get; set; }
		public int label { get; set; }
		public operation op { get; set; }
		public int out_port { get; set; }
		public int out_label { get; set; }
		public int last_idx { get; set; }

	}
	public class MplsConfiguration
	{
		public List<MplsBorderEntry> mplsBorderList = new List<MplsBorderEntry>();
		public List<MplsForwardEntry> mplsForwardList = new List<MplsForwardEntry>();
		public MplsConfiguration(string config_string)
		{
			LoadConfiguration(config_string);
		}

		private void LoadConfiguration(string config_string)
		{
			string[] config = config_string.Split(",end ");
			string[] border_config = new string[0];
			if (!(config.Length == 1))
            {
				if (!string.Empty.Equals(config[0]))
				{
					border_config = config[0].Split(",");
				}
				if (!string.Empty.Equals(config[1]))
				{
					config = config[1].Split(",")[..^1];
				}
			}

			foreach (string line in config)
			{
				MplsForwardEntry temp = new MplsForwardEntry();
				string[] line_par = line.Split();
				temp.idx = int.Parse(line_par[0]);
				temp.inc_port = int.Parse(line_par[1]);
				temp.label = int.Parse(line_par[2]);
				temp.op = (operation)Enum.Parse(typeof(operation), line_par[3]);
				temp.out_port = int.Parse(line_par[4]);
				temp.out_label = int.Parse(line_par[5]);
				temp.last_idx = int.Parse(line_par[6]);
				mplsForwardList.Add(temp);
			}

			foreach (string line in border_config)
			{
				MplsBorderEntry temp = new MplsBorderEntry();
				string[] line_par = line.Split();
				temp.dst = IPAddress.Parse(line_par[0]);
				temp.transport_layer_port = int.Parse(line_par[1]);
				temp.label = int.Parse(line_par[2]);
				temp.out_port = int.Parse(line_par[3]);

				mplsBorderList.Add(temp);
			}

			NetworkNode.ViewOnScreen("Added MPLS configuration!"); 
			PrintConfig();
		}

		public void PrintConfig()
		{
			Console.WriteLine("Dest".PadRight(15) + " | " + "Transport Layer Port | Label | OutPort");
			mplsBorderList.ForEach(delegate (MplsBorderEntry element) { Console.WriteLine(
				element.dst.ToString().PadRight(15) + " | " +
				element.transport_layer_port.ToString().PadRight(20) + " | " +
				element.label.ToString().PadRight(5) + " | " +
				element.out_port.ToString().PadRight(7)); });
			Console.WriteLine("\n--------------------------------------------------------------\n");
			Console.WriteLine("Idx| IncomingPort | Label | Operation | Out_port | OutLabel | Last Index");
			mplsForwardList.ForEach(delegate (MplsForwardEntry element) { Console.WriteLine(element.idx.ToString().PadLeft(3) + "| " +
				element.inc_port.ToString().PadRight(12) + " | " +
				element.label.ToString().PadRight(5) + " | " +
				element.op.ToString().PadRight(9) + " | " +
				element.out_port.ToString().PadRight(8) + " | " +
				element.out_label.ToString().PadRight(8) + " | " +
				element.last_idx.ToString().PadRight(10)); });
		}
	}
}