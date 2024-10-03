namespace DevFileChangeServer
{
    public class EventBus
    {
        public event EventHandler<EventArgs>? FileChanged;

        public void OnFileChanged(object sender)
        {
            FileChanged?.Invoke(sender, EventArgs.Empty);
        }
    }
}
