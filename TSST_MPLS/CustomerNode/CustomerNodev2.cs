using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkMessageNamespace;

namespace CustomerNode
{

	class CustomerNodev2
	{
		private static ManualResetEvent connectDone = new ManualResetEvent(false);
		private static ManualResetEvent sendDone = new ManualResetEvent(false);
		private static ManualResetEvent receiveDone = new ManualResetEvent(false);
		
		public static int howMany;
		public static int counter = 0;


		public static int howMany2;
		public static int counter2;

		private Socket _socket { get; set; } = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static byte[] receive_buffer { get; set; } = new byte[64];
		public IPEndPoint EndPoint { get; private set; }
	
		public static ReadFromFile readFromFile;
		
		// Get/set name of this node.
		public string NodeName { get; set; }
		public IPAddress NodeAddress { get; set; }
		public int OutPort { get; set; }
		public int DestPort { get; set; }

		// Get/Set address and port of Cloud.
		public string CloudName { get; set; }
		public IPAddress CloudAddress { get; set; }
		public int CloudPort { get; set; }
		public string Label { get; set; }

		private List<Connections> Connections = new List<Connections>();

		public CustomerNodev2(ReadFromFile File)
		{
			NodeName = File.NodeName;
			NodeAddress = File.NodeAddress;
			OutPort = File.OutPort;
			DestPort = File.DestPort;
			CloudName = File.CloudName;
			CloudAddress = File.CloudAddress;
			CloudPort = File.CloudPort;
			Connections = File.PossibleConnections;
			Label = "1";
		}
		
		public async static Task Main(string[] args)
		{
			try
			{
				readFromFile = new ReadFromFile(args[0]);
				//readFromFile = new ReadFromFile($"..\\..\\..\\..\\Configs\\CustomerConfig1.txt");
				CustomerNodev2 customerNodev2 = new CustomerNodev2(readFromFile);
				await Task.Run(() => customerNodev2.Start());

			}
			catch(ArgumentNullException e)
			{
				ViewOnScreen(e.ToString()+" Invalid Config path. Try again");
			}
		}

		public async Task Start()
		{
			ViewOnScreen($"CustomerNode {NodeName} {NodeAddress} is running!");
			Console.Title = NodeName;
						
			await Task.Run(() => ConnectCC());
		}

		private async Task ConnectCC()
		{
			ViewOnScreen($"Connecting to {CloudName} at {CloudAddress}:{CloudPort}!");
			try
			{
				EndPoint = new IPEndPoint(CloudAddress, CloudPort);
				_socket.BeginConnect(EndPoint, new AsyncCallback(ConnectCallback), _socket);
				connectDone.WaitOne();
				int[] arr = { 0 };
				NetworkMessage networkMessage = new NetworkMessage(howMany, NodeAddress, OutPort, Connections[0].IPAddress, 1, arr,80,  NodeName);
				Send(_socket, networkMessage.GetBytes());

				//Sprawdzam czy polaczenie jest nawiazane
				//Console.WriteLine(SocketConnected(_socket));
			
				await Task.Run(() => ClientListen());
			}
			catch (SocketException)
			{					
				ViewOnScreen($"Host {NodeName} could not establish connection with CloudCable!");
			}
		}

