using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using NetworkMessageNamespace;
using System.Threading.Tasks;

namespace NetworkNode
{
	public class ComutationField
	{
		public MplsConfiguration mpls { get; set;}
		public List<port_map> port_maps { get; set; } = new List<port_map>();
		public ComutationField(List<port_map> port_maps)
        {
			this.port_maps = port_maps;
        }
		public NetworkMessage HandleMessage(NetworkMessage message)
		{
			if (message.labels.Count == 0)
            {
				int temp_label = mpls.mplsBorderList.Find(x => x.dst.Equals(message.dst) & x.transport_layer_port == message.transport_layer_port).label;
				if (temp_label == 0)
				{
					NetworkNode.ViewOnScreen("There is no MPLS tunnel to IP: " + message.dst.ToString());
					message.labels.Add(temp_label);
					return message;
				}
				else
				{
					message.labels.Add(temp_label);
					NetworkNode.ViewOnScreen("GAVE MPLS LABEL: " + message.labels[0]);
				}
			}

			while (true)
			{

				//ViewOnScreen("Message: " + message.GetDiagnosticString());
				MplsForwardEntry mpls_entry = mpls.mplsForwardList.Find(x => x.label == message.labels[message.labels.Count - 1] &
																			x.inc_port == message.dst_port &
																			x.last_idx == message.last_idx);
				switch (mpls_entry.op)
				{
					case operation.ADD:
						message.labels.Add(mpls_entry.out_label);
						message.src_port = mpls_entry.out_port;
						message.dst_port = port_maps.Find(x => x.src == message.src_port).dst;
						NetworkNode.ViewOnScreen("LABEL ADDED: " + mpls_entry.out_label);
						message.last_idx = mpls_entry.idx;
						//NetworkNode.ViewOnScreen("ACTUAL LABELS: " + string.Join(" ", message.labels));
						return message;
					case operation.SWAP:
						message.src_port = mpls_entry.out_port;
						NetworkNode.ViewOnScreen("LABEL SWAP: " + message.labels[message.labels.Count - 1] +" -> "+ mpls_entry.out_label);
						//NetworkNode.ViewOnScreen("ACTUAL LABELS: " + string.Join(" ", message.labels));
						message.labels[message.labels.Count - 1] = mpls_entry.out_label;
						message.last_idx = mpls_entry.idx;
						if (message.src_port != 0)
						{
							message.dst_port = port_maps.Find(x => x.src == message.src_port).dst;
							return message;
						}
						else break;
					case operation.POP:
						NetworkNode.ViewOnScreen("LABBEL POPPED: " + message.labels[message.labels.Count - 1]);
						//NetworkNode.ViewOnScreen("ACTUAL LABELS: " + string.Join(" ", message.labels));
						message.labels.RemoveAt(message.labels.Count - 1);
						message.src_port = mpls_entry.out_port;
						message.last_idx = mpls_entry.idx;
						
						if (message.labels.Count == 0)
						{
							MplsBorderEntry mpls_border_entry = mpls.mplsBorderList.Find(x => x.dst.Equals(message.dst));
							message.src_port = mpls_border_entry.out_port;
							message.dst_port = port_maps.Find(x => x.src == message.src_port).dst;
							return message;
						}
						break;
				}
			}
		}

		#region ViewOnScreen Method
		/// <summary>
		/// Simple function showing the start time of the event and highlighting it.
		/// </summary>
		/// <param name="msg"></param>
		public static void ViewOnScreen(string msg)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("[" + "{0:HH:mm:ss.fff}" + "] ", DateTime.Now);
			Task.WaitAll(Task.Run(() => Console.ResetColor()));
			Console.WriteLine(msg);

		}
		#endregion
	}
}
