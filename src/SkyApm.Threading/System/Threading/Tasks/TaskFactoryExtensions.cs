namespace System.Threading.Tasks
{
    public static class TaskFactoryExtensions
    {
        public static Task StartSkyApmNew(this TaskFactory factory, Action action)
        {
            return factory.StartNew(action.WithSkyApm());
        }

        public static Task StartSkyApmNew(this TaskFactory factory, Action action, CancellationToken cancellationToken)
        {
            return factory.StartNew(action.WithSkyApm(), cancellationToken);
        }

        public static Task StartSkyApmNew(this TaskFactory factory, Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.StartNew(action.WithSkyApm(), cancellationToken, creationOptions, scheduler);
        }

        public static Task StartSkyApmNew(this TaskFactory factory, Action action, TaskCreationOptions creationOptions)
        {
            return factory.StartNew(action.WithSkyApm(), creationOptions);
        }

        public static Task StartSkyApmNew(this TaskFactory factory, Action<object> action, object state)
        {
            return factory.StartNew(action.WithSkyApm(), state);
        }

        public static Task StartSkyApmNew(this TaskFactory factory, Action<object> action, object state, CancellationToken cancellationToken)
        {
            return factory.StartNew(action.WithSkyApm(), state, cancellationToken);
        }

        public static Task StartSkyApmNew(this TaskFactory factory, Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.StartNew(action.WithSkyApm(), state, cancellationToken, creationOptions, scheduler);
        }

        public static Task StartSkyApmNew(this TaskFactory factory, Action<object> action, object state, TaskCreationOptions creationOptions)
        {
            return factory.StartNew(action.WithSkyApm(), state, creationOptions);
        }

        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<object, TResult> function, object state)
        {
            return factory.StartNew(function.WithSkyApm(), state);
        }

        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<object, TResult> function, object state, CancellationToken cancellationToken)
        {
            return factory.StartNew(function.WithSkyApm(), state, cancellationToken);
        }

        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.StartNew(function.WithSkyApm(), state, cancellationToken, creationOptions, scheduler);
        }

        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            return factory.StartNew(function.WithSkyApm(), state, creationOptions);
        }

        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<TResult> function)
        {
            return factory.StartNew(function.WithSkyApm());
        }

        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<TResult> function, CancellationToken cancellationToken)
        {
            return factory.StartNew(function.WithSkyApm(), cancellationToken);
        }

        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.StartNew(function.WithSkyApm(), cancellationToken, creationOptions, scheduler);
        }

        public static Task<TResult> StartSkyApmNew<TResult>(this TaskFactory factory, Func<TResult> function, TaskCreationOptions creationOptions)
        {
            return factory.StartNew(function.WithSkyApm(), creationOptions);
        }
    }
}
