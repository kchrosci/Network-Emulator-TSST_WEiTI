using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace NetworkNode
{
	class ManagementAgent
	{
		private NetworkNode network_node { get; set; }

		public ManagementAgent(NetworkNode _networkNode)
		{
			network_node = _networkNode;

			ConnectManagementSystem();
		}

		public void ConnectManagementSystem()
		{
			try
			{                
                IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, network_node.management_port);
                string request = network_node.node_name;
                Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
                Byte[] bytesReceived = new Byte[256];

                using (Socket socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
					socket.Connect(ep);
                    if (socket == null)  ViewOnScreen("Connection failed");
                    socket.Send(bytesSent, bytesSent.Length, 0);
                    socket.Receive(bytesReceived, bytesReceived.Length, 0);
					socket.Shutdown(0);
					socket.Close();
                }
				network_node.commutation_field.mpls = new MplsConfiguration(Encoding.ASCII.GetString(bytesReceived));
            }
			catch (SocketException e)
			{
				ViewOnScreen(e.Message + "\n" + "\t\tNetwork node could not establish connection with MS!");
			}

		}
		#region ViewOnScreen Method
		/// <summary>
		/// Simple function showing the start time of the event and highlighting it.
		/// </summary>
		/// <param name="msg"></param>
		public void ViewOnScreen(string msg)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("[" + "{0:HH:mm:ss.fff}" + "] ", DateTime.Now);
			Task.WaitAll(Task.Run(() => Console.ResetColor()));
			Console.WriteLine(msg);

		}
		#endregion
	}
}
