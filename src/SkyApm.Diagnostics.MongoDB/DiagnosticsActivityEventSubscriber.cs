using MongoDB.Driver.Core.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SkyApm.Diagnostics.MongoDB
{
    public class DiagnosticsActivityEventSubscriber : IEventSubscriber
    {
        public static DiagnosticSource diagnosticSource = new DiagnosticListener("MongoSourceListener");
        internal static readonly AssemblyName AssemblyName = typeof(DiagnosticsActivityEventSubscriber).Assembly.GetName();
        internal static readonly string ActivitySourceName = AssemblyName.Name;
        internal static readonly Version Version = AssemblyName.Version;
         
        private readonly ReflectionEventSubscriber _subscriber;
        private readonly ConcurrentDictionary<int, Activity> _activityMap = new ConcurrentDictionary<int, Activity>();

        private static readonly HashSet<string> CommandsWithCollectionNameAsValue =
            new HashSet<string>
            {
                "aggregate",
                "count",
                "distinct",
                "mapReduce",
                "geoSearch",
                "delete",
                "find",
                "killCursors",
                "findAndModify",
                "insert",
                "update",
                "create",
                "drop",
                "createIndexes",
                "listIndexes"
            };

        public DiagnosticsActivityEventSubscriber()
        { 
            _subscriber = new ReflectionEventSubscriber(this, bindingFlags: BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
            => _subscriber.TryGetEventHandler(out handler);

        private void Handle(CommandStartedEvent @event)
        {
            var collectionName = GetCollectionName(@event);
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                return;
            }
            var activity = new Activity("MongoActivity");
            if (activity == null)
            {
                return;
            }
            _activityMap.TryAdd(@event.RequestId, activity);
            diagnosticSource.StartActivity(activity, @event);
        }

        private void Handle(CommandSucceededEvent @event)
        {
            if (_activityMap.TryRemove(@event.RequestId, out var activity))
            {
                WithReplacedActivityCurrent(activity, () =>
                {
                    diagnosticSource.StopActivity(activity, @event);
                });
            }
        }

        private void Handle(CommandFailedEvent @event)
        {
            if (_activityMap.TryRemove(@event.RequestId, out var activity))
            {
                WithReplacedActivityCurrent(activity, () =>
                {                    
                    diagnosticSource.Write("MongoActivity.Failed", @event);                  
                });
            }
        }

        public static string GetCollectionName(CommandStartedEvent @event)
        {
            if (@event.CommandName == "getMore")
            {
                if (@event.Command.Contains("collection"))
                {
                    var collectionValue = @event.Command.GetValue("collection");
                    if (collectionValue.IsString)
                    {
                        return collectionValue.AsString;
                    }
                }
            }
            else if (CommandsWithCollectionNameAsValue.Contains(@event.CommandName))
            {
                var commandValue = @event.Command.GetValue(@event.CommandName);
                if (commandValue != null && commandValue.IsString)
                {
                    return commandValue.AsString;
                }
            }

            return null;
        }

        private static void WithReplacedActivityCurrent(Activity activity, Action action)
        {
            var current = Activity.Current;
            try
            {
                Activity.Current = activity;
                action();
            }
            finally
            {
                Activity.Current = current;
            }
        }
    }
}
