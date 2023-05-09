using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace NetVitals.Commands;

[Command(Name = "monitor", Description = "NetVitals is a command line application built to monitor networking speed and collect information about the local network configuration.")]
public class NetVitalsCommands
{
    private String programName = "NetVitals";
    private String programVersion = "1.0.3";

    private readonly ILogger _logger;
    private readonly HttpClient _client = new HttpClient {Timeout = TimeSpan.FromSeconds(120)};
    private bool isConnected;

    public NetVitalsCommands(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [Option(Description = "Collect all available metrics", ShortName = "a", LongName = "all")]
    public bool GetAllMetrics { get; set; }
    
    [Option(Description = "Validates connection request", ShortName = "c", LongName = "connect")]
    public bool GetConnectionStatus { get; set; }
    
    [Option(Description = "Collect internal IP Address", ShortName = "i", LongName = "internal-ip")]
    public bool GetActiveIPAddress { get; set; }

    [Option(Description = "Collect external IP Address", ShortName = "e", LongName = "external-ip")]
    public bool GetPublicIPAddress { get; set; }
    
    [Option(Description = "Get ping speed", ShortName = "p", LongName = "ping")]
    public bool GetPingSpeed { get; set; }
    
    [Option(Description = "Get internet stats for all network adapters on the device", ShortName = "s", LongName = "net-stats")]
    public bool GetAllInternetStats { get; set; }
    
    [Option(Description = "Test download speed", ShortName = "d", LongName = "download")]
    public bool GetInternetDownloadSpeed { get; set; }

    [Option(Description = "NetVitals version information", ShortName = "v", LongName = "version")]
    public bool GetVersionInformation { get; set; }

    private void divider()
    {
        string lineDivider = new string('-', 40);
        Console.WriteLine(lineDivider);
    }

    public async Task<int> OnExecuteAsync()
    {

        if (GetAllMetrics)
        {
            Console.WriteLine("\n");
            divider();
            VersionInformation();
            divider();
            
            Console.WriteLine("\nNetwork Information");
            divider();
            GetNetworkConnectionStatus();
            divider();
            GetInternalIPAddress();
            divider();
            GetExternalIPAddress();
            divider();
            PingSystem();
            divider();
            GetInternetStats();
            await GetRealTimeDownloadSpeed();
            return 0;
        }

        if (GetVersionInformation)
        {
            divider();
            VersionInformation();
            divider();
        }

        if (GetConnectionStatus)
        {
            GetNetworkConnectionStatus();
        }
        
        if (GetActiveIPAddress)
        {
            GetInternalIPAddress();
        }

        if (GetPublicIPAddress)
        {
            GetExternalIPAddress();
        }
        
        if (GetPingSpeed)
        {
            PingSystem();
        }
        
        if (GetAllInternetStats)
        {
            GetInternetStats();
        }
        
        if (GetInternetDownloadSpeed)
        {
            await GetRealTimeDownloadSpeed();
        }

        return 0;
    }

    private void VersionInformation()
    {
        Console.WriteLine($"{programName} Version: {programVersion}");
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
            try
            {
                var url = "https://www.apple.com/legal/sla/docs/iOS7.pdf";
                var currentBytes = currentAdapter.GetIPv4Statistics().BytesReceived;
                var sw = Stopwatch.StartNew();
                
                await _client.GetAsync(url, CancellationToken.None);
                sw.Stop();
                
                var newBytes = currentAdapter.GetIPv4Statistics().BytesReceived;
                var byteDifference = (newBytes - currentBytes) / 125000.0;
                var timeDifference = (sw.Elapsed.TotalMilliseconds - 35) / 1000.0;
                var speed = byteDifference / timeDifference;

                avgs = avgs.Append(speed).ToArray();
                Console.WriteLine("Speed Test {1}: {0} mbps", speed, i);
                i++;
            }
            catch (HttpRequestException)
            {
                isConnected = false;
                
                if (!isConnected)
                {
                    Console.WriteLine("Speed Test: Cannot gather information in offline mode");
                }
                
                break;
            }
        }

        try
        {
            var avgSpeed = avgs.Average(x => x);
            Console.WriteLine("Average speed {0} mbps", avgSpeed);
        }
        catch (InvalidOperationException)
        {
            isConnected = false;
        }
    }

