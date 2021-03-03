//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Xml;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Globalization;

//namespace CustomerNode
//{

//	// Client app is the one sending messages to a Server/listener.   
//	// Both listener and client can send messages back and forth once a   
//	// communication is established.  
//	public class CustomerNode
//	{
//		public class OutPort
//		{
//			public int port;
//			IPAddress ipAddress;
//			public void StartClient(string message)
//			{
//				byte[] bytes = new byte[1024];

//				try
//				{
//					// Connect to a Remote server  
//					// Get Host IP Address that is used to establish a connection  
//					// In this case, we get one IP address of localhost that is IP : 127.0.0.1  
//					// If a host has multiple addresses, you will get a list of addresses  
//					//IPHostEntry host = Dns.GetHostEntry("localhost");  
//					//IPAddress ipAddress = host.AddressList[0];  
//					IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

//					// Create a TCP/IP  socket.    
//					Socket sender = new Socket(ipAddress.AddressFamily,
//						SocketType.Stream, ProtocolType.Tcp);

//					// Connect the socket to the remote endpoint. Catch any errors.    
//					try
//					{
//						// Connect to Remote EndPoint  
//						sender.Connect(remoteEP);

//						Console.WriteLine("OutPort:Socket connected to {0}",
//							sender.RemoteEndPoint.ToString());


//						// Encode the data string into a byte array.    
//						byte[] msg = Encoding.ASCII.GetBytes(" " + message + "<EOF>");

//						// Send the data through the socket.    
//						int bytesSent = sender.Send(msg);

//						// Receive the response from the remote device.    
//						int bytesRec = sender.Receive(bytes);
//						Console.WriteLine("OutPort:Echoed test = {0}",
//							Encoding.ASCII.GetString(bytes, 0, bytesRec));

//						// Release the socket.    
//						sender.Shutdown(SocketShutdown.Both);
//						sender.Close();

//					}
//					catch (ArgumentNullException ane)
//					{
//						Console.WriteLine("OutPort:ArgumentNullException : {0}", ane.ToString());
//					}
//					catch (SocketException se)
//					{
//						Console.WriteLine("OutPort:SocketException : {0}", se.ToString());
//					}
//					catch (Exception e)
//					{
//						Console.WriteLine("OutPort:Unexpected exception : {0}", e.ToString());
//					}

//				}
//				catch (Exception e)
//				{
//					Console.WriteLine(e.ToString());
//				}
//			}

//			public OutPort(int port, IPAddress ipAddress)
//			{
//				this.port = port;
//				this.ipAddress = ipAddress;
//			}
//		}

//		public class InPort
//		{
//			public int port;
//			IPAddress ipAddress;
//			public void StartServer()
//			{
//				// Get Host IP Address that is used to establish a connection  
//				// In this case, we get one IP address of localhost that is IP : 127.0.0.1  
//				// If a host has multiple addresses, you will get a list of addresses  
//				//IPHostEntry host = Dns.GetHostEntry("localhost");  
//				//IPAddress ipAddress = host.AddressList[0];  
//				IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);


//				try
//				{

//					// Create a Socket that will use Tcp protocol      
//					Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//					// A Socket must be associated with an endpoint using the Bind method  
//					listener.Bind(localEndPoint);
//					// Specify how many requests a Socket can listen before it gives Server busy response.  
//					// We will listen 10 requests at a time  
//					listener.Listen(10);

//					Console.WriteLine("InPort:Waiting for a connection...");
//					Socket handler = listener.Accept();

//					// Incoming data from the client.    
//					string data = null;
//					byte[] bytes = null;

//					while (true)
//					{
//						bytes = new byte[1024];
//						int bytesRec = handler.Receive(bytes);
//						data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
//						if (data.IndexOf("<EOF>") > -1)
//						{
//							break;
//						}
//					}

//					Console.WriteLine("InPort: Text received : {0}", data);

//					byte[] msg = Encoding.ASCII.GetBytes(data);
//					handler.Send(msg);
//					handler.Shutdown(SocketShutdown.Both);
//					handler.Close();
//				}
//				catch (Exception e)
//				{
//					Console.WriteLine(e.ToString());
//				}

//				Console.WriteLine("\n InPort: Press any key to continue...");
//				Console.ReadKey();
//			}

//			public InPort(int port, IPAddress ipAddress)
//			{
//				this.port = port;
//				this.ipAddress = ipAddress;
//			}
//		}

//		public static int Main(string[] args)
//		{
//			string[] objects = new string[2];
//			DateTime time = DateTime.Now;

//			int[] ports = new int[2];
//			IPAddress[] addresses = new IPAddress[2];
//			int i = 0;
//			int j = 0;
//			int k = 0;
//			string path;
//			path = args[0];
//			using (XmlReader reader = XmlReader.Create(path))
//			{
//				while (reader.Read())
//				{
//					if (reader.IsStartElement())
//					{
//						//return only when you have START tag  

//						switch (reader.Name.ToString())
//						{
//							case "Name":
//								objects[i] = reader.ReadString();
//								i++;
//								break;
//							case "Location":
//								ports[j] = short.Parse(reader.ReadString());
//								j++;
//								break;
//							case "Address":
//								addresses[k] = IPAddress.Parse(reader.ReadString());
//								k++;
//								break;
//						}
//					}
//					Console.WriteLine("");

//				}
//			}
//			//ports[0] - NUMER PORTU CHMURY
//			//ports[1] - NUMER PORTU HOSTA   
//			//addresses[0] - NUMER ADRESU IP CHMURY
//			//addresses[1] - NUMER ADRESU IP HOSTA   
//			Console.WriteLine(objects[1]);
//			Console.WriteLine(time.ToString("h:mm:ss tt"));
//			InPort port_we = new InPort(ports[1], addresses[1]);
//			OutPort port_wy = new OutPort(ports[0], addresses[0]);
//			Console.WriteLine("Main thread: Start a second thread.");
//			// The constructor for the Thread class requires a ThreadStart
//			// delegate that represents the method to be executed on the
//			// thread.  C# simplifies the creation of this delegate.
//			Thread t = new Thread(new ThreadStart(port_we.StartServer));
//			t.Start();
//			Thread.Sleep(1000);
//			Console.WriteLine("\n OutPort: Press any key to send a message...");
//			Console.ReadKey();
//			Console.WriteLine("Message: ");
//			string message;
//			message = Console.ReadLine();
//			port_wy.StartClient(message);

//			return 0;
//		}


//	}
//}