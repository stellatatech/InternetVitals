using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace InternetVitals.Commands;

[Command(Name = "get-vitals", Description = "Gets the specified health vitals of your current network connection")]
public class InternetVitalsCommands
{
    private readonly ILogger _logger;

    public InternetVitalsCommands(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [Option(Description = "Get all metrics", ShortName = "a", LongName = "get-all-metrics")]
    public string GetAllMetrics { get; set; }

    public async Task<int> OnExecuteAsync()
    {
        if (String.IsNullOrEmpty(GetAllMetrics))
        {
            GetLocalIPAddress();
            Console.WriteLine("---------------------------------------");
            PingSystem();
            Console.WriteLine("---------------------------------------");
            return 0;
        }
        
        return 0;
    }

    private void GetNetworkConnectionStatus()
    {
        //Create ping object
        Ping netMon = new Ping();

        //Ping host (this will block until complete)
        PingReply response = netMon.Send("www.google.ca", 1000);

        if (response.Status == IPStatus.Success)
        {
            Console.WriteLine("Connection Status: Connected to internet");
        }
        else
        {
            Console.WriteLine("Connection Status: Error validating network connection");
        }
    }

    private void GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        Console.WriteLine("IP Addresses on {0}:", host.HostName);
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                Console.WriteLine(" -{0}", ip);
            }
            else if(ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                Console.WriteLine(" -{0}", ip.MapToIPv4());
            }
        }
    }
    
    private void PingSystem()
    {
        //Create ping object
        Ping netMon = new Ping();

        //Ping host (this will block until complete)
        PingReply response = netMon.Send("www.google.ca", 1000);

        Console.WriteLine("Ping Speed: {0}ms", response.RoundtripTime);
    } 
}