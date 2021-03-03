using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkNode
{
	class NetworkNodeLauncher
	{
		
		public async static Task Main(string[] args)
		{
			//Create networknode object and read its parameters.
			NetworkNode networkNode = new NetworkNode();

			#region Start by .bat
			try
			{	
				await Task.Run(() => networkNode = networkNode.Start(args[0]));
				//await Task.Run(() => networkNode = networkNode.Start($"..\\..\\..\\..\\Configs\\NetworkConfig1.txt"));
			}
			catch (ArgumentNullException e)
			{
				Console.WriteLine(e.Message);
			}

			Task.Run(() => networkNode.ReconfigMpls());
			await Task.Run(() => networkNode.StartConnections(networkNode));
			Console.ReadLine();
			#endregion
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
