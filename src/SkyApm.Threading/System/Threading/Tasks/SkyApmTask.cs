namespace System.Threading.Tasks
{
    public class SkyApmTask : Task
    {
        public SkyApmTask(Action action) : base(action.WithSkyApm())
        {
        }

        public SkyApmTask(Action action, CancellationToken cancellationToken) : base(action.WithSkyApm(), cancellationToken)
        {
        }

        public SkyApmTask(Action action, TaskCreationOptions creationOptions) : base(action.WithSkyApm(), creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state) : base(action.WithSkyApm(), state)
        {
        }

        public SkyApmTask(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action.WithSkyApm(), cancellationToken, creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state, CancellationToken cancellationToken) : base(action.WithSkyApm(), state, cancellationToken)
        {
        }

        public SkyApmTask(Action<object> action, object state, TaskCreationOptions creationOptions) : base(action.WithSkyApm(), state, creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action.WithSkyApm(), state, cancellationToken, creationOptions)
        {
        }

        public static new Task Run(Action action)
        {
            return Task.Run(action.WithSkyApm());
        }

        public static new Task Run(Action action, CancellationToken cancellationToken)
        {
            return Task.Run(action.WithSkyApm(), cancellationToken);
        }

        public static new Task Run(Func<Task> function)
        {
            return Task.Run(function.WithSkyApm());
        }

        public static new Task Run(Func<Task> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.WithSkyApm(), cancellationToken);
        }

        public static new Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return Task.Run(function.WithSkyApm());
        }

        public static new Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.WithSkyApm(), cancellationToken);
        }

        public static new Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return Task.Run(function.WithSkyApm());
        }

        public static new Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.WithSkyApm(), cancellationToken);
        }
    }
}
