using System.Threading.Tasks;

namespace System.Threading
{
    public class SkyApmTimer : MarshalByRefObject, IAsyncDisposable, IDisposable
    {
        private readonly Timer _timer;

        public SkyApmTimer(TimerCallback callback)
        {
            _timer = new Timer(callback.WithSkyApm());
        }

        public SkyApmTimer(TimerCallback callback, object state, int dueTime, int period)
        {
            _timer = new Timer(callback.WithSkyApm(), state, dueTime, period);
        }
        
        public SkyApmTimer(TimerCallback callback, object state, long dueTime, long period)
        {
            _timer = new Timer(callback.WithSkyApm(), state, dueTime, period);
        }

        public SkyApmTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            _timer = new Timer(callback.WithSkyApm(), state, dueTime, period);
        }

        [CLSCompliant(false)]
        public SkyApmTimer(TimerCallback callback, object state, uint dueTime, uint period)
        {
            _timer = new Timer(callback.WithSkyApm(), state, dueTime, period);
        }

        public bool Change(int dueTime, int period)
        {
            return _timer.Change(dueTime, period);
        }

        public bool Change(long dueTime, long period)
        {
            return _timer.Change(dueTime, period);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            return _timer.Change(dueTime, period);
        }

        [CLSCompliant(false)]
        public bool Change(uint dueTime, uint period)
        {
            return _timer.Change(dueTime, period);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public bool Dispose(WaitHandle notifyObject)
        {
            return _timer.Dispose(notifyObject);
        }

        public ValueTask DisposeAsync()
        {
            return _timer.DisposeAsync();
        }
    }
}
