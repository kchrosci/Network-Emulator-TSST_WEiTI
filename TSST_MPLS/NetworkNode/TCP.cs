using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
	public class TCP
	{
		
			// Client socket.  
	public Socket workSocket = null;
			// Size of receive buffer.  
	public const int BufferSize = 256;
			// Receive buffer.  
	public byte[] buffer = new byte[BufferSize];
			// Received data string.  
	public StringBuilder sb = new StringBuilder();
	public static string response = string.Empty;
		

		//public void ReceivePacket()
		//{
		//	socket = _socket;
		//	socket.ReceiveBufferSize = dataBufferSize;
		//	socket.SendBufferSize = dataBufferSize;

		//	stream = socket.GetStream();
		//	receiveBuffer = new byte[dataBufferSize];

		//	stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

		//}

		//private void ReceiveCallback(IAsyncResult ar)
		//{
		//	try
		//	{
		//		int _byteLength = stream.EndRead(ar);
		//		if (_byteLength <= 0) return;

		//		byte[] _data = new byte[_byteLength];
		//		Array.Copy(receiveBuffer, _data, _byteLength);

		//		stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
		//	}
		//	catch
		//	{
		//		ViewOnScreen("Error while receiving TCP data.");
		//	}
		//}

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
