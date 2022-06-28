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
        const string tempfile = "tempfile.tmp";
        FileInfo fileInfoTemp = new FileInfo(tempfile);
        fileInfoTemp.Delete();
        var httpClient = new HttpClient{DefaultRequestHeaders = {}};
        
        Console.WriteLine("Downloading file....");

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        var response =
            await httpClient.GetByteArrayAsync("http://dl.google.com/googletalk/googletalk-setup.exe",
                CancellationToken.None);
        sw.Stop();
        
        double speed = response.Count() / sw.Elapsed.TotalSeconds/ 1000000;
        

        /* WebClient webClient = new WebClient();

        Console.WriteLine("Downloading file....");

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        webClient.DownloadFile("http://dl.google.com/googletalk/googletalk-setup.exe", tempfile);
        sw.Stop();

        FileInfo fileInfo = new FileInfo(tempfile);
        Console.WriteLine("length of file, {0}",fileInfo.Length);
        var speed = fileInfo.Length / sw.Elapsed.TotalSeconds;

        Console.WriteLine("Download duration: {0}", sw.Elapsed);
        Console.WriteLine("File size: {0}", fileInfo.Length.ToString("N0"));
        Console.WriteLine("Speed: {0} bps ", speed.ToString("N0"));

        Console.WriteLine("Press any key to continue...");
        Console.ReadLine();
        */
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