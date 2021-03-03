using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace ManagementSystem
{
	class ManagementSystem
	{
        private string border_config_path { get; set; }
        private string forward_config_path { get; set; }
        private MplsConfiguration mpls_config { get; set; }

        public ManagementSystem(string bmc, string fmc)
        {
            border_config_path = bmc;
            forward_config_path = fmc;
            mpls_config = new MplsConfiguration(border_config_path, forward_config_path);
            ViewOnScreen("MPLS configuration added!");
            mpls_config.PrintConfig();
        }

		public void HandleNodes()
		{
            try
            {
                Int32 port = 7888;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                TcpListener mpls_server = new TcpListener(localAddr, port);
                mpls_server.Start();

                Byte[] bytes = new Byte[256];
                String data = null;

                while (true)
                {
                    TcpClient client = mpls_server.AcceptTcpClient();
                    ViewOnScreen("Node connected!");
                    NetworkStream stream = client.GetStream();
                    int i;

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        mpls_config = new MplsConfiguration(border_config_path, forward_config_path);//nie wiem czy tego nie ttrzeba przestawic gdzies wczesniej

                        data = null;
                        string node = Encoding.ASCII.GetString(bytes, 0, i);
                        
                        List<string> out_mess = new List<string>();
                        mpls_config.mplsBorderList.ForEach(delegate (MplsBorderEntry element)
                        {
                            if (element.node.Equals(node))
                            {
                                out_mess.Add(element.dst.ToString() + " ");
                                out_mess.Add(element.transport_layer_port.ToString() + " ");
                                out_mess.Add(element.label.ToString() + " ");
                                out_mess.Add(element.out_port.ToString() + ",");
                            }
                        });
                        if (out_mess.Count == 0)
                        {
                            out_mess.Add(",");
                        }

                        out_mess.Add("end ");
                        mpls_config.mplsForwardList.ForEach(delegate (MplsForwardEntry element)
                        {
                            if (element.node.Equals(node))
                            {
                                out_mess.Add(element.idx.ToString() + " ");
                                out_mess.Add(element.inc_port.ToString() + " ");
                                out_mess.Add(element.label.ToString() + " ");
                                out_mess.Add(element.op.ToString() + " ");
                                out_mess.Add(element.out_port.ToString() + " ");
                                out_mess.Add(element.out_label.ToString() + " ");
                                out_mess.Add(element.last_idx.ToString() + ",");
                            }
                        });
                        data = string.Concat(out_mess);

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        stream.Write(msg, 0, msg.Length);
                        ViewOnScreen("Sent data to node!");
                    }

                    client.Close();
                }
            }
			catch (SocketException e)
			{
				ViewOnScreen(e.Message + "\n" + "\t\tHandleNodes() failed!");
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
