using DevFileChangeServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);


var config = builder.Configuration;
config.AddEnvironmentVariables();
config.AddCommandLine(args);

if (args.Any(s => s == "--help"))
{
    Console.WriteLine("Usage:");
    Console.WriteLine("--port [Port to listen on. Defaults to 5057]");
    Console.WriteLine("--directory [DIRECTORIES TO MONITOR]");
    Console.WriteLine("--siteUrl [YOUR DEVELOPMENT SITE URL]");
    Console.WriteLine("--includeFilter [Regex filter of file paths to INCLUDE. Defaults to including all files.]");
    Console.WriteLine("--excludeFilter [Regex filter of file paths to EXCLUDE. Defaults to excluding  tmp and .git directories]");
    return;
}

var services = builder.Services;

var settings = new AppSettings();
config.Bind(settings);

builder.WebHost.ConfigureKestrel(c => c.ListenAnyIP(settings.Port));



var directory = settings.Directory ?? throw new Exception("Please set monitored directory");
Console.WriteLine("Monitoring directory: " + directory);

var devSiteBase = settings.SiteUrl;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins(devSiteBase)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var options = new DirectoryWatcherOptions(
    Directories: [directory], 
    Filter: settings.IncludeFilter, 
    Exclude: settings.ExcludeFilter
);

services.AddHostedService<DirectoryWatcher>();
services.AddSignalR();
services.AddSingleton<EventBus>();
services.AddSingleton(options);
services.AddSingleton<FileNotificationService>();

// Add services to the container.

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

_ = app.Services.GetRequiredService<FileNotificationService>(); // Force the creation so the event registration is setup

// Map SignalR hubs
app.MapHub<FileNotificationHub>("/event-hub");

app.UseHttpsRedirection();

var contentRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(contentRootPath)
});

app.Run();



class AppSettings()
{
    public string? Directory { get; set; }
    public int Port { get; set; } = 5057;

    public string? SiteUrl { get; set; } = "http://bigtyre8.bigtyre.local:8082";
    public string IncludeFilter { get; set; }  = ".*";

    public string ExcludeFilter { get; set; } = "tmp|\\.git";
};