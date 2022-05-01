namespace System.Threading
{
    public static class SkyApmThreadPool
    {
        public static bool QueueUserWorkItem(WaitCallback callBack)
        {
            return ThreadPool.QueueUserWorkItem(callBack.WithSkyApm());
        }

        public static bool QueueUserWorkItem(WaitCallback callBack, object state)
        {
            return ThreadPool.QueueUserWorkItem(callBack.WithSkyApm(), state);
        }

        public static bool QueueUserWorkItem<TState>(Action<TState> callBack, TState state, bool preferLocal)
        {
            return ThreadPool.QueueUserWorkItem(callBack.WithSkyApm(), state, preferLocal);
        }

        public static bool UnsafeQueueUserWorkItem(IThreadPoolWorkItem callBack, bool preferLocal)
        {
            return ThreadPool.UnsafeQueueUserWorkItem(new SkyApmThreadPoolWorkItem(callBack), preferLocal);
        }

        public static bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state)
        {
            return ThreadPool.UnsafeQueueUserWorkItem(callBack.WithSkyApm(), state);
        }

        public static bool UnsafeQueueUserWorkItem<TState>(Action<TState> callBack, TState state, bool preferLocal)
        {
            return ThreadPool.UnsafeQueueUserWorkItem(callBack.WithSkyApm(), state, preferLocal);
        }
    }
}
