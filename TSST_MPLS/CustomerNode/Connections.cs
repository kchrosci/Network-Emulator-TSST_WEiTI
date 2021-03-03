using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CustomerNode
{
	class Connections
	{		
		public string Name { get; set; }
		public IPAddress IPAddress { get; set; }

		public void Config(string config)
		{
			var par = config.Split();
			Name = par[0];
			IPAddress = IPAddress.Parse(par[1]);
		}
		public override string ToString()
		{
			return "Host: " + Name + "  IPAddress: " + IPAddress;
		}

	}
}
