using System;
using System.Threading.Tasks;

namespace CloudCable
{
	class CloudCableLauncher
	{
		public async static Task Main(string[] args)
		{
            try 
			{ 
				CloudCable cc = new CloudCable(args[0]);
				//CloudCable cc = new CloudCable($"..\\..\\..\\..\\Configs\\CloudConfig.txt");
				Task.Run(() => cc.logs.WriteLog());
				Task.Run(() => cc.ConfigLinks());
				while (true)
				{
					await Task.Run(() => cc.ListenForConnection());
				}
			}
			catch (ArgumentNullException e)
			{
				Console.WriteLine(e.Message);
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
