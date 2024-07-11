namespace TryRx;

internal class RxFsEventsMultiSubscriber : IObservable<FileSystemEventArgs>
{
    private readonly object _sync = new();
    private readonly List<Subscription> _subscribers = new();
    private readonly FileSystemWatcher _watcher;

    public RxFsEventsMultiSubscriber(string folder)
    {
        _watcher = new FileSystemWatcher(folder);

        _watcher.Created += SendEventToObservers;
        _watcher.Changed += SendEventToObservers;
        _watcher.Renamed += SendEventToObservers;
        _watcher.Deleted += SendEventToObservers;

        _watcher.Error += SendErrorToObservers;

    }

    public IDisposable Subscribe(IObserver<FileSystemEventArgs> observer)
    {
        Subscription sub = new(this, observer);
        lock (_sync)
        {
            _subscribers.Add(sub);

            if (_subscribers.Count == 1)
            {
                _watcher.EnableRaisingEvents = true;
            }
        }
        return sub;
    }

    private void Unsubscribe(Subscription sub)
    {
        lock (_sync)
        {
            _subscribers.Remove(sub);
            if (_subscribers.Count == 0)
            {
                _watcher.EnableRaisingEvents = false;
            }
        }
    }

    void SendEventToObservers(object _, FileSystemEventArgs e)
    {
        lock (_sync) {
            foreach (var sub in _subscribers)
            {
                sub.Observer.OnNext(e);
            }
        }
    }

    void SendErrorToObservers(object _, ErrorEventArgs e)
    {
        var ex = e.GetException();
        lock (_sync)
        {
            foreach(var sub in _subscribers)
            {
                sub.Observer.OnError(ex);
            }

            _subscribers.Clear();
        }
    }

    private class Subscription : IDisposable
    {
        private RxFsEventsMultiSubscriber? _parent;

        public Subscription(RxFsEventsMultiSubscriber? parent,
            IObserver<FileSystemEventArgs> observer)
        {
            _parent = parent;
            Observer = observer;
        }

        public IObserver<FileSystemEventArgs> Observer { get; }


        public void Dispose()
        {
            _parent?.Unsubscribe(this);
            _parent = null;
        }
    }
}
