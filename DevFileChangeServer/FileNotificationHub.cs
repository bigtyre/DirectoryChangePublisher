using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace DevFileChangeServer
{
    public class FileNotificationHub : Hub
    {
        public FileNotificationHub()
        {
        }

        public override Task OnConnectedAsync()
        {
            Debug.WriteLine("Client connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Debug.WriteLine("Client disconnected");
            return base.OnDisconnectedAsync(exception);
        }
    }

    public class FileNotificationService
    {
        private readonly IHubContext<FileNotificationHub> _hubContext;

        public FileNotificationService(EventBus eventBus, IHubContext<FileNotificationHub> hubContext)
        {
            _hubContext = hubContext;
            eventBus.FileChanged += EventBus_FileChanged;
        }

        private async void EventBus_FileChanged(object? sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("File changed. Signaling reload.");
                await _hubContext.Clients.All.SendAsync("ReloadRequested");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while signaling reload. " + ex.Message);
            }
        }
    }
}
