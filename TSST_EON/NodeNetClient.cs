using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Utility;
using System.Text;

namespace NetClientNS 
{ 
	public class NetClient
	{
		private int server_port { get; set; } = 9000;
		private Socket main_socket { get; set; } = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private byte[] receive_buffer { get; set; } = new byte[256];
		private	ManualResetEvent sendDone = new ManualResetEvent(false);
		private ManualResetEvent receiveDone = new ManualResetEvent(false);
		private Func<byte[], byte[]> HandleMessage; 
		
		public NetClient(Func<byte[], byte[]> handle_message_func, string device_name)
        {
			main_socket.Connect(new IPEndPoint(IPAddress.Loopback, server_port));
			Send(Encoding.ASCII.GetBytes(device_name));
			HandleMessage = handle_message_func;
		}

		public void ListenForMessages()
		{
			while (true)
			{
				try
				{
					receiveDone.Reset();
					main_socket.BeginReceive(receive_buffer, 0, 256, 0, new AsyncCallback(ReceiveCallback), main_socket);
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
					byte[] msg = HandleMessage(receive_buffer);	
					Send(msg);
					receiveDone.Set();
				}
			}
			catch (Exception e)
			{
				Disp.ViewOnScreen(e.ToString());
			}
		}

		public void Send(byte[] byteData)
		{
			sendDone.Reset();
			main_socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), main_socket);
			sendDone.WaitOne();
		}

		private void SendCallback(IAsyncResult ar)
		{
			try
			{
				Socket client = (Socket)ar.AsyncState;
				int bytesSent = client.EndSend(ar);
				sendDone.Set();
			}
			catch (Exception e)
			{
				Disp.ViewOnScreen(e.ToString());
			}
		}
	}		
}