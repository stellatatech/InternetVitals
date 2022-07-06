using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace InternetVitals.Commands;

[Command(Name = "get-vitals", Description = "Internet Vitals is a console application built to monitor networking speed and information about the local network.")]
public class InternetVitalsCommands
{
    private readonly ILogger _logger;
    private readonly HttpClient _client = new HttpClient {Timeout = TimeSpan.FromSeconds(120)};

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
            GetInternetStats();
            Console.WriteLine("---------------------------------------");
            await GetRealTimeDownloadSpeed();
            //await GetDownloadSpeed();
            return 0;
        }
        
        return 0;
    }

    private async Task GetRealTimeDownloadSpeed()
    {
        //Get all adapters
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        
        //Get the ones that are running and in use
        var activeAdapters = adapters.Where(x=> x.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                                && x.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                                                && x.OperationalStatus == OperationalStatus.Up 
                                                && x.Name.StartsWith("vEthernet") == false).ToList();
        
        //Get the one doing the most work, that is likely the active one
        NetworkInterface currentAdapter = activeAdapters.First();
        foreach (var networkInterface in activeAdapters)
        {
            if (networkInterface.GetIPv4Statistics().BytesReceived > currentAdapter.GetIPv4Statistics().BytesReceived)
            {
                currentAdapter = networkInterface;
            }
        }
        
        //Calculate internet speed
        int i = 0;
        double[] avgs = new double[0];
        
        while (i <= 30)
        {
            var url = "https://www.apple.com/legal/sla/docs/iOS7.pdf";
            var currentBytes = currentAdapter.GetIPv4Statistics().BytesReceived;
            var sw = Stopwatch.StartNew();
            await _client.GetAsync(url,
                CancellationToken.None);
            sw.Stop();
            var newBytes = currentAdapter.GetIPv4Statistics().BytesReceived;
            var byteDifference = (newBytes - currentBytes)/125000.0;
            var timeDifference = (sw.Elapsed.TotalMilliseconds -35)/1000.0;
            var speed = byteDifference / timeDifference;
            avgs = avgs.Append(speed).ToArray();
            Console.WriteLine("Speed Test {1}: {0} mbps", speed, i);
            i++;
        }

        var avgSpeed = avgs.Average(x => x);
        Console.WriteLine("Average speed {0} mbps", avgSpeed);
    }

    private void GetInternetStats()
    {
        Console.WriteLine("Getting network adapter information");
        
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        var activeAdapters = adapters.Where(x=> x.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                               && x.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                                               && x.OperationalStatus == OperationalStatus.Up 
                                               && x.Name.StartsWith("vEthernet") == false);
        
        foreach (NetworkInterface adapter in activeAdapters)
        {
            IPInterfaceProperties properties = adapter.GetIPProperties();
            IPv4InterfaceStatistics stats = adapter.GetIPv4Statistics();
            Console.WriteLine("Adapter details: Name: {0}, Description: {1}, NetworkInterfaceType: {2}, Status: {3}",adapter.Name, adapter.Description, adapter.NetworkInterfaceType, adapter.OperationalStatus);
            Console.WriteLine("Speed: {0} mbps",
                adapter.Speed/1000000);
            Console.WriteLine("IP Addresses:");
            foreach (var unicastIpAddressInformation in properties.UnicastAddresses)
            {
                Console.WriteLine(" -{0}", unicastIpAddressInformation.Address.MapToIPv4());
            }
            Console.WriteLine("Bytes Received: {0}", stats.BytesReceived);
            Console.WriteLine("Bytes Sent: {0}", stats.BytesSent);
            Console.WriteLine("---------------------------------------");
        }
    }

    private void RetryNetworkConnection()
    {

        // Create ping object
        Ping netMon = new Ping();

        try
        {
            // Ping host (this will block until complete)
            PingReply response = netMon.Send("www.google.ca", 1000);

            // Temp variable to offset issue with catch (requires internet connection)
            var internetStatus = $"Connection Status: {response.Status}";
            Console.WriteLine(internetStatus);
        }
        catch (PingException e)
        {
            int tryNum = 0;

            while (tryNum < 10)
            {
                Thread.Sleep(2000);
                // Ping host (this will block until complete)
                PingReply response = netMon.Send("www.google.ca", 1000);
                tryNum++;
                Console.WriteLine($"Retrying connection...{tryNum}");

                if (response.Status == IPStatus.Success)
                {
                    var internetStatus = $"Connection Status: {response.Status}";
                    Console.WriteLine(internetStatus);
                    break;
                }

                if (tryNum == 9)
                {
                    Console.WriteLine($"Try limit exceeded. Please refer to the error below:\n {e}");
                    break;
                }
            }
        }
    }
    
    private void GetNetworkConnectionStatus()
    {
        RetryNetworkConnection();
        var internetStatus = $"Connection Status: {response.Status}";
        Console.WriteLine(internetStatus);
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