		public async Task SendMessage()
		{
			ViewOnScreen($"Possible connection with:");
			foreach (Connections connections in Connections)
			{
				ViewOnScreen($"****-> {connections} <-****");
			}

			ViewOnScreen($"Input name of destination host: ");
			string destination = "";
			await Task.Run(() => destination = Console.ReadLine());

			ViewOnScreen($"Write down your message:");
			string message = "";
			await Task.Run(() => message = Console.ReadLine());
			ViewOnScreen($"Write down amount of them:");
			await Task.Run(() => howMany = int.Parse(Console.ReadLine()));
			counter = 0;
			while (true)
			{   
				if (destination == Connections[0].Name)
				{
					counter++;
					// EX H2->H3: 3 , 172.100.102.2 , 222, 172.100.103.2, 202, 0, content-message;
					int[] arr = new int[] { };
					NetworkMessage networkMessage = new NetworkMessage(howMany, NodeAddress, OutPort, Connections[0].IPAddress, DestPort,arr,80, message);
					Send(_socket, networkMessage.GetBytes());
					ViewOnScreen(networkMessage.MakeSentenceClient(NodeName, counter, Connections[0].IPAddress, Connections[0].Name));
					Thread.Sleep(5000);


				}
				if (destination == Connections[1].Name)
				{
					counter++;
					int[] arr = new int[] { };
					NetworkMessage networkMessage = new NetworkMessage(howMany, NodeAddress, OutPort, Connections[1].IPAddress, DestPort,arr,80, message);
					Send(_socket, networkMessage.GetBytes());
					ViewOnScreen(networkMessage.MakeSentenceClient(NodeName, counter, Connections[1].IPAddress, Connections[1].Name));
					Thread.Sleep(5000);



				}
				if (destination == Connections[2].Name)
				{
					counter++;
					int[] arr = new int[] { };
					NetworkMessage networkMessage = new NetworkMessage(howMany, NodeAddress, OutPort, Connections[2].IPAddress, DestPort, arr,80, message); 
					Send(_socket, networkMessage.GetBytes());
					ViewOnScreen(networkMessage.MakeSentenceClient(NodeName, counter, Connections[2].IPAddress, Connections[2].Name));
					Thread.Sleep(5000);

				}
				if (counter >= howMany)
				{
					counter = 0;
					break;
				}
					
			}
		}

		public async Task ClientListen()
		{
			while (true)
			{
				while (_socket == null || !_socket.Connected)
				{
					ViewOnScreen($"Retrying to establish connection: {NodeName} to {CloudAddress}:{CloudPort}");
					Thread.Sleep(2000);
					await Task.Run(()=>ConnectCC());
				}

				try
				{
					ViewOnScreen("Type (1) to send a message. Press any key to begin listening...");
					string choice = "";

					await Task.Run(() => choice = Console.ReadLine());
					
					if (choice == "1")
					{
						await Task.Run(() => SendMessage());
					}
					else
					{
						ViewOnScreen($"{NodeName} is listening !");
						await Task.Run(()=> Receive(_socket));
					}
				}
				catch (SocketException)
				{
					_socket.Shutdown(SocketShutdown.Both);
					_socket.Close();
					Console.ReadLine();
				}
			}
		}

		private static void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.  
				Socket client = (Socket)ar.AsyncState;

				// Complete the connection.  
				client.EndConnect(ar);
				ViewOnScreen($"{CustomerNodev2.readFromFile.NodeName} {CustomerNodev2.readFromFile.NodeAddress} established connection with CloudCable {client.RemoteEndPoint}");

				// Signal that the connection has been made.  
				connectDone.Set();
			}
			catch (Exception e)
			{
				ViewOnScreen(e.ToString());
				ViewOnScreen($"Exception. Can not connect to Cloud Cable! Retrying...");
				connectDone.Set();
			}
		}

		private void Receive(Socket client)
		{
			counter2 = 0;
			ViewOnScreen("Started waiting for a messages...");
			while (true)
			{
				try
				{
					receiveDone.Reset();
					client.BeginReceive(receive_buffer, 0, 64, 0, new AsyncCallback(ReceiveCallback), client);
					receiveDone.WaitOne();					
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				if (counter2 >= howMany2)
				{
					counter2 = 0;
					break;
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
					string str = Encoding.ASCII.GetString(receive_buffer);
					howMany2 = int.Parse(str.Substring(0, 1));
					counter2++;
					ViewOnScreen($"Received: message {counter2} of "+ Encoding.ASCII.GetString(receive_buffer));
									
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
			client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
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
		//bool SocketConnected(Socket s)
		//{
		//	bool part1 = s.Poll(1000, SelectMode.SelectRead);
		//	bool part2 = (s.Available == 0);
		//	if (part1 && part2)
		//		return false;
		//	else
		//		return true;
		//}


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
