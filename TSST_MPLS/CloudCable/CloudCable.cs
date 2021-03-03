using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using NetworkMessageNamespace;
using LogNamespace;

namespace CloudCable
{
	public struct Link
    {
		public int one;
		public int two;

		public void Config(string config)
        {
			string[] par = config.Split();
			this.one = int.Parse(par[0]);
			this.two = int.Parse(par[1]);
        }

		public override string ToString()
        {
			return one + " " + two;	
        }
    }

	public struct port_map
    {
		public int port { get; set; }
		public string name { get; set; }
    }

	public class SocketState
	{
		public Socket socket { get; set; }
		public string name { get; set; }
		public byte[] receive_buffer { get; set; }

		public ManualResetEvent begin_receive { get; set; } = new ManualResetEvent(false);

		public SocketState()
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			receive_buffer = new byte[64];
		}

		public SocketState(Socket _socket)
		{
			socket = _socket;
			receive_buffer = new byte[64];
		}
	}

	class CloudCable
	{
		private static int sim_port { get; set; } = 9000;
		private static ManualResetEvent begin_accept { get; set; } = new ManualResetEvent(false);

		private List<port_map> port_maps = new List<port_map>();		

		public Log logs { get; set; } = new Log();

		private static Socket main_socket { get; set; } = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static List<SocketState> socket_states = new List<SocketState>();
		private static List<Link> links = new List<Link>();

		public CloudCable(string FilePath)
		{
			ViewOnScreen("CloudCable is created!");
			Console.Title ="CloudCable";
			try
			{
				List<string> LoadedFile = File.ReadAllLines(FilePath).ToList();
				string[] links_string = GetFromFile(LoadedFile, "LINK").Split(", ");
				string[] ports_string = GetFromFile(LoadedFile, "PORTS").Split(", ");
				foreach (string el in links_string)
				{
					Link link = new Link();
					link.Config(el);
					links.Add(link);
				}
				foreach (string el in ports_string)
				{
					string[] conf = el.Split(" ");
					port_map temp = new port_map();
					temp.name = conf[1];
					temp.port = int.Parse(conf[0]);
					port_maps.Add(temp);
				}

				ViewOnScreen("Connections loaded:");
				foreach(Link link in links)
                {
					Console.WriteLine(link.ToString());
                }
				
			}
			catch (FileNotFoundException e)
			{
				ViewOnScreen(e.Message);
			}

			main_socket.Bind(new IPEndPoint(IPAddress.Loopback, sim_port));
		}

		public void ListenForConnection()
		{
			try
			{
				logs.AddLog(DateTime.Now, "Listen for connections..");
				main_socket.Listen(122);
				begin_accept.Reset();
				SocketState state = new SocketState(main_socket);
				main_socket.BeginAccept(64, new AsyncCallback(ConnectCallback), state);
				begin_accept.WaitOne();
			}
			catch (Exception e)
			{
				ViewOnScreen("Listen() error: " + e);
			}
		}

		private void ListenForMessage(SocketState state)
		{
			while (true)
			{
				try
				{
					state.begin_receive.Reset();
					state.socket.BeginReceive(state.receive_buffer, 0, 64, 0, new AsyncCallback(ReadCallback), state);
					state.begin_receive.WaitOne();
				}
				catch (Exception e)
				{
					logs.AddLog(DateTime.Now,e.ToString());

				}
			}
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			SocketState state = (SocketState)ar.AsyncState;
			byte[] buffer;
			state.socket = state.socket.EndAccept(out buffer, ar);
			NetworkMessage content = new NetworkMessage(buffer);
			state.name = content.message;
			socket_states.Add(state);
			logs.AddLog(DateTime.Now, "Added socket: " + content.message);
			begin_accept.Set();
			Task.Run(() => ListenForMessage(state));
		}

		private void ReadCallback(IAsyncResult ar)
		{
			SocketState state = (SocketState)ar.AsyncState;
			int n_bytes = state.socket.EndReceive(ar);
			
			if (n_bytes > 0)
			{
				//czek czy ta gowno wiadomosc sie nie robi
				string check = Encoding.ASCII.GetString(state.receive_buffer).Split()[0];
				int cos;


				if (int.TryParse(check,out cos))
				{
					NetworkMessage content = new NetworkMessage(state.receive_buffer);
					logs.AddLog(DateTime.Now, "Message receive: " + content.GetString());
					Socket handler = socket_states.Find(x => x.name.Equals(port_maps.Find(y => y.port == content.dst_port).name)).socket;

					if (CheckLink(content.src_port, content.dst_port))
					{
						Send(handler, content.GetBytes());
						logs.AddLog(DateTime.Now, "Message sent: " + content.GetString());
					}
					else
					{
						logs.AddLog(DateTime.Now, " There is no link: " + content.src_port + " " + content.dst_port);
					}
				}
				
			}
			state.receive_buffer = new byte[64];
			state.begin_receive.Set();
		}

		private void Send(Socket socket, byte[] data)
		{
			socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), socket);
		}

		private void SendCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndSend(ar);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		private static bool CheckLink(int node1, int node2)
        {
			return links.Contains(new Link() { one = node1, two = node2 }) | links.Contains(new Link() { one = node2, two = node1 });
        }

		public void ConfigLinks()
		{
			while (true)
			{
				string[] config_string = Console.ReadLine().Split();
				int one = int.Parse(config_string[0]);
				int two = int.Parse(config_string[1]);
				if (config_string.Length == 2)
				{
					if (CheckLink(one, two))
					{
						links.RemoveAt(links.FindIndex(x => (x.one==one && x.two==two) || (x.one==two && x.two==one)));
						ViewOnScreen("Link removed!");
						ViewOnScreen("List of links:");
						foreach (Link link in links)
						{
							Console.WriteLine(link.ToString());
						}
					}
					else
					{
						links.Add(new Link() { one = one, two = two});
						ViewOnScreen("Link added!");
						ViewOnScreen("List of links:");
						foreach (Link link in links)
						{
							Console.WriteLine(link.ToString());
						}
					}
				}
				else
					continue;
				

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
		#region GetFromFile Method
		/// <summary>
		/// Simple function to replace strings while loading from file.
		/// </summary>
		/// <param name="msg"></param>
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
        #endregion
    }
}
