using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using NetworkMessageNamespace;

namespace NetworkNode
{
	public struct port_map
	{
		public int src;
		public int dst;

		public void Config(string config)
		{
			string[] par = config.Split();
			this.src = int.Parse(par[0]);
			this.dst = int.Parse(par[1]);
		}

		public override string ToString()
		{
			return src + " " + dst;
		}
	}
	class NetworkNode
	{
		public int counter = 0;
		public string node_name { get; set; }
		public IPAddress management_addr { get; set; }
		public int management_port { get; set; }
		public IPAddress cloud_addr { get; set; }
		public int cloud_port { get; set; }
		public NetworkNodeConfiguration config {get;set;}
		public ManagementAgent management_agent { get; set; }
		public ComutationField commutation_field { get; set; }
		private static Socket main_socket { get; set; } = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static byte[] receive_buffer { get; set; } = new byte[64];

		public List<port_map> port_maps = new List<port_map>();

		/// <summary>
		/// Start function creating new NetworkNode object and then return it to Main.
		/// </summary>
		/// <param name="FilePath"></param>
		/// <returns>NetworkNode</returns>
		public NetworkNode Start(string FilePath)
		{
			NetworkNode nn = new NetworkNode();
			ViewOnScreen("Node is running!");
			try
			{
				config = NetworkNodeConfiguration.ParseConfiguration(FilePath);
				Console.Title = config.NodeName;

				nn.node_name = config.NodeName;
				nn.management_port = config.ManagementPort;
				nn.management_addr = config.ManagementAddress;
				nn.cloud_addr = config.CloudAddress;
				nn.cloud_port = config.CloudPort;
				nn.port_maps = config.port_maps;
			}
			catch (FileNotFoundException e)
			{
				ViewOnScreen(e.Message);
			}
			return nn;
		}

		private static ManualResetEvent sendDone = new ManualResetEvent(false);
		private static ManualResetEvent receiveDone = new ManualResetEvent(false);

		public void StartConnections(NetworkNode networkNode)
		{
			commutation_field = new ComutationField(port_maps);
			management_agent = new ManagementAgent(networkNode);
			main_socket.Connect(new IPEndPoint(cloud_addr, cloud_port));


			int[] arr = { 0 };
			NetworkMessage init = new NetworkMessage(0, IPAddress.Loopback,123, IPAddress.Loopback, 123, arr,80,  node_name);
			Send(main_socket,init.GetBytes());

			ListenForMessages();
		}

		private void ListenForMessages()
        {
			ViewOnScreen("Started waiting for a messages...");
			while (true)
			{
				try
				{
					receiveDone.Reset();
					//ViewOnScreen("LISTENING");
					main_socket.BeginReceive(receive_buffer, 0, 64, 0, new AsyncCallback(ReceiveCallback), main_socket);
					receiveDone.WaitOne();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				Socket client = (Socket)ar.AsyncState;
				int bytesRead = client.EndReceive(ar);

				if (bytesRead > 0)
				{
					ViewOnScreen("Received message: " + Encoding.ASCII.GetString(receive_buffer));
					NetworkMessage msg = commutation_field.HandleMessage(new NetworkMessage(receive_buffer));
					if (msg.labels.Count == 0)
					{
						Send(main_socket, msg.GetBytes());
						ViewOnScreen("Message sent: " + msg.GetString());
					}
					else
					{
						if (msg.labels[0] != 0)
						{
							Send(main_socket, msg.GetBytes());
							ViewOnScreen("Message sent: " + msg.GetString());
						}
					}
					receiveDone.Set();
				}
			}
			catch (Exception e)
			{
				ViewOnScreen(e.ToString());
			}
		}

		private static void Send(Socket client, byte[] byteData)
		{
			sendDone.Reset();
			client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
			sendDone.WaitOne();
		}

		private static void SendCallback(IAsyncResult ar)
		{
			try
			{
				Socket client = (Socket)ar.AsyncState;
				int bytesSent = client.EndSend(ar);
				sendDone.Set();
			}
			catch (Exception e)
			{
				ViewOnScreen(e.ToString());
			}
		}

		public void ReconfigMpls()
        {
            while (true)
            {
				Console.ReadLine();
				management_agent.ConnectManagementSystem();
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
			Console.Write("["+ "{0:HH:mm:ss.fff}"+"] ", DateTime.Now);
			Task.WaitAll(Task.Run(() => Console.ResetColor()));
			Console.WriteLine(msg);

		}
		#endregion

	}
}
