// using System.ComponentModel.DataAnnotations;
using InternetVitals.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace InternetVitals;

[Command(Name = "internetvitals")]
[Subcommand(typeof(InternetVitalsCommands))]
class Program
{
    private static ILogger _logger;
    public static async Task<int> Main(string[] args)
    {
        
        Log.Logger = new LoggerConfiguration()
            .WriteTo
            .Console()
            .CreateLogger();
        var serilogFactory = new SerilogLoggerFactory(Log.Logger);
        _logger = serilogFactory.CreateLogger(nameof(Program));

        try
        {
            var provider = ConfigureServices();
            var app = new CommandLineApplication<Program>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(provider);

            return await app.ExecuteAsync(args);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.ToString());
            return -1;
        }
    }
    
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddTransient(s => _logger);

        return services.BuildServiceProvider();
    }
 
}