    private void GetInternetStats()
    {
        
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        var activeAdapters = adapters.Where(x=> x.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                               && x.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                                               && x.OperationalStatus == OperationalStatus.Up 
                                               && x.Name.StartsWith("vEthernet") == false);
        
        foreach (NetworkInterface adapter in activeAdapters)
        {
            IPInterfaceProperties properties = adapter.GetIPProperties();
            IPv4InterfaceStatistics stats = adapter.GetIPv4Statistics();
            
            Console.WriteLine("\nAdapter Details: Name: {0} | Description: {1} | NetworkInterfaceType: {2} | Status: {3}", adapter.Name, adapter.Description, adapter.NetworkInterfaceType, adapter.OperationalStatus);
            Console.WriteLine("Speed: {0} mbps", adapter.Speed / 1000000);
            Console.WriteLine("IP Address(es):");

            int addressNum = 0;
            
            foreach (var unicastIpAddressInformation in properties.UnicastAddresses)
            {
                addressNum++;
                Console.WriteLine($"{addressNum}. {unicastIpAddressInformation.Address.MapToIPv4()}");
            }
            
            Console.WriteLine("\nBytes Received: {0}", stats.BytesReceived);
            Console.WriteLine("Bytes Sent: {0}", stats.BytesSent);

            divider();
        }
    }
    
    private void GetNetworkConnectionStatus()
    {
        try
        {
            // Create ping object
            Ping netMon = new Ping();

            // Ping host (this will block until complete)
            PingReply response = netMon.Send("www.google.ca", 1000);

            var internetStatus = $"Connection Status: {response.Status}";
            Console.WriteLine(internetStatus);

            isConnected = true;
        }
        catch (PingException)
        {
            isConnected = false;

            if (!isConnected)
            {
                var internetStatus = "Connection Status: Offline";
                Console.WriteLine(internetStatus);
            }
        }
    }

    private void GetInternalIPAddress()
    {
        string internalIPAddress = "";
        
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            try
            {   
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket?.LocalEndPoint as IPEndPoint ?? throw new InvalidOperationException("Cannot read connection data");
                internalIPAddress = endPoint?.Address?.ToString() ?? throw new ArgumentNullException(nameof(internalIPAddress), "Cannot determine internal IP address");
                isConnected = true;

                Console.WriteLine("Internal IP Address: {0}", internalIPAddress);
            }
            catch (SocketException)
            {
                isConnected = false;

                if (!isConnected)
                {
                    Console.WriteLine("Internal IP Address: Cannot gather information in offline mode");
                }
            }
        }
    }

    private void GetExternalIPAddress()
    {
        try
        {
            // TODO: Convert WebClient => HTTPClient
            string externalIPString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            var externalIPAddress = IPAddress.Parse(externalIPString);
            var externalIPAddressStr = externalIPAddress.ToString() ?? throw new ArgumentNullException(nameof(externalIPAddress), "Cannot determine IP address");
            Console.WriteLine("External IP Address: {0}", externalIPAddressStr);
        }
        catch (WebException)
        {
            isConnected = false;

            if (!isConnected)
            {
                Console.WriteLine("External IP Address: Cannot gather information in offline mode");
            }
        }
    }
    
    private void PingSystem()
    {
        try
        {
            // Create ping object
            Ping netMon = new Ping();

            // Ping host (this will block until complete)
            PingReply response = netMon.Send("www.google.ca", 1000);

            isConnected = true;

            Console.WriteLine("Ping Speed: {0}ms", response.RoundtripTime);
        }
        catch (PingException)
        {
            isConnected = false;

            if (!isConnected)
            {
                Console.WriteLine("Ping Speed: Cannot gather information in offline mode");
            }
        }
    } 
}
