using System;
using System.IO;
using System.Threading;
using MBN;
using MBN.Modules;
using Microsoft.SPOT;

namespace NetworkingExample
{
    class HttpProgram
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

			var r = new HttpRequest("http://services.odata.org/V2/(S(fhsma0h0orzoqo55jjrw1wyq))/OData/OData.svc/");
		 
            r.Headers.Add("Accept", "*/*");

            var response = r.Send();

	        if (response != null)
	        {
		        Debug.Print("Response: " + response.Message);
	        }
	        else
	        {
		        Debug.Print("No response");
	        }

			/* Alternate example using a different service */
			//const int minVal = 0;
			//const int maxVal = 100;

			//string apiUrl = @"http://www.random.org/integers/?num=1"
			//	+ "&min=" + minVal + "&max=" + maxVal
			//	+ "&col=1&base=10&format=plain&rnd=new";

			//var request = new HttpRequest(apiUrl);
			//request.Headers.Add("Accept", "*/*");

			//HttpResponse response = request.Send();

			if (response != null)
			{
				Debug.Print("Random number: " + response.Message.Trim());
			}
			else
			{
				Debug.Print("No response");
			}

	        Thread.Sleep(Timeout.Infinite);
        }
    }
}
