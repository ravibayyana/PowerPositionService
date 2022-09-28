using PowerPositionService;
using PowerPositionService.FileUtil;
using PowerPositionService.Logger;
using PowerPositionService.Service;
using PowerPositionService.Setttings;

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options => { options.ServiceName = "PowerPositionService"; })
    .ConfigureServices(services =>
    {
        services.AddSingleton<ICustomLogger, CustomLogger>();
        services.AddSingleton<IWriteToCsvFile, WriteToCsvFile>();
        services.AddSingleton<IPositionService, PositionService>();
        services.AddSingleton<IAppConfigSettings, AppConfigSettings>();

        services.AddHostedService<PowerPositionBackgroundService>();
    })
    .Build();

//.ConfigureLogging((context, loggingBuilder) =>
//{
//    //var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
//    //var fileFormat = $"PowerPositionServicelog_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.log";
//    //var logFile = Path.Combine(Path.GetTempPath(), "Petroineos", "Logs", fileFormat);

//    //var logger = new LoggerConfiguration()
//    //    .WriteTo.File(logFile, outputTemplate: outputTemplate)
//    //    .CreateLogger();

//    //loggingBuilder.AddConsole();
//    //loggingBuilder.AddSerilog(logger);
//})


await host.RunAsync();