using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System;

namespace ManagementSystem
{
	public enum operation { ADD, SWAP, POP };
	public struct MplsBorderEntry
	{
		public IPAddress dst { get; set; }
		public int transport_layer_port { get; set; }
		public int out_port { get; set; }
		public int label { get; set; }
		public string node { get; set; }
	}
	public struct MplsForwardEntry
	{
		public int idx { get; set; }
		public string node { get; set; }
		public int inc_port { get; set; }
		public int label { get; set; }
		public operation op { get; set; }
		public int out_port { get; set; }
		public int out_label { get; set; }
		public int last_idx { get; set; }

	}
	class MplsConfiguration
	{
		public List<MplsBorderEntry> mplsBorderList = new List<MplsBorderEntry>();
		public List<MplsForwardEntry> mplsForwardList = new List<MplsForwardEntry>();
		public MplsConfiguration(string border_file_path, string forward_file_path)
        {
			LoadConfiguration(border_file_path, forward_file_path);
        }

		private void LoadConfiguration(string border_file_path, string forward_file_path)
		{
			var LoadedFile = File.ReadAllLines(border_file_path).ToList();
			LoadedFile.RemoveAt(0);

			foreach (string line in LoadedFile)
			{
				MplsBorderEntry temp = new MplsBorderEntry();
				string[] line_par = line.Split();
				temp.node = line_par[0];
				temp.dst = IPAddress.Parse(line_par[1]);
				temp.transport_layer_port = int.Parse(line_par[2]);
				temp.label = int.Parse(line_par[3]);
				temp.out_port = int.Parse(line_par[4]);
				mplsBorderList.Add(temp);
			}

			LoadedFile = File.ReadAllLines(forward_file_path).ToList();
			LoadedFile.RemoveAt(0);
			foreach (string line in LoadedFile)
			{
				MplsForwardEntry temp = new MplsForwardEntry();
				string[] line_par = line.Split();
				temp.idx = int.Parse(line_par[0]);
				temp.node = line_par[1];
				temp.inc_port = int.Parse(line_par[2]);
				temp.label = int.Parse(line_par[3]);
				temp.op = (operation)Enum.Parse(typeof(operation), line_par[4]);
				temp.out_port = int.Parse(line_par[5]);
				temp.out_label = int.Parse(line_par[6]);
				temp.last_idx = int.Parse(line_par[7]);
				
				mplsForwardList.Add(temp);
			}
		}

		public void PrintConfig()
		{
			Console.WriteLine("Node | " + "Dest".PadRight(15)+" | "+"Transport Layer Port | Label | OutPort"//+ "\n-------------------------------------------------------------------------------------------------"
				);
            mplsBorderList.ForEach(delegate (MplsBorderEntry element) { Console.WriteLine(element.node.PadRight(4) + " | " + 
				element.dst.ToString().PadRight(15) + " | " + 
				element.transport_layer_port.ToString().PadRight(20) + " | " + 
				element.label.ToString().PadRight(5) + " | " + 
				element.out_port.ToString().PadRight(7)
				//+"\n-------------------------------------------------------------------------------------------------"
				); });

			//Console.WriteLine("------------------------");
			//Console.WriteLine("\n\n");
			Console.WriteLine("\n--------------------------------------------------------------\n");
			Console.WriteLine("Idx| Node | IncomingPort | Label | Operation | Out_port | OutLabel | Last Index");
			mplsForwardList.ForEach(delegate (MplsForwardEntry element) { Console.WriteLine(element.idx.ToString().PadLeft(3) + "| " + 
				element.node.PadRight(4) + " | " + 
				element.inc_port.ToString().PadRight(12) + " | " + 
				element.label.ToString().PadRight(5) + " | " + 
				element.op.ToString().PadRight(9)+" | "+
				element.out_port.ToString().PadRight(8) + " | " + 
				element.out_label.ToString().PadRight(8) + " | " + 
				element.last_idx.ToString().PadRight(10)); });
		}
	}
}
