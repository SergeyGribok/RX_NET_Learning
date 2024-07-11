namespace TryRx;

internal class RxFsEvents : IObservable<FileSystemEventArgs>
{
    private readonly string _folder;

    public RxFsEvents(string folder)
    {
        _folder = folder;
    }

    public IDisposable Subscribe(IObserver<FileSystemEventArgs> observer)
    {
        FileSystemWatcher watcher = new(_folder);
        object sync = new();
        bool onErrorAlreadyCalled = false;

        void SendToObserver(object _, FileSystemEventArgs e)
        {
            lock (sync)
            {
                if (!onErrorAlreadyCalled)
                {
                    observer.OnNext(e);
                }
            }
        }

        watcher.Created += SendToObserver;
        watcher.Changed += SendToObserver;
        watcher.Renamed += SendToObserver;
        watcher.Deleted += SendToObserver;

        watcher.Error += (_, e) =>
        {
            lock (sync)
            {
                if (!onErrorAlreadyCalled)
                {
                    observer.OnError(e.GetException());
                    onErrorAlreadyCalled = true;
                    watcher.Dispose();
                }
            }
        };

        watcher.EnableRaisingEvents = true;

        return watcher;
    }
}
