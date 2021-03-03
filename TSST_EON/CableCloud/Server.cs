using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using Utility;


namespace ServerNS
{
    public class Server
    {
		public class Log
		{
			private struct LogEntry
			{
				public DateTime time { get; set; }
				public string log { get; set; }
			}
			public Log()
			{
				Task.Run(() => WriteLog());
			}

			private List<LogEntry> log = new List<LogEntry>();
			public void AddLog(string _log)
			{
				log.Add(new LogEntry() { log = _log, time = DateTime.Now });
			}

			public void WriteLog()
			{
				while (true)
				{
					if (log.Count > 0)
					{
						LogEntry logen = log[0];
						log.RemoveAt(0);
						if (log.Count > 0)
						{
							Console.ForegroundColor = ConsoleColor.Green;
							Console.Write("[" + "{0:HH:mm:ss.fff}" + "] ", logen.time);
							Console.ResetColor();
							Console.WriteLine(logen.log);
						}
					}
					Thread.Sleep(10);
				}
			}
		}
		private int tcp_port { get; set; } = 9000;
		private ManualResetEvent begin_accept { get; set; } = new ManualResetEvent(false);
		//private LinksMap ports_map;
		private Log logs = new Log();
		private class SocketState
        {
			public Socket socket { get; set; }
			public string device { get; set; }
			//public List<int> ports { get; set; }
			public byte[] receive_buffer { get; set; }
			public ManualResetEvent begin_receive { get; set; } = new ManualResetEvent(false);

			public SocketState()
            {
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				receive_buffer = new byte[256];
				device = string.Empty;
            }

			public SocketState(Socket _socket)
            {
				socket = _socket;
				receive_buffer = new byte[256];
				device = string.Empty;
			}
			public SocketState(Socket _socket, string device)
			{
				socket = _socket;
				receive_buffer = new byte[256];
				this.device = device;
			}
		}

		private static Socket main_socket { get; set; } = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static List<SocketState> socket_states = new List<SocketState>();
		
		public Server(){}

		public void Start()
        {
			Console.Title = "Cable Cloud";
			main_socket.Bind(new IPEndPoint(IPAddress.Loopback, tcp_port));
            while (true)
            {
				ListenForConnection(main_socket);
	        }
        }

        private void ListenForConnection(Socket socket)
        {
            try
            {
            	socket.Listen(20);
				begin_accept.Reset();
				SocketState state = new SocketState(socket);
				socket.BeginAccept(256, new AsyncCallback(ConnectCallback), state);
				begin_accept.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine("Listen() error: " + e);
            }
        }

		private void ListenForMessage(SocketState state)
        {
			while (true)
			{
				try
				{
					state.begin_receive.Reset();
					state.socket.BeginReceive(state.receive_buffer, 0, 256, 0, new AsyncCallback(ReadCallback), state);
					state.begin_receive.WaitOne();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
        }

        private void ConnectCallback(IAsyncResult ar)
		{
			SocketState state = (SocketState)ar.AsyncState;
			byte[] buffer;
			state.socket = state.socket.EndAccept(out buffer, ar);
			state.device = string.Join("",Encoding.ASCII.GetString(buffer).Split());
			socket_states.Add(state);
			begin_accept.Set();
			//Console.WriteLine("Client added to the list: "+ state.device);
			socket_states.Add(state);
			Task.Run(() => ListenForMessage(state));
		}

		private void ReadCallback(IAsyncResult ar)
		{
			
			SocketState state = (SocketState)ar.AsyncState;
			int n_bytes = state.socket.EndReceive(ar);
			if(n_bytes > 0)
            {
				NetworkMessage packet = new NetworkMessage(state.receive_buffer);
				if (!packet.dst.Equals("DELETE"))
				{
					Disp.ViewOnScreen(packet.ToString());
					//logs.AddLog(packet.ToString());
					Socket socket = socket_states.Find(x => x.device.Equals(packet.dst)).socket;
					if(socket!=null) Send(socket, state.receive_buffer);
				}
			}

			state.receive_buffer = new byte[256];
			state.begin_receive.Set();
		}

		private void Send(Socket socket, byte[] data)
		{
			socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), socket);
		}

		private static void SendCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndSend(ar);
				//Console.WriteLine($"Sent message.");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

	}
}