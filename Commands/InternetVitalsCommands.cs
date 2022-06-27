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
        GetLocalIPAddress();
        PingSystem();
        return 0;
    }
    
    public static void GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                Console.WriteLine("IPV4 Address: {0}", ip);
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