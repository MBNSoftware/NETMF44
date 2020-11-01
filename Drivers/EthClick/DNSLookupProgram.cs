using MBN;
using MBN.Modules;
using Microsoft.SPOT;

namespace NetworkingExample
{
	public class DNSLookupProgram
	{
		private static EthClick _eth;

		public static void Main()
		{

			/* Use this for Static IP and No DHCP */
			//_eth.Name = "Quail";
			//_eth.MacAddress = _eth.GenerateUniqueMacAddress("Quail");
			//_eth.IPAddress = "192.168.1.95".ToBytes();
			//_eth.DefaultGateway = "192.168.1.1".ToBytes();
			//_eth.PreferredDomainNameServer = "8.8.8.8".ToBytes(); // Google DNS Servers
			//_eth.AlternateDomainNameServer = "8.8.8.4".ToBytes(); // Google DNS Servers
			//_eth.DHCPDisabled = true;

			_eth = new EthClick(Hardware.SocketOne);

			_eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet");

			while (true)
			{
				if (_eth.ConnectedToInternet)
				{
					Debug.Print("Connected to Internet");
					break;
				}
				Debug.Print("Waiting on Internet connection");
			}

			var addressBytes = DNS.Lookup("www.google.com");
			Debug.Print("DNS Lookup: www.google.com -> " + addressBytes.ToAddress());
			addressBytes = DNS.Lookup("www.mikrobusnet.org");
			Debug.Print("DNS Lookup: www.mikrobusnet.org -> " + addressBytes.ToAddress());
		}
	}
}
