using System.Collections;
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
            Console.WriteLine("Retrieving all data");
            Console.WriteLine("---------------------------------------");
            GetNetworkConnectionStatus();
            Console.WriteLine("---------------------------------------");
            GetLocalIPAddress();
            Console.WriteLine("---------------------------------------");
            PingSystem();
            Console.WriteLine("---------------------------------------");
            await GetDownloadSpeed();
            return 0;
        }
        
        return 0;
    }

    private async Task GetDownloadSpeed()
    {
        var httpClient = new HttpClient();
        
        Console.WriteLine("Downloading file....");

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        int size = 0;
        double[] times = Array.Empty<double>();
        while (sw.ElapsedMilliseconds < 20000)
        {
            System.Diagnostics.Stopwatch responseTime = System.Diagnostics.Stopwatch.StartNew();
            var response =
                await httpClient.GetByteArrayAsync("https://www.apple.com/legal/sla/docs/iOS7.pdf",
                    CancellationToken.None);
            responseTime.Stop();
            size += response.Length;
            times = times.Append(responseTime.Elapsed.TotalMilliseconds).ToArray();
        }
        sw.Stop();
        times = times.Where(x => x != times[0]).ToArray();

        var avg = times.Average(x => x);
        avg *= times.Length -1;
        var time = avg / 1000.0;
        var conversion = 125000.0;
        var convertedSize = size / conversion;
        
        var speed = convertedSize / time;

        Console.WriteLine("Download duration (seconds): {0}", time);
        Console.WriteLine("Speed: {0:N0} mbps ", speed);
        
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
        string localIPAddress = "";
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket?.LocalEndPoint as IPEndPoint ?? throw new InvalidOperationException("Cannot read connection data");
            localIPAddress = endPoint?.Address?.ToString() ?? throw new ArgumentNullException(nameof(localIPAddress), "Cannot determine IP address");
            Console.WriteLine("Your device's IP Address: {0}", localIPAddress);
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