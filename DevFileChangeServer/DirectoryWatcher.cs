using System.Text.RegularExpressions;

namespace DevFileChangeServer
{
    public record DirectoryWatcherOptions(
        List<string> Directories,
        string Filter = @".*",
        string Exclude = @"\\tmp|\.git\\"
    );

    public class DirectoryWatcher(DirectoryWatcherOptions options, EventBus eventBus) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var pathToMonitor in options.Directories)
            {
                if (Directory.Exists(pathToMonitor) is false)
                {
                    Console.WriteLine($"Directory '{pathToMonitor}' does not exist. It will not be monitored.");
                    continue;
                }

                _ = MonitorDirectoryUntilCancelled(pathToMonitor, stoppingToken);
            }

            // Keep the application running until 'q' is pressed
            await Task.Delay(-1, stoppingToken);
        }

        private async Task MonitorDirectoryUntilCancelled(string pathToMonitor, CancellationToken stoppingToken)
        {
            using var watcher = new FileSystemWatcher(pathToMonitor);

            // Monitor all files and directories recursively
            watcher.IncludeSubdirectories = true;

            // Raise events for changes to files or directories
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

            // Subscribe to the event handlers
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            // Start monitoring
            watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Monitoring changes in: {pathToMonitor}");

            await Task.Delay(-1, stoppingToken);
        }

        // Define what is done when a file is changed, created, or deleted
        void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File {e.ChangeType}: {e.FullPath}");
            NotifyFileChanged(e.FullPath);
        }

        // Define what is done when a file is renamed
        void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"File Renamed: {e.OldFullPath} -> {e.FullPath}");
            NotifyFileChanged(e.FullPath);
        }

        private void NotifyFileChanged(string path)
        {
            if (Regex.IsMatch(path, options.Filter) is false)
            {
                Console.WriteLine($"File is ignored. Skipping sending notification.");
                return;
            }

            if (Regex.IsMatch(path, options.Exclude))
            {
                Console.WriteLine($"File is ignored. Skipping sending notification.");
                return;
            }

            Console.WriteLine($"Sending change notification.");
            eventBus.OnFileChanged(this);
        }
    }
}
