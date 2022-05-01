using System.Globalization;

namespace System.Threading
{
    public class SkyApmThread
    {
        private readonly Thread _thread;

        public SkyApmThread(ParameterizedThreadStart start)
        {
            _thread = new Thread(start.WithSkyApm());
        }

        public SkyApmThread(ParameterizedThreadStart start, int maxStackSize)
        {
            _thread = new Thread(start.WithSkyApm(), maxStackSize);
        }

        public SkyApmThread(ThreadStart start)
        {
            _thread = new Thread(start.WithSkyApm());
        }

        public SkyApmThread(ThreadStart start, int maxStackSize)
        {
            _thread = new Thread(start.WithSkyApm(), maxStackSize);
        }

        [Obsolete("The ApartmentState property has been deprecated.  Use GetApartmentState, SetApartmentState or TrySetApartmentState instead.", false)]
        public ApartmentState ApartmentState
        {
            get => _thread.ApartmentState;
            set => _thread.ApartmentState = value;
        }

        public CultureInfo CurrentCulture
        {
            get => _thread.CurrentCulture;
            set => _thread.CurrentCulture = value;
        }

        public CultureInfo CurrentUICulture
        {
            get => _thread.CurrentCulture;
            set => _thread.CurrentCulture = value;
        }

        public ExecutionContext ExecutionContext => _thread.ExecutionContext;

        public bool IsAlive => _thread.IsAlive;

        public bool IsBackground
        {
            get => _thread.IsBackground;
            set => _thread.IsBackground = value;
        }

        public bool IsThreadPoolThread => _thread.IsThreadPoolThread;

        public int ManagedThreadId => _thread.ManagedThreadId;

        public string Name
        {
            get => _thread.Name;
            set => _thread.Name = value;
        }

        public ThreadPriority Priority
        {
            get => _thread.Priority;
            set => _thread.Priority = value;
        }

        public ThreadState ThreadState => _thread.ThreadState;

        public void Abort()
        {
            _thread.Abort();
        }

        public void Abort(object stateInfo)
        {
            _thread.Abort(stateInfo);
        }

        public void DisableComObjectEagerCleanup()
        {
            _thread.DisableComObjectEagerCleanup();
        }

        public ApartmentState GetApartmentState()
        {
            return _thread.GetApartmentState();
        }

        [Obsolete("Thread.GetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
        public CompressedStack GetCompressedStack()
        {
            return _thread.GetCompressedStack();
        }

        public override int GetHashCode()
        {
            return _thread.GetHashCode();
        }

        public void Interrupt()
        {
            _thread.Interrupt();
        }

        public void Join()
        {
            _thread.Join();
        }

        public bool Join(int millisecondsTimeout)
        {
            return _thread.Join(millisecondsTimeout);
        }

        public bool Join(TimeSpan timeout)
        {
            return _thread.Join(timeout);
        }

        [Obsolete("Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  https://go.microsoft.com/fwlink/?linkid=14202", false)]
        public void Resume()
        {
            _thread.Resume();
        }

        public void SetApartmentState(ApartmentState state)
        {
            _thread.SetApartmentState(state);
        }

        [Obsolete("Thread.SetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
        public void SetCompressedStack(CompressedStack stack)
        {
            _thread.SetCompressedStack(stack);
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Start(object parameter)
        {
            _thread.Start(parameter);
        }

        [Obsolete("Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  https://go.microsoft.com/fwlink/?linkid=14202", false)]
        public void Suspend()
        {
            _thread.Suspend();
        }

        public bool TrySetApartmentState(ApartmentState state)
        {
            return _thread.TrySetApartmentState(state);
        }
    }
}
