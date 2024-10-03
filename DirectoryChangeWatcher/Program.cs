var pathToMonitor = @"C:\xampp-7-4\htdocs\bigtyre"; // Change this to the directory you want to monitor.

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
Console.WriteLine("Press 'q' to quit.");

// Keep the application running until 'q' is pressed
while (Console.Read() != 'q') ;

// Define what is done when a file is changed, created, or deleted
static void OnChanged(object sender, FileSystemEventArgs e)
{
    Console.WriteLine($"File {e.ChangeType}: {e.FullPath}");
}

// Define what is done when a file is renamed
static void OnRenamed(object sender, RenamedEventArgs e)
{
    Console.WriteLine($"File Renamed: {e.OldFullPath} -> {e.FullPath}");
}